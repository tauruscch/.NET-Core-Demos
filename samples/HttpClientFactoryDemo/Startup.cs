using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using HttpClientFactoryDemo.Clients;
using System.Net.Http;
using HttpClientFactoryDemo.DelegatingHandlers;
using Microsoft.AspNetCore.Http;

namespace HttpClientFactoryDemo
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddControllersAsServices();

            // 工厂模式
            services.AddHttpClient();
            services.AddScoped<OrderServiceClient>();

            // 命令客户端模式
            services.AddSingleton<RequestIdDelegatingHandler>();
            services.AddHttpClient("NamedOrderServiceClient", client =>
            {
                client.DefaultRequestHeaders.Add("client-name", "namedclient");
                client.BaseAddress = new Uri("https://localhost:5003");
            }).SetHandlerLifetime(TimeSpan.FromMinutes(20))
            .AddHttpMessageHandler(provider => provider.GetService<RequestIdDelegatingHandler>()); // 增加自定义消息处理中间件
            services.AddScoped<NamedOrderServiceClient>();

            // 类型化客户端模式
            services.AddHttpClient<TypedOrderServiceClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:5003");
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
