using SharedModels;
using System.Collections.Generic;

namespace OrderApi.Infrastructure
{
    public interface IMessagePublisher
    {
        void PublishOrderCreatedMessage(int customerId, int orderId, IList<OrderLine> orderLines);
    }
}
