using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
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
                .Parameter("@productId", SqlDbType.Int, value: 800)
                .Parameter("@color", SqlDbType.NVarChar, value: "black", size: 50)
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
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value: "red", size: 50)
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
                .Parameter("@customerId", SqlDbType.Int, value: 10)
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harris", size: 50)
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
                .Parameter("@name", SqlDbType.NVarChar, value: $"Test-{newGuid}", size: 200)
                .Parameter("@rowguid", SqlDbType.UniqueIdentifier, value: newGuid)
                .Parameter("@categoryId", SqlDbType.Int, direction: ParameterDirection.Output)
                .Parameter("@retVal", SqlDbType.Int, direction: ParameterDirection.ReturnValue)
                .ExecuteNonQueryAsync(cmd =>
                {
                    if ((int)cmd.Parameters["@retVal"].Value == 1)
                    {
                        newCategoryId = (int)cmd.Parameters["@categoryId"].Value;
                    }
                });

            return Json(new { id=newCategoryId, newCategory = $"Test-{newGuid}" }, JsonRequestBehavior.AllowGet);
        }
    }
}
