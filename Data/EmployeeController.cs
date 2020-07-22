using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tatneft.Data
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        // GET: api/Employee
        [HttpGet]
        public IEnumerable<Employee> Get()
        {
            return GetEmployees();
        }

        // GET: api/Employee/5
        [HttpGet("{id}", Name = "Get")]
        public Employee Get(int id)
        {
            return GetEmployees().Find(e => e.Id == id);
        }

        // POST: api/Employee
        [HttpPost]
        [Produces("application/json")]
        public Employee Post([FromBody] Employee employee)
        {
            // Logic to create new Employee
            return new Employee();
        }

        // PUT: api/Employee/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] Employee employee)
        {
            // Logic to update an Employee
        }

        // DELETE: api/Employee/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


        private List<Employee> GetEmployees()
        {
            return new List<Employee>()
        {
            new Employee()
            {
                Id = 1,
                FirstName= "John",
                LastName = "Smith",
                EmailId ="John.Smith@gmail.com"
            },
            new Employee()
            {
                Id = 2,
                FirstName= "Jane",
                LastName = "Doe",
                EmailId ="Jane.Doe@gmail.com"
            }
        };
        }
    }
}
