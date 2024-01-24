
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;

using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;
            
            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void ReportingStructure_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/ReportingStructure");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reportingStructure = response.DeserializeContent<ReportingStructure>();

            Assert.IsNotNull(reportingStructure);
            AssertEmployee("John", "Lennon", reportingStructure.Employee);
            Assert.AreEqual(4, reportingStructure.NumberOfReports);

            // Paul reports to John
            var directReport1 = reportingStructure
                .Employee
                .DirectReports
                .SingleOrDefault(d => d.EmployeeId == "b7839309-3348-463b-a7e3-5de1c168beb3");

            Assert.IsNotNull(directReport1);
            AssertEmployee("Paul", "McCartney", directReport1);

            // Ringo reports to John
            var directReport2 = reportingStructure
                .Employee
                .DirectReports
                .SingleOrDefault(d => d.EmployeeId == "03aa1462-ffa9-4978-901b-7c001562cf6f");

            Assert.IsNotNull(directReport1);
            AssertEmployee("Ringo", "Starr", directReport2);

            // Pete reports to Ringo who reports to John
            var directReport3 = directReport2
                .DirectReports
                .SingleOrDefault(d => d.EmployeeId == "62c1084e-6e34-4630-93fd-9153afb65309");

            Assert.IsNotNull(directReport1);
            AssertEmployee("Pete", "Best", directReport3);

            // George reports to Ringo who reports to John
            var directReport4 = directReport2
                .DirectReports
                .SingleOrDefault(d => d.EmployeeId == "c0c2293d-16bd-4603-8e08-638a9d18b22c");

            Assert.IsNotNull(directReport1);
            AssertEmployee("George", "Harrison", directReport4);
        }

        [TestMethod]
        public void ReportingStructure_Returns_NotFound()
        {
            // Arrange
            var employeeId = "Invalid_Id";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/NumberOfReports");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void CreateCompensation_Returns_Ok()
        {
            // Arrange
            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";
            var compensation = CreateDefaultCompensation();
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{employeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var postResponse = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, postResponse.StatusCode);
            var newCompensation = postResponse.DeserializeContent<CompensationResponse>();

            AssertCompensation(employeeId, compensation, newCompensation);
        }

        [TestMethod]
        public void GetCompensation_Returns_Ok()
        {
            // Arrange
            var employeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f";
            var compensation = CreateDefaultCompensation();
            var requestContent = new JsonSerialization().ToJson(compensation);
            _httpClient.PostAsync($"api/employee/{employeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json")).Wait();

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}/compensation");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var newCompensation = response.DeserializeContent<CompensationResponse>();

            AssertCompensation(employeeId, compensation, newCompensation);
        }

        [TestMethod]
        public void CreateCompensation_Returns_NotFound()
        {
            // Arrange
            var compensation = CreateDefaultCompensation();
            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/Invalid_Id/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var postResponse = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, postResponse.StatusCode);
        }

        [TestMethod]
        public void GetCompensation_Returns_NotFound()
        {
            // Arrange
            var employeeId = "Invalid_Id";

            // Execute
            var requestTask = _httpClient.GetAsync($"api/employee/{employeeId}/compensation");
            var response = requestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetCompensation_Returns_NotFound_For_Existing_Employee()
        {
            // Arrange
            var employee = new Employee() 
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            }; // Create new employee with no compensation

            var requestContent = new JsonSerialization().ToJson(employee);
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;
            var newEmployee = response.DeserializeContent<Employee>(); // Get Id of new Employee

            // Execute
            var requestTask = _httpClient.GetAsync($"api/employee/{newEmployee.EmployeeId}/compensation");
            response = requestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Helper Methods

        private static void AssertEmployee(string expectedFirstName, string expectedLastName,
            Employee actualEmployee)
        {
            Assert.AreEqual(expectedFirstName, actualEmployee.FirstName);
            Assert.AreEqual(expectedLastName, actualEmployee.LastName);
        }

        private static Compensation CreateDefaultCompensation()
            => new()
            {
                Salary = Int16.MaxValue,
                EffectiveDate = (new DateOnly()).ToString()
            };

        private static void AssertCompensation(String employeeId, 
            Compensation expectedCompensation, CompensationResponse actualCompensaton)
        {
            Assert.AreEqual(employeeId, actualCompensaton.Employee.EmployeeId);
            Assert.AreEqual(expectedCompensation.Salary, actualCompensaton.Salary);
            Assert.AreEqual(expectedCompensation.EffectiveDate, actualCompensaton.EffectiveDate);
        }
    }
}
