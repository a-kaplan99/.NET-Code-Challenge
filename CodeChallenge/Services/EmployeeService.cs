using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeChallenge.Models;
using Microsoft.Extensions.Logging;
using CodeChallenge.Repositories;

namespace CodeChallenge.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(ILogger<EmployeeService> logger, IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
            _logger = logger;
        }

        public Employee Create(Employee employee)
        {
            if(employee != null)
            {
                _employeeRepository.Add(employee);
                _employeeRepository.SaveAsync().Wait();
            }

            return employee;
        }

        public Employee GetById(string id)
        {
            if(!String.IsNullOrEmpty(id))
            {
                return _employeeRepository.GetById(id);
            }

            return null;
        }

        public Employee Replace(Employee originalEmployee, Employee newEmployee)
        {
            if(originalEmployee != null)
            {
                _employeeRepository.Remove(originalEmployee);
                if (newEmployee != null)
                {
                    // ensure the original has been removed, otherwise EF will complain another entity w/ same id already exists
                    _employeeRepository.SaveAsync().Wait();

                    _employeeRepository.Add(newEmployee);
                    // overwrite the new id with previous employee id
                    newEmployee.EmployeeId = originalEmployee.EmployeeId;
                }
                _employeeRepository.SaveAsync().Wait();
            }

            return newEmployee;
        }

        public ReportingStructure ReportingStructure(string id)
        {
            // Verify employee exists
            Employee employee = GetById(id);
            if (employee == null)
            {
                return null;
            }

            var numberOfReports = LoadReportingStructureAndCalculateNumberOfReports(employee.EmployeeId, 0);
            return new ReportingStructure
            {
                Employee = employee,
                NumberOfReports = numberOfReports
            };
        }

        // Recursively build reporting structure and calculate number of reports given an employee id
        private int LoadReportingStructureAndCalculateNumberOfReports(String id, int numDirectReports)
        {
            var directReports = _employeeRepository.GetDirectReportsById(id);

            // Exit case: current employee has no direct reports
            if (directReports == null || !directReports.Any())
            {
                return 0;
            }

            return numDirectReports
                + directReports.Count
                + directReports.Sum(d => LoadReportingStructureAndCalculateNumberOfReports(d.EmployeeId, numDirectReports));
        }

        public CompensationResponse Create(String employeeId, Compensation compensation)
        {
            // Validate employee exists
            var employee = _employeeRepository.GetById(employeeId);
            if (employee == null)
            {
                return null;
            }

            if (compensation != null)
            {
                _employeeRepository.Add(employee, compensation);
                _employeeRepository.SaveAsync().Wait();
            }

            return new CompensationResponse
            {
                Employee = employee,
                Salary = compensation.Salary,
                EffectiveDate = compensation.EffectiveDate
            };
        }

        public CompensationResponse GetCompensationById(string id)
        {
            if (!String.IsNullOrEmpty(id))
            {
                // Validate employee and compensation exists
                var employee = _employeeRepository.GetByIdWithCompensation(id);
                if(employee == null || employee.Compensation == null)
                    return null;

                return new CompensationResponse
                {
                    Employee = employee,
                    Salary = employee.Compensation.Salary,
                    EffectiveDate = employee.Compensation.EffectiveDate
                };
            }

            return null;
        }
    }
}
