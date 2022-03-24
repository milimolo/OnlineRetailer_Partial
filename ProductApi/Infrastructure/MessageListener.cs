using EasyNetQ;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using ProductApi.Models;
using SharedModels;
using SharedModels.Messages;
using System;
using System.Collections.Generic;
using System.Threading;

namespace ProductApi.Infrastructure
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
                bus.PubSub.Subscribe<OrderCreatedMessage>("productApiCreated", HandleOrderCreated);

                // Block the thread so that it will not exit and stop subscribing.
                lock (this)
                {
                    Monitor.Wait(this);
                }
            }
        }

        private void HandleOrderCreated(OrderCreatedMessage message)
        {
            using(var scope = _provider.CreateScope())
            {
                var services = scope.ServiceProvider;
                var productRepos = services.GetService<IRepository<Product>>();

                if(ProductItemsAvailable(message.OrderLines, productRepos))
                {
                    // Reserve items and publish an OrderAcceptedMessage
                    foreach (var ol in message.OrderLines)
                    {
                        var product = productRepos.Get(ol.ProductId);
                        product.ItemsReserved += ol.NoOfItems;
                        productRepos.Edit(product);
                    }

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

        private bool ProductItemsAvailable(IList<OrderLine> orderLines, IRepository<Product> productRepos)
        {
            foreach (var ol in orderLines)
            {
                var product = productRepos.Get(ol.ProductId);
                if(ol.NoOfItems > product.ItemsInStock - product.ItemsReserved)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
