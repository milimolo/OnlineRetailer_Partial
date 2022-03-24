﻿using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using SharedModels;

namespace OrderApi.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderApiContext db;

        public OrderRepository(OrderApiContext context)
        {
            db = context;
        }

        public IEnumerable<Order> GetByCustomer(int customerId)
        {
            var ordersForCustomer = from o in db.Orders
                                    where o.CustomerId == customerId
                                    select o;

            return ordersForCustomer.ToList();
        }

        Order IRepository<Order>.Add(Order entity)
        {
            if (entity.Date == null)
                entity.Date = DateTime.Now;
            
            var newOrder = db.Orders.Add(entity).Entity;
            db.SaveChanges();

            foreach (var ol in newOrder.OrderLines)
            {
                ol.Order = null;
            };

            return newOrder;
        }

        void IRepository<Order>.Edit(Order entity)
        {
            db.Entry(entity).State = EntityState.Modified;
            db.SaveChanges();
        }

        Order IRepository<Order>.Get(int id)
        {
            var order = db.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefault(o => o.Id == id);

            foreach (var ol in order.OrderLines)
            {
                ol.Order = null;
            };

            return order;
        }

        IEnumerable<Order> IRepository<Order>.GetAll()
        {
            return db.Orders.ToList();
        }

        void IRepository<Order>.Remove(int id)
        {
            var order = db.Orders.FirstOrDefault(p => p.Id == id);
            db.Orders.Remove(order);
            db.SaveChanges();
        }
    }
}
