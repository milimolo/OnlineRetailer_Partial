﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderApi.Models
{
    public class OrderDto
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public int CustomerId { get; set; }
        public List<OrderLine> OrderLines { get; set; }
        public OrderStatus OrderStatus { get; set; }
    }
}
