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
            /* 1. 一般服务较多，推荐提取扩展方法，对不同的服务类进行注入
             * 2. 读取配置后，使用 services.PostConfigure<OrderOptions> 动态更新配置
             * 
             * 实现配置参数的校验：
             * 1. 要实现校验，必须通过 services.AddOptions<OrderOptions>().Bind(configuration) 方式注入选项参数，然后通过 Validate 实现校验，并支持热更新。当DataAnnotations不满足时，推荐使用该方式
             * 2. 通过 ValidateDataAnnotations 实现属性校验的方式。当配置比较简单，且验证逻辑仅仅是对单个值的简单验证时使用
             * 3. 通过实现 IValidateOptions<OrderOptions> 接口进行校验。一般是验证逻辑较为复杂时，或验证逻辑依赖其它服务时使用
             */

            //services.Configure<OrderOptions>(configuration);

            //services.AddOptions<OrderOptions>().Bind(configuration)
            //    .Validate(options =>
            //    {
            //        return options.MaxOrderCount <= 100;
            //    }, "MaxOrderCount 不能大于100");

            services.AddOptions<OrderOptions>().Bind(configuration)
                .ValidateDataAnnotations();

            // 此种方式绑定选项数据，不支持热更新
            //services.AddOptions<OrderOptions>().Configure(options => { configuration.Bind(options); })
            //    .ValidateDataAnnotations();

            //services.AddOptions<OrderOptions>().Bind(configuration)
            //    .Services.AddSingleton<IValidateOptions<OrderOptions>>(new OrderServiceValidateOptions());

            //services.PostConfigure<OrderOptions>(options =>
            //{
            //    options.MaxOrderCount += 100;
            //});


            services.AddSingleton<IOrderService, OrderService>();
            return services;
        }
    }
}
