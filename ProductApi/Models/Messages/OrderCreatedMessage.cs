using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Models.Messages
{
    public class OrderCreatedMessage
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public IList<OrderLine> OrderLines { get; set; }
    }
}
