using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Infrastructure;
using OrderApi.Models;
using RestSharp;

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
                    Thread.Sleep(500);
                }

                return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            }
            catch
            {
                return StatusCode(500, "The order could not be created. Please try again.");
            }


            //// Call ProductApi to get the product ordered
            //// You may need to change the port number in the BaseUrl below
            //// before you can run the request.
            //RestClient c = new RestClient("https://localhost:5001/products/");
            //var productIdList = order.OrderLines.Select(ol => ol.ProductId);
            //var json = JsonSerializer.Serialize(productIdList);
            //var request = new RestRequest(json);
            //var response = c.GetAsync<List<Product>>(request);
            //response.Wait();
            //var orderedProduct = response.Result;

            //RestClient cc = new RestClient("https://localhost:5005/customers/");
            //var request2 = new RestRequest(order.CustomerId.ToString());
            //var response2 = cc.GetAsync<Customer>(request2);
            //response2.Wait();
            //var intendedCustomer = response2.Result;



            //if (intendedCustomer is Customer && intendedCustomer.GoodCreditStanding == true)
            //{
            //    bool orderFailed = false;
            //    var productsToUpdate = new List<Product>();
            //    foreach (var orderLine in order.OrderLines)
            //    {
            //        if (orderFailed == true)
            //        {
            //            break;
            //        }
            //        foreach (var product in orderedProduct)
            //        {
            //            if (orderLine.ProductId == product.Id)
            //            {
            //                if (orderLine.NoOfItems <= product.ItemsInStock - product.ItemsReserved)
            //                {
            //                    product.ItemsReserved += orderLine.NoOfItems;

            //                    productsToUpdate.Add(product);
            //                }
            //                else
            //                {
            //                    orderFailed = true;
            //                    break;
            //                }
            //            }
            //        }
            //        if (order.OrderLines.IndexOf(orderLine) == order.OrderLines.Count - 1)
            //        {
            //            if (productsToUpdate.Count != 0)
            //            {
            //                foreach (var product in productsToUpdate)
            //                {
            //                    var updateRequest = new RestRequest(product.Id.ToString());
            //                    updateRequest.AddJsonBody(product);

            //                    // make temp list and send in batch to be updated
            //                    var updateResponse = c.PutAsync(updateRequest);
            //                    updateResponse.Wait();
            //                }
            //                var newOrder = repository.Add(order);
            //                return CreatedAtRoute("GetOrder",
            //                    new { id = newOrder.Id }, newOrder);
            //            }
            //        }
            //    }
            //}

            ////If the order could not be created, "return no content".
            //return NoContent();
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
            throw new NotImplementedException();

            // Add code to implement this method.
        }

    }
}
