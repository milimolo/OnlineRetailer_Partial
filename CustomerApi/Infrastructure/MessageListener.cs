using CustomerApi.Data;
using CustomerApi.Models;
using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using SharedModels.Messages;
using System;
using System.Threading;

namespace CustomerApi.Infrastructure
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
            using (bus = RabbitHutch.CreateBus(_connectionString))
            {
                bus.PubSub.Subscribe<OrderCreatedMessage>("customerApiCreated", HandleOrderCreated);
                bus.PubSub.Subscribe<OrderPayMessage>("customerApiPay", HandleOrderPaid);

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderPaid(OrderPayMessage message)
        {
            using (var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customerRepos = services.GetService<IRepository<Customer>>();

                if (!CustomerHasGoodStanding(message.CustomerId, customerRepos))
                {
                    var customer = customerRepos.Get(message.CustomerId);
                    customer.GoodCreditStanding = true;
                    customerRepos.Edit(customer);

                    var replyMessage = new OrderPayAcceptedMessage
                    {
                        OrderId = message.OrderId
                    };

                    bus.PubSub.Publish(replyMessage);
                }
                else
                {
                    // Publish rejected Pay message
                    var replyMessage = new OrderPayRejectedMessage
                    {
                        OrderId = message.OrderId
                    };

                    bus.PubSub.Publish(replyMessage);
                }
            }
        }

        private void HandleOrderCreated(OrderCreatedMessage message)
        {
            using (var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var customerRepos = services.GetService<IRepository<Customer>>();

                if(CustomerHasGoodStanding(message.CustomerId, customerRepos))
                {
                    var customer = customerRepos.Get(message.CustomerId);
                    customer.GoodCreditStanding = false;
                    customerRepos.Edit(customer);

                    var replyMessage = new OrderAcceptedMessage
                    {
                        OrderId = message.OrderId
                    };

                    bus.PubSub.Publish(replyMessage);
                }
                else
                {
                    // Publish rejected Order message
                    var replyMessage = new OrderRejectedMessage
                    {
                        OrderId = message.OrderId
                    };

                    bus.PubSub.Publish(replyMessage);
                }
            }
        }

        private bool CustomerHasGoodStanding(int customerId, IRepository<Customer> customerRepos)
        {
            var customer = customerRepos.Get(customerId);
            if (customer != null && customer.GoodCreditStanding)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
