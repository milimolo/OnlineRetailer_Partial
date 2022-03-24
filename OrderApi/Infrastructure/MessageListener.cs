using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using OrderApi.Data;
using SharedModels;
using SharedModels.Messages;
using System;
using System.Threading;

namespace OrderApi.Infrastructure
{
    public class MessageListener
    {
        IServiceProvider _provider;
        string _connectionString;
        IBus bus;

        public MessageListener(IServiceProvider provider, string connectionString)
        {
            _provider = provider;
            _connectionString = connectionString;
        }

        public void Start()
        {
            using(bus = RabbitHutch.CreateBus(_connectionString))
            {
                bus.PubSub.Subscribe<OrderAcceptedMessage>("orderApiAccepted", HandleOrderAccepted);
                bus.PubSub.Subscribe<OrderRejectedMessage>("orderApiRejected", HandleOrderRejected);

                bus.PubSub.Subscribe<OrderPayAcceptedMessage>("orderApiPayAccepted", HandleOrderPaidAccepted);
                bus.PubSub.Subscribe<OrderPayRejectedMessage>("orderApiPayRejected", HandleOrderPaidRejected);


                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderPaidRejected(OrderPayRejectedMessage message)
        {
            using (var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var orderRepos = services.GetService<IRepository<Order>>();

                // Mark as completed
                var order = orderRepos.Get(message.OrderId);
                order.OrderStatus = OrderStatus.Completed;
                orderRepos.Edit(order);
            }
        }

        private void HandleOrderPaidAccepted(OrderPayAcceptedMessage message)
        {
            using (var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var orderRepos = services.GetService<IRepository<Order>>();

                // Mark as completed
                var order = orderRepos.Get(message.OrderId);
                order.OrderStatus = OrderStatus.Paid;
                orderRepos.Edit(order);
            }
        }

        private void HandleOrderAccepted(OrderAcceptedMessage message)
        {
            using(var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var orderRepos = services.GetService<IRepository<Order>>();

                // Mark as completed
                var order = orderRepos.Get(message.OrderId);
                order.OrderStatus = OrderStatus.Completed;
                orderRepos.Edit(order);
            }
        }

        private void HandleOrderRejected(OrderRejectedMessage message)
        {
            using (var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var orderRepos = services.GetService<IRepository<Order>>();

                // Delete tentative order.
                orderRepos.Remove(message.OrderId);
            }
        }
    }
}
