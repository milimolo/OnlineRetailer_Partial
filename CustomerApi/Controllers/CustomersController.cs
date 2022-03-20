using CustomerApi.Data;
using CustomerApi.Models;
using CustomerApi.Models.Converter;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace CustomerApi.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IRepository<Customer> repository;
        private readonly IConverter<Customer, CustomerDto> _customerConverter;

        public CustomersController(IRepository<Customer> repo, IConverter<Customer, CustomerDto> converter)
        {
            repository = repo;
            _customerConverter = converter;
        }

        // GET: Customers
        [HttpGet]
        public IEnumerable<CustomerDto> Get()
        {
            var customerDtoList = new List<CustomerDto>();
            foreach (var customer in repository.GetAll())
            {
                var customerDto = _customerConverter.Convert(customer);
                customerDtoList.Add(customerDto);
            }
            return customerDtoList;
        }

        // GET Customers/{id}
        [HttpGet("{id}", Name ="GetCustomer")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if(item == null)
            {
                return NotFound();
            }
            var customerDto = _customerConverter.Convert(item);
            return new ObjectResult(customerDto);
        }

        // POST customers
        [HttpPost]
        public IActionResult Post([FromBody] CustomerDto customerDto)
        {
            if(customerDto == null)
            {
                return BadRequest();
            }

            var customer = _customerConverter.Convert(customerDto);
            var newCustomer = repository.Add(customer);

            return CreatedAtRoute("GetCustomer", new { id = newCustomer.Id }, 
                _customerConverter.Convert(newCustomer));
        }

        // PUT customers/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] CustomerDto customerDto)
        {
            if (customerDto == null || customerDto.Id != id)
            {
                return BadRequest("Customer does not exist");
            }

            var modifiedCustomer = repository.Get(id);

            if (modifiedCustomer == null)
            {
                return NotFound();
            }

            modifiedCustomer.Name = customerDto.Name;
            modifiedCustomer.Email = customerDto.Email;
            modifiedCustomer.Phone = customerDto.Phone;
            modifiedCustomer.BillingAddress = customerDto.BillingAddress;
            modifiedCustomer.ShippingAddress = customerDto.ShippingAddress;
            modifiedCustomer.GoodCreditStanding = customerDto.GoodCreditStanding;

            repository.Edit(modifiedCustomer);
            return new NoContentResult();
        }

        // DELETE customers/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (repository.Get(id) == null)
            {
                return NotFound();
            }

            repository.Remove(id);
            return new NoContentResult();
        }
    }
}
