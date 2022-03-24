using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedModels.Messages
{
    public class OrderPayRejectedMessage
    {
        public int OrderId { get; set; }
    }
}
