using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharedModels
{
    public class OrderLine
    {
        public int Id { get; set; }
        public int OrderID { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public int NoOfItems { get; set; }
    }
}
