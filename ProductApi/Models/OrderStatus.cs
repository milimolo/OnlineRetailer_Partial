using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductApi.Models
{
    public enum OrderStatus
    {
        Completed,
        Cancelled,
        Shipped,
        Paid
    }
}
