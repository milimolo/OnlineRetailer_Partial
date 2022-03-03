using CustomerApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Customers
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Order> orders = new List<Order>
            {
                new Order { Date = DateTime.Today, ProductId = 1, Quantity = 2 }
            };

            List<Customer> customers = new List<Customer>
            {
                new Customer { Name = "Paul", Email = "Paul-walker@gmail.com", Phone = 12345678, BillingAddress = "Walkerstreet 5", 
                    ShippingAddress = "Workerstreet 32", GoodCreditStanding = true, Orders = orders}
            };

            CustomerDto customer = new CustomerDto
            {
                Id = customers[1].Id,
                Name = customers[1].Name,
                Phone = customers[1].Phone
            };

            orders[1].Customer = customer;

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
