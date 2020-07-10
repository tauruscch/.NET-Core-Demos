using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OptionsDemo.Services
{
    public static class OrderServiceExtensions
    {
        public static IServiceCollection AddOrderService(this IServiceCollection services, IConfiguration configuration)
        {
            /* 1. 提取扩展方法，注入不同的服务类
             * 2. 读取配置后，使用 services.PostConfigure<OrderOptions> 动态更新配置
             * 
             * 实现配置参数的校验：
             * 1. 要实现校验，必须通过 services.AddOptions<OrderOptions>().Configure(options =>{configuration.Bind(options);}) 方式绑定选项参数，然后通过 Validate 实现校验
             * 2. 通过 ValidateDataAnnotations 实现属性校验的方式
             * 3. 通过实现 IValidateOptions<OrderOptions> 接口进行校验
             */

            //services.Configure<OrderOptions>(configuration);

            //services.AddOptions<OrderOptions>().Configure(options =>
            //{
            //    configuration.Bind(options);
            //}).Validate(options =>
            //{
            //    return options.MaxOrderCount <= 100;
            //}, "MaxOrderCount 不能大于100");

            //services.AddOptions<OrderOptions>().Configure(options =>
            //{
            //    configuration.Bind(options);
            //}).ValidateDataAnnotations();

            services.AddOptions<OrderOptions>().Configure(options =>
            {
                configuration.Bind(options);
            }).Services.AddSingleton<IValidateOptions<OrderOptions>>(new OrderServiceValidateOptions());

            //services.PostConfigure<OrderOptions>(options =>
            //{
            //    options.MaxOrderCount += 100;
            //});


            services.AddTransient<IOrderService, OrderService>();
            return services;
        }
    }
}
