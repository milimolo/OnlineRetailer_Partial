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



            if (intendedCustomer is Customer && intendedCustomer.GoodCreditStanding == true)
            {
                bool orderFailed = false;
                var productsToUpdate = new List<Product>();
                foreach (var orderLine in order.OrderLines)
                {
                    if (orderFailed == true)
                    {
                        break;
                    }
                    foreach (var product in orderedProduct)
                    {
                        if (orderLine.ProductId == product.Id)
                        {
                            if (orderLine.NoOfItems <= product.ItemsInStock - product.ItemsReserved)
                            {
                                product.ItemsReserved += orderLine.NoOfItems;

                                if (productsToUpdate.Count != 0)
                                {
                                    foreach (var prodToUpd in productsToUpdate)
                                    {
                                        if (prodToUpd.Id == product.Id)
                                        {
                                            prodToUpd.ItemsReserved = product.ItemsReserved;
                                        }
                                        else
                                        {
                                            productsToUpdate.Add(product);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    productsToUpdate.Add(product);
                                }

                            }
                            else
                            {
                                orderFailed = true;
                                break;
                            }
                        }
                    }
                    if (order.OrderLines.IndexOf(orderLine) == order.OrderLines.Count - 1)
                    {
                        if (productsToUpdate.Count != 0)
                        {
                            foreach (var product in productsToUpdate)
                            {
                                var updateRequest = new RestRequest(product.Id.ToString());
                                updateRequest.AddJsonBody(product);

                                // make temp list and send in batch to be updated
                                var updateResponse = c.PutAsync(updateRequest);
                                updateResponse.Wait();
                            }
                            var newOrder = repository.Add(order);
                            return CreatedAtRoute("GetOrder",
                                new { id = newOrder.Id }, newOrder);
                        }
                    }
                }
            }

            //If the order could not be created, "return no content".
            return NoContent();
        }

    }
}
