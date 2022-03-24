using EasyNetQ;
using SharedModels;
using SharedModels.Messages;
using System.Collections.Generic;

namespace OrderApi.Infrastructure
{
    public class MessagePublisher : IMessagePublisher
    {
        readonly IBus bus;

        public MessagePublisher(string connectionString)
        {
            bus = RabbitHutch.CreateBus(connectionString);
        }

        public void Dispose()
        {
            bus.Dispose();
        }

        public void PublishOrderCreatedMessage(int customerId, int orderId, IList<OrderLine> orderLines)
        {
            var message = new OrderCreatedMessage
            {
                CustomerId = customerId,
                OrderId = orderId,
                OrderLines = orderLines
            };

            bus.PubSub.Publish(message);
        }

        public void PublishOrderPayment(int customerId, int orderId)
        {
            var message = new OrderPayMessage
            {
                CustomerId = customerId,
                OrderId = orderId
            };

            bus.PubSub.Publish(message);
        }
    }
}
