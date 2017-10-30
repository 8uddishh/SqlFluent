using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Web.Mvc;
using SqlFluent.Web2.Models;

namespace SqlFluent.Web2.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index () {
            return View();
        }

        public async Task<JsonResult> QueryExecuteReaderAsync()
        {
            var builder =
                new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"]);
            builder.AsynchronousProcessing = true;

            var products = await new SqlFluent(builder.ConnectionString)
                .Query("select top 25 * from SalesLT.Product where productid > @productId and Color = @color")
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 800)
                .Parameter("@color", SqlDbType.NVarChar, value: "black", size: 50)
                .ParametersEnd()
                .Async()
                .ExecuteReaderAsync(async reader => new Product
                {
                    ProductId = await reader.GetSafeValueAsync<int>("ProductId"),
                    ProductName = await reader.GetSafeValueAsync<string>("Name"),
                    ProductNumber = await reader.GetSafeValueAsync<string>("ProductNumber"),
                    Color = await reader.GetSafeValueAsync<string>("Color"),
                    StandardCost = await reader.GetSafeValueAsync<decimal>("StandardCost"),
                    ListPrice = await reader.GetSafeValueAsync<decimal>("ListPrice"),
                    Size = await reader.GetSafeValueAsync<string>("Size"),
                    Weight = await reader.GetSafeValueAsync<decimal?>("Weight"),
                    SellStartDate = await reader.GetSafeValueAsync<DateTime>("SellStartDate"),
                    SellEndDate = await reader.GetSafeValueAsync<DateTime?>("SellEndDate")
                });

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> StoredProcedureExecuteReaderAsync() {
            var builder =
                new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"]);
            builder.AsynchronousProcessing = true;

            var products = await new SqlFluent(builder.ConnectionString)
                .StoredProcedure("SalesLT.Top25Products")
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value: "red", size: 50)
                .ParametersEnd()
                .Async()
                .ExecuteReaderAsync(async reader => new Product
                {
                    ProductId = await reader.GetSafeValueAsync<int>("ProductId"),
                    ProductName = await reader.GetSafeValueAsync<string>("Name"),
                    ProductNumber = await reader.GetSafeValueAsync<string>("ProductNumber"),
                    Color = await reader.GetSafeValueAsync<string>("Color"),
                    StandardCost = await reader.GetSafeValueAsync<decimal>("StandardCost"),
                    ListPrice = await reader.GetSafeValueAsync<decimal>("ListPrice"),
                    Size = await reader.GetSafeValueAsync<string>("Size"),
                    Weight = await reader.GetSafeValueAsync<decimal?>("Weight"),
                    SellStartDate = await reader.GetSafeValueAsync<DateTime>("SellStartDate"),
                    SellEndDate = await reader.GetSafeValueAsync<DateTime?>("SellEndDate")
                });

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> QueryExecuteSingleAsync()
        {
            var builder =
                new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"]);
            builder.AsynchronousProcessing = true;

            var customer = await new SqlFluent(builder.ConnectionString)
                .Query("Select * from SalesLT.Customer where LastName = @lastname and customerid < @customerId")
                .ParametersStart()
                .Parameter("@customerId", SqlDbType.Int, value: 10)
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harris", size: 50)
                .ParametersEnd()
                .Async()
                .ExecuteSingleAsync(async reader => new Customer
                {
                    CustomerId = await reader.GetSafeValueAsync<int>("CustomerId"),
                    Title = await reader.GetSafeValueAsync<string>("Title"),
                    FirstName = await reader.GetSafeValueAsync<string>("FirstName"),
                    MiddleName = await reader.GetSafeValueAsync<string>("MiddleName"),
                    LastName = await reader.GetSafeValueAsync<string>("LastName"),
                    Suffix = await reader.GetSafeValueAsync<string>("Suffix"),
                    CompanyName = await reader.GetSafeValueAsync<string>("CompanyName"),
                    EmailAddress = await reader.GetSafeValueAsync<string>("EmailAddress")
                });

            return Json(customer, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> StoredProcedureExecuteNonQueryAsync() {
            var newGuid = Guid.NewGuid();
            var newCategoryId = 0;

            var builder =
                new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"]);
            builder.AsynchronousProcessing = true;
            await new SqlFluent(builder.ConnectionString)
                .StoredProcedure("SalesLT.AddCategory")
                .ParametersStart()
                .Parameter("@name", SqlDbType.NVarChar, value: $"Test-{newGuid}", size: 200)
                .Parameter("@rowguid", SqlDbType.UniqueIdentifier, value: newGuid)
                .Parameter("@categoryId", SqlDbType.Int, direction: ParameterDirection.Output)
                .Parameter("@retVal", SqlDbType.Int, direction: ParameterDirection.ReturnValue)
                .ParametersEnd()
                .Async()
                .ExecuteNonQueryAsync(cmd =>
                {
                    if ((int)cmd.Parameters["@retVal"].Value == 1)
                    {
                        newCategoryId = (int)cmd.Parameters["@categoryId"].Value;
                    }
                });

            return Json(new { id=newCategoryId, newCategory = $"Test-{newGuid}" }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> StoredProcedureCascadeSingleAsync() {
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"]){
                AsynchronousProcessing = true
            };
            var customer2 = await new SqlFluent(builder.ConnectionString)
            .StoredProcedure("SalesLT.GetCustomerCompleteInfo")
            .ParametersStart()
            .Parameter("@customerId", SqlDbType.Int, value: 29545)
            .ParametersEnd()
            .Async() 
            .Cascade()
            .ReadersStartAsync()
            .ReaderAsync(async reader => new Customer
            {
                CustomerId = await reader.GetSafeValueAsync<int>("CustomerId"),
                Title = await reader.GetSafeValueAsync<string>("Title"),
                FirstName = await reader.GetSafeValueAsync<string>("FirstName"),
                MiddleName = await reader.GetSafeValueAsync<string>("MiddleName"),
                LastName = await reader.GetSafeValueAsync<string>("LastName"),
                Suffix = await reader.GetSafeValueAsync<string>("Suffix"),
                CompanyName = await reader.GetSafeValueAsync<string>("CompanyName"),
                EmailAddress = await reader.GetSafeValueAsync<string>("EmailAddress"),
                Addresses = new List<Address>(),
                Orders = new List<SalesOrder>()
            }).ReaderAsync<Customer>(async (reader, cust) => {
                cust.Addresses.Add(new Address {
                    AddressId = await reader.GetSafeValueAsync<int>("AddressId"),
                    AddressType = await reader.GetSafeValueAsync<string>("AddressType"),
                    AddressGuid = await reader.GetSafeValueAsync<Guid>("RowGuid"),
                    AddressLine1 = await reader.GetSafeValueAsync<string>("AddressLine1"),
                    AddressLine2 = await reader.GetSafeValueAsync<string>("AddressLine2"),
                    City = await reader.GetSafeValueAsync<string>("City"),
                    CountryRegion = await reader.GetSafeValueAsync<string>("CountryRegion"),
                    StateProvince = await reader.GetSafeValueAsync<string>("StateProvince"),
                    PostalCode = await reader.GetSafeValueAsync<string>("PostalCode")
                });
            }).ReaderAsync<Customer>(async (reader, cust) => {
                cust.Orders.Add(new SalesOrder {
                    SalesOrderId = await reader.GetSafeValueAsync<int>("SalesOrderId"),
                    OrderDate = await reader.GetSafeValueAsync<DateTime>("OrderDate"),
                    DueDate = await reader.GetSafeValueAsync<DateTime>("DueDate"),
                    ShipDate = await reader.GetSafeValueAsync<DateTime>("ShipDate"),
                    RevisionNumber = await reader.GetSafeValueAsync<byte>("RevisionNumber"),
                    SalesOrderNumber = await reader.GetSafeValueAsync<string>("SalesOrderNumber")
                });
            }).ReadersEndAsync()
            .ExecuteSingleAsync<Customer>();

            return Json(customer2, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> StoredProcedureCascadeReaderAsync() {
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"])
            {
                AsynchronousProcessing = true
            };
            var customers = await new SqlFluent(builder.ConnectionString)
                .StoredProcedure("SalesLT.GetCustomerCompleteInfoForName")
                .ParametersStart()
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harrington", size: 50)
                .ParametersEnd()
                .Async()
                .Cascade()
                .ReadersStartAsync()
                .ReaderAsync(async reader => new Customer {
                    CustomerId = await reader.GetSafeValueAsync<int>("CustomerId"),
                    Title = await reader.GetSafeValueAsync<string>("Title"),
                    FirstName = await reader.GetSafeValueAsync<string>("FirstName"),
                    MiddleName = await reader.GetSafeValueAsync<string>("MiddleName"),
                    LastName = await reader.GetSafeValueAsync<string>("LastName"),
                    Suffix = await reader.GetSafeValueAsync<string>("Suffix"),
                    CompanyName = await reader.GetSafeValueAsync<string>("CompanyName"),
                    EmailAddress = await reader.GetSafeValueAsync<string>("EmailAddress"),
                    Addresses = new List<Address>(),
                    Orders = new List<SalesOrder>()
                }).ReaderAsync<Customer>(async (reader, cust) => {
                    cust.Addresses.Add(new Address
                    {
                        AddressId = await reader.GetSafeValueAsync<int>("AddressId"),
                        AddressType = await reader.GetSafeValueAsync<string>("AddressType"),
                        AddressGuid = await reader.GetSafeValueAsync<Guid>("RowGuid"),
                        AddressLine1 = await reader.GetSafeValueAsync<string>("AddressLine1"),
                        AddressLine2 = await reader.GetSafeValueAsync<string>("AddressLine2"),
                        City = await reader.GetSafeValueAsync<string>("City"),
                        CountryRegion = await reader.GetSafeValueAsync<string>("CountryRegion"),
                        StateProvince = await reader.GetSafeValueAsync<string>("StateProvince"),
                        PostalCode = await reader.GetSafeValueAsync<string>("PostalCode")
                    });
                }).ReaderAsync<Customer>(async (reader, cust) => {
                    cust.Orders.Add(new SalesOrder
                    {
                        SalesOrderId = await reader.GetSafeValueAsync<int>("SalesOrderId"),
                        OrderDate = await reader.GetSafeValueAsync<DateTime>("OrderDate"),
                        DueDate = await reader.GetSafeValueAsync<DateTime>("DueDate"),
                        ShipDate = await reader.GetSafeValueAsync<DateTime>("ShipDate"),
                        RevisionNumber = await reader.GetSafeValueAsync<byte>("RevisionNumber"),
                        SalesOrderNumber = await reader.GetSafeValueAsync<string>("SalesOrderNumber")
                    });
                }).ReadersEndAsync()
                .SelectorAsync<Customer>(reader => cust => cust.CustomerId == reader.GetSafeValue<int>("CustomerId"))
                .ExecuteReaderAsync<Customer>();

            return Json(customers, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> QueryMultiReaderAsync() {
            var builder = new SqlConnectionStringBuilder(ConfigurationManager.AppSettings["connectionstring"])
            {
                AsynchronousProcessing = true
            };
            var resultSet = await new SqlFluent(builder.ConnectionString)
                .Query("select * from SalesLT.Product where Color=@color; select * from SalesLT.Customer where lastname=@lastname")
                .ParametersStart()
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harrington", size: 50)
                .Parameter("@color", SqlDbType.NVarChar, value: "yellow", size: 50)
                .ParametersEnd()
                .Async()
                .Multi()
                .ReadersStartAsync()
                .ReaderAsync("Colors", async reader => new Product {
                    ProductId = await reader.GetSafeValueAsync<int>("ProductId"),
                    ProductName = await reader.GetSafeValueAsync<string>("Name"),
                    ProductNumber = await reader.GetSafeValueAsync<string>("ProductNumber"),
                    Color = await reader.GetSafeValueAsync<string>("Color"),
                    StandardCost = await reader.GetSafeValueAsync<decimal>("StandardCost"),
                    ListPrice = await reader.GetSafeValueAsync<decimal>("ListPrice"),
                    Size = await reader.GetSafeValueAsync<string>("Size"),
                    Weight = await reader.GetSafeValueAsync<decimal?>("Weight"),
                    SellStartDate = await reader.GetSafeValueAsync<DateTime>("SellStartDate"),
                    SellEndDate = await reader.GetSafeValueAsync<DateTime?>("SellEndDate")
                }).ReaderAsync("Customers", async reader => new Customer {
                    CustomerId = await reader.GetSafeValueAsync<int>("CustomerId"),
                    Title = await reader.GetSafeValueAsync<string>("Title"),
                    FirstName = await reader.GetSafeValueAsync<string>("FirstName"),
                    MiddleName = await reader.GetSafeValueAsync<string>("MiddleName"),
                    LastName = await reader.GetSafeValueAsync<string>("LastName"),
                    Suffix = await reader.GetSafeValueAsync<string>("Suffix"),
                    CompanyName = await reader.GetSafeValueAsync<string>("CompanyName"),
                    EmailAddress = await reader.GetSafeValueAsync<string>("EmailAddress")
                }).ReadersEndAsync()
                .ExecuteReaderAsync();

            var products = resultSet.Get<Product>("Colors");
            var customers = resultSet.Get<Customer>("Customers");
            return Json(new { products, customers }, JsonRequestBehavior.AllowGet);
        }
    }
}
