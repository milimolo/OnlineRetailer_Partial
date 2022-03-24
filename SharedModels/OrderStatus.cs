using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedModels
{
    public enum OrderStatus
    {
        Tentative,
        Completed,
        Cancelled,
        Shipped,
        Paid
    }
}
