using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string Street { get; set; }
        public int StreetNum { get; set; }
        public int Zipcode { get; set; }
        public string City { get; set; }
    }
}
