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

            List<Customer> customers = new List<Customer>
            {
                new Customer { Name = "Paul", Email = "Paul-walker@gmail.com", Phone = 12345678, BillingAddress = "Walkerstreet 5", 
                    ShippingAddress = "Workerstreet 32", GoodCreditStanding = true}
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
