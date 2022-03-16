using System.Collections.Generic;
using System.Linq;
using OrderApi.Models;
using System;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }

            var mockOrderLines = new List<OrderLine>
            {
                new OrderLine { NoOfItems = 3, OrderID = 1, ProductId = 1 }
            };

            List<Order> orders = new List<Order>
            {
                new Order { Date = DateTime.Today, OrderStatus = OrderStatus.Paid, OrderLines = mockOrderLines }
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
