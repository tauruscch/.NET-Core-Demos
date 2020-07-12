using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MediatRDemo
{
    class Program
    {
        async static Task Main(string[] args)
        {
            var services = new ServiceCollection();

            // 注入消息类，MediatR会扫描指定程序集下的消息/消息处理器
            services.AddMediatR(Assembly.GetExecutingAssembly());

            var serviceProvider = services.BuildServiceProvider();

            var mediator = serviceProvider.GetService<IMediator>();

            //await mediator.Send(new MyCommand { CommandName = "cmd01" });

            await mediator.Publish(new MyEvent { EventName = "event01" });

            Console.ReadLine();
        }

        #region 单播消息，只有一个处理器，后注册的处理器会覆盖之前的
        /// <summary>
        /// 定义消息，指定返回值类型为long
        /// </summary>
        internal class MyCommand : IRequest<long>
        {
            public string CommandName { get; set; }
        }

        internal class MyCommandHandler : IRequestHandler<MyCommand, long>
        {
            public Task<long> Handle(MyCommand request, CancellationToken cancellationToken)
            {
                Console.WriteLine($"MyCommandHandler执行命令：{request.CommandName}");
                return Task.FromResult(10L);
            }
        }
        #endregion

        #region 多播消息，可以用多个处理器
        internal class MyEvent : INotification
        {
            public string EventName { get; set; }
        }

        internal class MyEventHandler : INotificationHandler<MyEvent>
        {
            public Task Handle(MyEvent notification, CancellationToken cancellationToken)
            {
                Console.WriteLine($"MyEventHandler执行：{notification.EventName}");
                return Task.CompletedTask;
            }
        }

        internal class MyEventHandlerV2 : INotificationHandler<MyEvent>
        {
            public Task Handle(MyEvent notification, CancellationToken cancellationToken)
            {
                Console.WriteLine($"MyEventHandlerV2执行：{notification.EventName}");
                return Task.CompletedTask;
            }
        }

        #endregion
    }
}
