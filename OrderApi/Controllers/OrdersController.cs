using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using RestSharp;
using SharedModels;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;
        private readonly IMessagePublisher _messagePublisher;

        public OrdersController(IRepository<Order> repos, IMessagePublisher messagePublisher)
        {
            repository = repos as IOrderRepository;
            _messagePublisher = messagePublisher;
        }

        // GET: orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET orders/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return BadRequest("Could not find order.");
            }
            return new ObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest("Order is null, please try again.");
            }

            try
            {
                // Create the tentative order
                order.OrderStatus = OrderStatus.Tentative;
                var newOrder = repository.Add(order);

                // Publish message: OrderCreatedMessage
                _messagePublisher.PublishOrderCreatedMessage(
                    newOrder.CustomerId, newOrder.Id, newOrder.OrderLines);

                // Wait for orderStatus to return "Completed"
                bool completed = false;
                while (!completed)
                {
                    var tentativeOrder = repository.Get(newOrder.Id);
                    if(tentativeOrder.OrderStatus == OrderStatus.Completed)
                    {
                        completed = true;
                    }
                    Thread.Sleep(200);
                }

                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }
            catch
            {
                return StatusCode(500, "The order could not be created. Please try again.");
            }
        }

        // PUT orders/5/cancel
        // This action method cancels an order and publishes an OrderStatusChangedMessage
        // with topic set to "cancelled".
        [HttpPut("{id}/cancel")]
        public IActionResult Cancel(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/ship
        // This action method ships an order and publishes an OrderStatusChangedMessage.
        // with topic set to "shipped".
        [HttpPut("{id}/ship")]
        public IActionResult Ship(int id)
        {
            throw new NotImplementedException();

            // Add code to implement this method.
        }

        // PUT orders/5/pay
        // This action method marks an order as paid and publishes a CreditStandingChangedMessage
        // (which have not yet been implemented), if the credit standing changes.
        [HttpPut("{id}/pay")]
        public IActionResult Pay(int id)
        {
            var paidOrder = repository.Get(id);
            if(paidOrder == null)
            {
                return BadRequest("Order is null, please try again.");
            }

            try
            {
                _messagePublisher.PublishOrderPayment(paidOrder.CustomerId, paidOrder.Id);

                // Wait for orderStatus to return "Paid"
                bool completed = false;
                while (!completed)
                {
                    var tentativeOrder = repository.Get(paidOrder.Id);
                    if (tentativeOrder.OrderStatus == OrderStatus.Paid)
                    {
                        completed = true;
                    }
                    Thread.Sleep(200);
                }

                return CreatedAtRoute("GetOrder", new { id = paidOrder.Id }, paidOrder);
            }
            catch
            {
                return StatusCode(500, "The order could not be paid. Please try again.");
            }
        }

    }
}
