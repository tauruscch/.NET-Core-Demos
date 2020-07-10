using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace OptionsDemo.Services
{
    public interface IOrderService
    {
        int ShowMaxOrderCount();
    }

    /// <summary>
    /// 1. 基本使用方式，通过 IOptions<OrderOptions> 定义我们的配置选项，再通过 services.Configure<OrderOptions>(Configuration.GetSection("OrderService")) 注入选项配置，使用时通过 _options.Value 获取配置对象
    /// 2. 使用 IOptionsSnapshot<OrderOptions> 定义我们的配置选项，就能实现配置的热更新,只能用于暂时和作用域实例
    /// 3. 使用 IOptionsMonitor<OrderOptions> 实现单例对象的配置热更新，必须使用 _options.CurrentValue 获取配置对象
    /// 4. 通过 _options.OnChange 监听配置的变更
    /// </summary>
    public class OrderService:IOrderService
    {
        IOptionsMonitor<OrderOptions> _options;

        public OrderService(IOptionsMonitor<OrderOptions> options)
        {
            this._options = options;

            //_options.OnChange(options =>
            //{
            //    Console.WriteLine($"配置发生了变更：{options.MaxOrderCount}");
            //});
        }

        public int ShowMaxOrderCount()
        {
            return _options.CurrentValue.MaxOrderCount;
        }
    }

    public class OrderOptions
    {
        [Range(30, 100)]
        public int MaxOrderCount { get; set; } = 100;
    }

    
}
