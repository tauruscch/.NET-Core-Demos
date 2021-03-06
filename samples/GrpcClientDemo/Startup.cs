﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using GrpcServices;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using Polly.Bulkhead;
using Polly.CircuitBreaker;
using static GrpcServices.OrderGrpc;

namespace GrpcClientDemo
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // 如果需要允许使用不加密的HTTP/2协议，则启用如下代码
            //AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true); 

            // 1. 通过AddGrpcClient注入grpc客户端服务。默认情况下，接口地址必须要是https，并且是HTTP/2协议的，否则不能调用
            services.AddGrpcClient<OrderGrpcClient>(options =>
            {
                options.Address = new Uri("https://localhost:5001"); // https的HTTP/2的接口，直接可以调用。如果server端使用得是无效的自签名证书，则需要通过ConfigurePrimaryHttpMessageHandler配置允许无效签名证书的SocketsHttpHandler
                //options.Address = new Uri("http://localhost:5002"); // 非https的HTTP/2的接口，需要启用支持未加密的HTTP/2
            })
            .ConfigurePrimaryHttpMessageHandler(provider =>
            {
                var handler = new SocketsHttpHandler();
                handler.SslOptions.RemoteCertificateValidationCallback = (a, b, c, d) => true; // 允许无效、或自签名证书
                return handler;
            })
            .AddTransientHttpErrorPolicy(p => p.WaitAndRetryForeverAsync(i => TimeSpan.FromSeconds(i * 3))); // 为httpclient添加瞬时的失败重试策略，当抛出HttpRequestException异常，或者返回状态500、408时触发

            // 1. 添加策略
            var reg = services.AddPolicyRegistry();
            reg.Add("retryforever", Policy.HandleResult<HttpResponseMessage>(message =>
            {
                return message.StatusCode == System.Net.HttpStatusCode.Created;
            }).RetryForeverAsync());

            // 2. 使用策略
            services.AddHttpClient("orderclient").AddPolicyHandlerFromRegistry("retryforever");
            services.AddHttpClient("orderclientv2").AddPolicyHandlerFromRegistry((registry, message) =>
            {
                // get请求，则使用retryforever策略，否则不使用策略
                return message.Method == HttpMethod.Get ? registry.Get<IAsyncPolicy<HttpResponseMessage>>("retryforever") : Policy.NoOpAsync<HttpResponseMessage>();
            });

            // 使用熔断
            services.AddHttpClient("orderclientv3").AddPolicyHandler(Policy<HttpResponseMessage>.Handle<HttpRequestException>().CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 10, // 失败10次就熔断
                durationOfBreak: TimeSpan.FromSeconds(10), // 熔断10秒
                onBreak: (r, t) => { }, // 熔断开启时触发的事件
                onReset: () => { }, // 熔断恢复时触发的事件
                onHalfOpen: () => { } // 尝试用部分流量去验证服务是否可用的事件
                ));

            // 使用高级熔断
            services.AddHttpClient("orderclientv3").AddPolicyHandler(Policy<HttpResponseMessage>.Handle<HttpRequestException>().AdvancedCircuitBreakerAsync(
                failureThreshold: 0.8, // 失败的比例
                samplingDuration: TimeSpan.FromSeconds(10), // 采样时间，表示10秒内有80%失败就触发熔断
                minimumThroughput: 100, // 10秒内请求量超过100个
                durationOfBreak: TimeSpan.FromSeconds(20), // 熔断时长
                onBreak: (r, t) => { },
                onReset: () => { },
                onHalfOpen: () => { }));


            // 组合策略，熔断后重试3次，3次都失败，则降级响应返回默认数据
            var breakPolicy = Policy<HttpResponseMessage>.Handle<HttpRequestException>().AdvancedCircuitBreakerAsync(
                failureThreshold: 0.8,
                samplingDuration: TimeSpan.FromSeconds(10),
                minimumThroughput: 100,
                durationOfBreak: TimeSpan.FromSeconds(20),
                onBreak: (r, t) => { },
                onReset: () => { },
                onHalfOpen: () => { });
            var message = new HttpResponseMessage()
            {
                Content = new StringContent("{}")
            };
            // BrokenCircuitException 为熔断后抛出的异常
            var fallback = Policy<HttpResponseMessage>.Handle<BrokenCircuitException>().FallbackAsync(message);
            var retry = Policy<HttpResponseMessage>.Handle<Exception>().WaitAndRetryAsync(3, i => TimeSpan.FromSeconds(1));
            var fallbackBreak = Policy.WrapAsync(fallback, retry, breakPolicy);
            services.AddHttpClient("httpv3").AddPolicyHandler(fallbackBreak);

            // 限流策略
            var bulk = Policy.BulkheadAsync<HttpResponseMessage>(
                maxParallelization: 30, // 并发30
                maxQueuingActions: 20, // 并发超过30时，还可以有20个进行排队
                onBulkheadRejectedAsync: contxt => Task.CompletedTask // 请求被限流时触发的事件
                );

            // 组合限流和降级响应
            var message2 = new HttpResponseMessage()
            {
                Content = new StringContent("{}")
            };
            var fallback2 = Policy<HttpResponseMessage>.Handle<BulkheadRejectedException>().FallbackAsync(message);
            var fallbackbulk = Policy.WrapAsync(fallback2, bulk);
            services.AddHttpClient("httpv4").AddPolicyHandler(fallbackbulk);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    // 2. 获取grpc客户端服务，并调用接口
                    OrderGrpcClient service = context.RequestServices.GetService<OrderGrpcClient>();
                    try
                    {
                        var r = service.CreateOrder(new CreateOrderCommand { BuyerId = "abc" });
                    }
                    catch (Exception ex)
                    {
                    }

                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
