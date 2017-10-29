using System;
using System.Collections.Generic;
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
            var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
            Console.WriteLine("ExecuteReader (Query)");
            Console.WriteLine("--------------------");
            new SQF(connectionstring)
                            .Query("select top 25 * from SalesLT.Product where productid > @productId and Color = @color")
                            .ParametersStart()
                            .Parameter("@productId", SqlDbType.Int, value: 800)
                            .Parameter("@color", SqlDbType.NVarChar, value: "black", size: 50)
                            .ParametersEnd()
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
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value: "red", size: 50)
                .ParametersEnd()
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
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value: "yellow", size: 50)
                .ParametersEnd()
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
                .ParametersStart()
                .Parameter("@customerId", SqlDbType.Int, value: 10)
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harris", size: 50)
                .ParametersEnd()
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
            .ParametersStart()
            .Parameter("@name", SqlDbType.NVarChar, value: $"Test-{newGuid}", size: 200)
            .Parameter("@rowguid", SqlDbType.UniqueIdentifier, value: newGuid)
            .Parameter("@categoryId", SqlDbType.Int, direction: ParameterDirection.Output)
            .Parameter("@retVal", SqlDbType.Int, direction: ParameterDirection.ReturnValue)
            .ParametersEnd()
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

            Console.WriteLine("ExecuteSingle Cascade (StoredProcedure)");
            Console.WriteLine("--------------------");
            var customer2 = new SQF(connectionstring)
            .StoredProcedure("SalesLT.GetCustomerCompleteInfo")
            .ParametersStart()
            .Parameter("@customerId", SqlDbType.Int, value: 29545)
            .ParametersEnd()
            .Cascade()
            .ReadersStart()
            .Reader(reader => new Customer { 
                CustomerId = reader.GetSafeValue<int>("CustomerId"),
                Title = reader.GetSafeValue<string>("Title"),
                FirstName = reader.GetSafeValue<string>("FirstName"),
                MiddleName = reader.GetSafeValue<string>("MiddleName"),
                LastName = reader.GetSafeValue<string>("LastName"),
                Suffix = reader.GetSafeValue<string>("Suffix"),
                CompanyName = reader.GetSafeValue<string>("CompanyName"),
                EmailAddress = reader.GetSafeValue<string>("EmailAddress"),
                Addresses = new List<Address>(),
                Orders = new List<SalesOrder>()
            })
            .Reader<Customer>((reader, cust) => { 
                cust.Addresses.Add(new Address {
                    AddressId = reader.GetSafeValue<int>("AddressId"),
                    AddressType = reader.GetSafeValue<string>("AddressType"),
                    AddressGuid = reader.GetSafeValue<Guid>("RowGuid"),
                    AddressLine1 = reader.GetSafeValue<string>("AddressLine1"),
                    AddressLine2 = reader.GetSafeValue<string>("AddressLine2"),
                    City = reader.GetSafeValue<string>("City"),
                    CountryRegion = reader.GetSafeValue<string>("CountryRegion"),
                    StateProvince = reader.GetSafeValue<string>("StateProvince"),
                    PostalCode = reader.GetSafeValue<string>("PostalCode")
                }); 
            }).Reader<Customer>((reader, cust) => {
                cust.Orders.Add(new SalesOrder {
                    SalesOrderId = reader.GetSafeValue<int>("SalesOrderId"),
                    OrderDate = reader.GetSafeValue<DateTime>("OrderDate"),
                    DueDate = reader.GetSafeValue<DateTime>("DueDate"),
                    ShipDate = reader.GetSafeValue<DateTime>("ShipDate"),
                    RevisionNumber = reader.GetSafeValue<byte>("RevisionNumber"),
                    SalesOrderNumber = reader.GetSafeValue<string>("SalesOrderNumber") 
                }); 
            }).ReadersEnd()
            .ExecuteSingle<Customer>();

            Console.WriteLine("ExecuteReader Cascade (StoredProcedure)");
            Console.WriteLine("--------------------");
            var customers = new SQF(connectionstring)
                .StoredProcedure("SalesLT.GetCustomerCompleteInfoForName")
                .ParametersStart()
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harrington", size: 50)
                .ParametersEnd()
                .Cascade()
                .ReadersStart()
                .Reader(reader => new Customer {
                    CustomerId = reader.GetSafeValue<int>("CustomerId"),
                    Title = reader.GetSafeValue<string>("Title"),
                    FirstName = reader.GetSafeValue<string>("FirstName"),
                    MiddleName = reader.GetSafeValue<string>("MiddleName"),
                    LastName = reader.GetSafeValue<string>("LastName"),
                    Suffix = reader.GetSafeValue<string>("Suffix"),
                    CompanyName = reader.GetSafeValue<string>("CompanyName"),
                    EmailAddress = reader.GetSafeValue<string>("EmailAddress"),
                    Addresses = new List<Address>(),
                    Orders = new List<SalesOrder>()
                }).Reader<Customer>((reader, cust) => {
                    cust.Addresses.Add(new Address
                    {
                        AddressId = reader.GetSafeValue<int>("AddressId"),
                        AddressType = reader.GetSafeValue<string>("AddressType"),
                        AddressGuid = reader.GetSafeValue<Guid>("RowGuid"),
                        AddressLine1 = reader.GetSafeValue<string>("AddressLine1"),
                        AddressLine2 = reader.GetSafeValue<string>("AddressLine2"),
                        City = reader.GetSafeValue<string>("City"),
                        CountryRegion = reader.GetSafeValue<string>("CountryRegion"),
                        StateProvince = reader.GetSafeValue<string>("StateProvince"),
                        PostalCode = reader.GetSafeValue<string>("PostalCode")
                    });
                }).Reader<Customer>((reader, cust) => {
                    cust.Orders.Add(new SalesOrder
                    {
                        SalesOrderId = reader.GetSafeValue<int>("SalesOrderId"),
                        OrderDate = reader.GetSafeValue<DateTime>("OrderDate"),
                        DueDate = reader.GetSafeValue<DateTime>("DueDate"),
                        ShipDate = reader.GetSafeValue<DateTime>("ShipDate"),
                        RevisionNumber = reader.GetSafeValue<byte>("RevisionNumber"),
                        SalesOrderNumber = reader.GetSafeValue<string>("SalesOrderNumber")
                    });
                }).ReadersEnd()
                .Selector<Customer>(reader => cust => cust.CustomerId == reader.GetSafeValue<int>("CustomerId"))
                .ExecuteReader<Customer>();

            Console.WriteLine("ExecuteReader Multi (Query)");
            Console.WriteLine("--------------------");
            var resultSet = new SQF(connectionstring)
                .Query("select * from SalesLT.Product where Color=@color; select * from SalesLT.Customer where lastname=@lastname")
                .ParametersStart()
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harrington", size: 50)
                .Parameter("@color", SqlDbType.NVarChar, value: "yellow", size: 50)
                .ParametersEnd()
                .Multi()
                .ReadersStart()
                .Reader("Colors", reader => new Product {
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
                }).Reader("Customers", reader => new Customer {
                    CustomerId = reader.GetSafeValue<int>("CustomerId"),
                    Title = reader.GetSafeValue<string>("Title"),
                    FirstName = reader.GetSafeValue<string>("FirstName"),
                    MiddleName = reader.GetSafeValue<string>("MiddleName"),
                    LastName = reader.GetSafeValue<string>("LastName"),
                    Suffix = reader.GetSafeValue<string>("Suffix"),
                    CompanyName = reader.GetSafeValue<string>("CompanyName"),
                    EmailAddress = reader.GetSafeValue<string>("EmailAddress")
                }).ReadersEnd()
                .ExecuteReader();

            var products = resultSet.Get<Product>("Colors");
            var custs = resultSet.Get<Customer>("Customers"); 
        }
    }
}
