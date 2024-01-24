using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody]Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        [HttpGet("{id}/reportingStructure", Name = "reportingStructure")]
        public IActionResult ReportingStructure(String id)
        {
            _logger.LogDebug($"Received employee get number of direct reports request for '{id}'");

            var reportingStructure = _employeeService.ReportingStructure(id);
            if (reportingStructure == null)
                return NotFound();

            return Ok(reportingStructure);
        }

        [HttpPost("{id}/compensation")]
        public IActionResult CreateCompensation(String id, [FromBody] Compensation compensation)
        {
            _logger.LogDebug($"Received compensation create request for employee with id '{id}'");

            var createResult = _employeeService.Create(id, compensation);
            if (createResult == null)
                return NotFound();

            return CreatedAtRoute("getCompensationById", new { id }, createResult);
        }

        [HttpGet("{id}/compensation", Name = "getCompensationById")]
        public IActionResult GetCompensationById(String id)
        {
            _logger.LogDebug($"Received employee get compensation request for '{id}'");

            var compensationResponse = _employeeService.GetCompensationById(id);
            if (compensationResponse == null)
                return NotFound();

            return Ok(compensationResponse);
        }
    }
}
