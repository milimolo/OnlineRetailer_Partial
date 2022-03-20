using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Models.Messages
{
    public class OrderAcceptedMessage
    {
        public int OrderId { get; set; }
    }
}
