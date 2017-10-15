using System;
using System.Configuration;
using System.Data;
using SqlFluent.App.Models;
using SQF = SqlFluent.SqlFluent;

namespace SqlFluent.App
{
    public class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("ExecuteReader (Query)");
            Console.WriteLine("--------------------");
            var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
            new SQF(connectionstring)
                            .Query("select top 25 * from SalesLT.Product where productid > @productId and Color = @color")
                            .Parameter("@productId", SqlDbType.Int, value: 800)
                            .Parameter("@color", SqlDbType.NVarChar, value: "black", size: 50)
                            .ExecuteReader(reader => new Product
                            {
                                ProductId = reader.GetSafeValue<int>("ProductId"),
                                ProductName = reader.GetSafeValue<string>("Name"),
                                ProductNumber = reader.GetSafeValue<string>("ProductNumber"),
                                Color = reader.GetSafeValue<string>("Color"),
                                StandardCost = reader.GetSafeValue<decimal>("StandardCost"),
                                ListPrice = reader.GetSafeValue<decimal>("ListPrice"),
                                Size = reader.GetSafeValue<string>("Size"),
                                Weight = reader.GetSafeValue<decimal?>("Weight"),
                                SellStartDate = reader.GetSafeValue<DateTime>("SellStartDate"),
                                SellEndDate = reader.GetSafeValue<DateTime?>("SellEndDate")
                            }).ForEach(product =>
                            {
                                Console.WriteLine($"Product Name: ${product.ProductName} -> Product Number: ${product.ProductNumber}");
                            });

            Console.WriteLine("ExecuteReader (Stored Procedure)");
            Console.WriteLine("--------------------");
            new SQF()
                .ConnectionString(connectionstring)
                .StoredProcedure("SalesLT.Top25Products")
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value: "red", size: 50)
                .ExecuteReader(reader => new Product
                {
                    ProductId = reader.GetSafeValue<int>("ProductId"),
                    ProductName = reader.GetSafeValue<string>("Name"),
                    ProductNumber = reader.GetSafeValue<string>("ProductNumber"),
                    Color = reader.GetSafeValue<string>("Color"),
                    StandardCost = reader.GetSafeValue<decimal>("StandardCost"),
                    ListPrice = reader.GetSafeValue<decimal>("ListPrice"),
                    Size = reader.GetSafeValue<string>("Size"),
                    Weight = reader.GetSafeValue<decimal?>("Weight"),
                    SellStartDate = reader.GetSafeValue<DateTime>("SellStartDate"),
                    SellEndDate = reader.GetSafeValue<DateTime?>("SellEndDate")
                }).ForEach(product =>
                {
                    Console.WriteLine($"Product Name: ${product.ProductName} -> Product Number: ${product.ProductNumber}");
                });

            Console.WriteLine("ExecuteReader with yield (Stored Procedure)");
            Console.WriteLine("--------------------");
            new SQF(connectionstring)
                .StoredProcedure("SalesLT.Top25Products")
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value: "yellow", size: 50)
                .ExecuteReaderWithYield(reader => new Product
                {
                    ProductId = reader.GetSafeValue<int>("ProductId"),
                    ProductName = reader.GetSafeValue<string>("Name"),
                    ProductNumber = reader.GetSafeValue<string>("ProductNumber"),
                    Color = reader.GetSafeValue<string>("Color"),
                    StandardCost = reader.GetSafeValue<decimal>("StandardCost"),
                    ListPrice = reader.GetSafeValue<decimal>("ListPrice"),
                    Size = reader.GetSafeValue<string>("Size"),
                    Weight = reader.GetSafeValue<decimal?>("Weight"),
                    SellStartDate = reader.GetSafeValue<DateTime>("SellStartDate"),
                    SellEndDate = reader.GetSafeValue<DateTime?>("SellEndDate")
                }).ForEach(product =>
                {
                    Console.WriteLine($"Product Name: ${product.ProductName} -> Product Number: ${product.ProductNumber}");
                });

            Console.WriteLine("ExecuteSingle (Query)");
            Console.WriteLine("--------------------");
            var customer = new SQF(connectionstring)
                .Query("Select * from SalesLT.Customer where LastName = @lastname and customerid < @customerId")
                .Parameter("@customerId", SqlDbType.Int, value: 10)
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harris", size: 50)
                .ExecuteSingle(reader => new Customer
                {
                    CustomerId = reader.GetSafeValue<int>("CustomerId"),
                    Title = reader.GetSafeValue<string>("Title"),
                    FirstName = reader.GetSafeValue<string>("FirstName"),
                    MiddleName = reader.GetSafeValue<string>("MiddleName"),
                    LastName = reader.GetSafeValue<string>("LastName"),
                    Suffix = reader.GetSafeValue<string>("Suffix"),
                    CompanyName = reader.GetSafeValue<string>("CompanyName"),
                    EmailAddress = reader.GetSafeValue<string>("EmailAddress")
                });

            Console.WriteLine($"Customer Name: ${customer.LastName}, ${customer.FirstName} -> Email Address: ${customer.EmailAddress}");

            Console.WriteLine("ExecuteNonQuery (StoredProcedure)");
            Console.WriteLine("--------------------");
            var newGuid = Guid.NewGuid();
            var newCategoryId = 0;
            new SQF(connectionstring)
                .StoredProcedure("SalesLT.AddCategory")
                .Parameter("@name", SqlDbType.NVarChar, value: $"Test-{newGuid}", size: 200)
                .Parameter("@rowguid", SqlDbType.UniqueIdentifier, value: newGuid)
                .Parameter("@categoryId", SqlDbType.Int, direction: ParameterDirection.Output)
                .Parameter("@retVal", SqlDbType.Int, direction: ParameterDirection.ReturnValue)
                .ExecuteNonQuery(cmd =>
                {
                    if ((int)cmd.Parameters["@retVal"].Value == 1)
                    {
                        newCategoryId = (int)cmd.Parameters["@categoryId"].Value;
                        Console.WriteLine($"New Category Added -> Category # {newCategoryId}");
                    }
                    else
                    {
                        Console.WriteLine("Error occurred");
                    }
                });
        }
    }
}
