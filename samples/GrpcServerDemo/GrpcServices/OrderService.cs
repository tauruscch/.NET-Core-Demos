using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcServices;
using Microsoft.Extensions.Logging;

namespace GrpcServerDemo.GrpcServices
{
    public class OrderService : OrderGrpc.OrderGrpcBase
    {
        public override Task<CreateOrderResult> CreateOrder(CreateOrderCommand request, ServerCallContext context)
        {

            //throw new System.Exception("order error");

            //��Ӵ����������ڲ��߼���¼�뽫������Ϣ�洢�����ݿ�
            return Task.FromResult(new CreateOrderResult { OrderId = 24 });
        }
    }
}
