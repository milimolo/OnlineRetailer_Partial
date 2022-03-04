using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using OrderApi.Models;
using RestSharp;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IRepository<Order> repository;

        public OrdersController(IRepository<Order> repos)
        {
            repository = repos;
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
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST orders
        [HttpPost]
        public IActionResult Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            // Call ProductApi to get the product ordered
            // You may need to change the port number in the BaseUrl below
            // before you can run the request.
            RestClient c = new RestClient("https://localhost:5001/products/");
            var productIdList = order.OrderLines.Select(ol => ol.ProductId);
            var json = JsonSerializer.Serialize(productIdList);
            var request = new RestRequest(json);
            var response = c.GetAsync<List<Product>>(request);
            response.Wait();
            var orderedProduct = response.Result;

            RestClient cc = new RestClient("https://localhost:5005/customers/");
            var request2 = new RestRequest(order.CustomerId.ToString());
            var response2 = cc.GetAsync<Customer>(request2);
            response2.Wait();
            var intendedCustomer = response2.Result;



            try
            {
                var isOrderValid = orderedProduct.TrueForAll(p => order.Quantity <= p.ItemsInStock - p.ItemsReserved);
                if (isOrderValid
                && intendedCustomer is Customer && intendedCustomer.GoodCreditStanding == true)
                {
                    // reduce the number of items in stock for the ordered product,
                    // and create a new order.
                    //orderedProduct.ItemsReserved += order.Quantity;

                    //var updateRequest = new RestRequest(orderedProduct.Id.ToString());
                    //updateRequest.AddJsonBody(orderedProduct);

                    //// make temp list and send in batch to be updated
                    //var updateResponse = c.PutAsync(updateRequest);
                    //updateResponse.Wait();

                    //if (updateResponse.IsCompletedSuccessfully)
                    //{
                    //    var newOrder = repository.Add(order);
                    //    return CreatedAtRoute("GetOrder",
                    //        new { id = newOrder.Id }, newOrder);
                    //}
                }
            }
            catch (Exception e)
            {
                throw new Exception("Something went wrong, order was not created. Error: " + e.ToString());
            }

            //If the order could not be created, "return no content".
            return NoContent();
        }

    }
}
