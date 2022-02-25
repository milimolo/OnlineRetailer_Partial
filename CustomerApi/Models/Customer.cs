using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public int Phone { get; set; }
        public Address BillingAddress { get; set; }
        public Address ShippingAddress { get; set; }
        public bool GoodCreditStanding { get; set; }
    }
}
