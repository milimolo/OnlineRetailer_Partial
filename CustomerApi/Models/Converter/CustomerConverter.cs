using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Models.Converter
{
    public class CustomerConverter : IConverter<Customer, CustomerDto>
    {
        public Customer Convert(CustomerDto sharedCust)
        {
            return new Customer
            {
                Id = sharedCust.Id,
                Name = sharedCust.Name,
                Email = sharedCust.Email,
                Phone = sharedCust.Phone,
                BillingAddress = sharedCust.BillingAddress,
                ShippingAddress = sharedCust.ShippingAddress,
                GoodCreditStanding = sharedCust.GoodCreditStanding
            };
        }

        public CustomerDto Convert(Customer hiddenCust)
        {
            return new CustomerDto
            {
                Id = hiddenCust.Id,
                Name = hiddenCust.Name,
                Email = hiddenCust.Email,
                Phone = hiddenCust.Phone,
                BillingAddress = hiddenCust.BillingAddress,
                ShippingAddress = hiddenCust.ShippingAddress,
                GoodCreditStanding = hiddenCust.GoodCreditStanding
            };
        }
    }
}
