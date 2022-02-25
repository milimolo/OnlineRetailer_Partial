using System;
namespace OrderApi.Models
{
    public class Product
    {
        // Order doens't need to know everything
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int ItemsInStock { get; set; }
        public int ItemsReserved { get; set; }
    }
}
