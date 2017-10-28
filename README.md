# SqlFluent
A fluent sql data access layer

## Cloning the repo

Clone this repo into new project folder (e.g., `herodb`).
```shell
git clone https://github.com/8uddishh/SqlFluent SqlFluent
cd SqlFluent

```

## Using the Library
The repository contains two projects SqlFluent and SqlFluent.App. SqlFluent contains the actual library and 
the SqlFluent.App has the code to implement. Refer SqlFluent to your current project as reference and import namespace using 

```c#
using SqlFluent;
using System.Data;
```
System.Data is required to pass parameters to SqlFluent. Once done you are all set to use the library. Below are some typical examples.


## Execute Reader using sql query
``` c#
var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
var products = new SqlFluent(connectionstring)
                .Query("select top 25 * from SalesLT.Product where productid > @productId and Color = @color")
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 800)
                .Parameter("@color", SqlDbType.NVarChar, value: "black", size: 50)
                .ParametersEnd()
                .ExecuteReader(reader => new Product {
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
                });
```

## Execute Reader using stored procedure, Note that the connection string can be passed as constructor parameter or by
calling ConnectionString method.

``` c#
var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
var products = new SqlFluent()
                .ConnectionString(connectionstring)
                .StoredProcedure("SalesLT.Top25Products")
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value:700)
                .Parameter("@color", SqlDbType.NVarChar, value:"red", size: 50)
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
                });
```

## Execute Reader using Yield (This would not hold the collection in memory)
``` c#
var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
var products = new SqlFluent(connectionstring)
                .StoredProcedure("SalesLT.Top25Products")
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value:"yellow", size: 50)
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
                });
```

## Execute Single - Get the first record as object
``` c#
var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
var customer = new SqlFluent(connectionstring)
                .Query("Select * from SalesLT.Customer where LastName = @lastname and customerid < @customerId")
                .ParametersStart()
                .Parameter("@customerId", SqlDbType.Int, value:10)
                .Parameter("@lastname", SqlDbType.NVarChar, value:"Harris", size: 50)
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
```

## Execute Non query
``` c#
            var newGuid = Guid.NewGuid();
            var newCategoryId = 0;
            new SqlFluent(connectionstring)
                .StoredProcedure("SalesLT.AddCategory")
                .ParametersStart()
                .Parameter("@name", SqlDbType.NVarChar, value:$"Test-{newGuid}", size: 200)
                .Parameter("@rowguid", SqlDbType.UniqueIdentifier, value: newGuid)
                .Parameter("@categoryId", SqlDbType.Int, direction: ParameterDirection.Output)
                .Parameter("@retVal", SqlDbType.Int, direction: ParameterDirection.ReturnValue)
                .ParametersEnd()
                .ExecuteNonQuery(cmd =>
                {
                    if((int)cmd.Parameters["@retVal"].Value == 1) {
                        newCategoryId = (int)cmd.Parameters["@categoryId"].Value;
                        Console.WriteLine($"New Category Added -> Category # {newCategoryId}");
                    }
                    else {
                        Console.WriteLine("Error occurred");
                    }
                });
```

## Execute Single - Cascade Mode 
``` c#
    var customer2 = new SQF(connectionstring)
            .StoredProcedure("SalesLT.GetCustomerCompleteInfo")
            .ParametersStart()
            .Parameter("@customerId", SqlDbType.Int, value: 29545)
            .ParametersEnd()
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
            })
            .Reader<Customer>((reader, cust) => {
                cust.Orders.Add(new SalesOrder {
                    SalesOrderId = reader.GetSafeValue<int>("SalesOrderId"),
                    OrderDate = reader.GetSafeValue<DateTime>("OrderDate"),
                    DueDate = reader.GetSafeValue<DateTime>("DueDate"),
                    ShipDate = reader.GetSafeValue<DateTime>("ShipDate"),
                    RevisionNumber = reader.GetSafeValue<byte>("RevisionNumber"),
                    SalesOrderNumber = reader.GetSafeValue<string>("SalesOrderNumber") 
                }); 
            })
            .ReadersEnd()
            .ExecuteSingleWithCascade<Customer>();
```

## Execute Reader - Cascade Mode
``` c#
    var customers = new SQF(connectionstring)
                .StoredProcedure("SalesLT.GetCustomerCompleteInfoForName")
                .ParametersStart()
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harrington", size: 50)
                .ParametersEnd()
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
                .ExecuteReaderWithCascade<Customer>();
```
# Async
Async implementations are available in the SqlFluent.Web2 application in the HomeController


## Async Implementations ExecuteReaderAsync
``` c#
    var products = await new SqlFluent(builder.ConnectionString)
                .Query("select top 25 * from SalesLT.Product where productid > @productId and Color = @color")
                .ParametersStart()
                .Parameter("@productId", SqlDbType.Int, value: 800)
                .Parameter("@color", SqlDbType.NVarChar, value: "black", size: 50)
                .ParametersEnd()
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
                
```

## Async Implementations ExecuteSingleAsync
``` c#
    var customer = await new SqlFluent(builder.ConnectionString)
                .Query("Select * from SalesLT.Customer where LastName = @lastname and customerid < @customerId")
                .ParametersStart()
                .Parameter("@customerId", SqlDbType.Int, value: 10)
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harris", size: 50)
                .ParametersEnd()
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
```

## Async Implementations ExecuteNonQueryAsync
``` c#
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
                .ExecuteNonQueryAsync(cmd =>
                {
                    if ((int)cmd.Parameters["@retVal"].Value == 1)
                    {
                        newCategoryId = (int)cmd.Parameters["@categoryId"].Value;
                    }
                });
```

## Async Implementations ExecuteSingleWithCascadeAsync
``` c#
    var customer2 = await new SqlFluent(builder.ConnectionString)
            .StoredProcedure("SalesLT.GetCustomerCompleteInfo")
            .ParametersStart()
            .Parameter("@customerId", SqlDbType.Int, value: 29545)
            .ParametersEnd()
            .Async() 
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
            .ExecuteSingleWithCascadeAsync<Customer>();
```

## Async Implementations ExecuteReaderWithCascadeAsync
``` c#
    var customers = await new SqlFluent(builder.ConnectionString)
                .StoredProcedure("SalesLT.GetCustomerCompleteInfoForName")
                .ParametersStart()
                .Parameter("@lastname", SqlDbType.NVarChar, value: "Harrington", size: 50)
                .ParametersEnd()
                .Async()
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
                .ExecuteReaderWithCascadeAsync<Customer>();
```
## What the future holds
* Multiple result sets handling will be added
* Multi level where level > 2 will be added
