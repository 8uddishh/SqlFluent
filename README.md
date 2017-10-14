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
                .Parameter("@productId", SqlDbType.Int, value: 800)
                .Parameter("@color", SqlDbType.NVarChar, value:"black", size: 50)
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

## Execute Reader using stored procedure, Note that the connection string can be passed as constructor parameter or by
calling ConnectionString method.

``` c#
var connectionstring = ConfigurationManager.AppSettings["connectionstring"];
var products = new SqlFluent()
                .ConnectionString(connectionstring)
                .StoredProcedure("SalesLT.Top25Products")
                .Parameter("@productId", SqlDbType.Int, value:700)
                .Parameter("@color", SqlDbType.NVarChar, value:"red", size: 50)
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
                .Parameter("@productId", SqlDbType.Int, value: 700)
                .Parameter("@color", SqlDbType.NVarChar, value:"yellow", size: 50)
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
                .Parameter("@customerId", SqlDbType.Int, value:10)
                .Parameter("@lastname", SqlDbType.NVarChar, value:"Harris", size: 50)
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
                .Parameter("@name", SqlDbType.NVarChar, value:$"Test-{newGuid}", size: 200)
                .Parameter("@rowguid", SqlDbType.UniqueIdentifier, value: newGuid)
                .Parameter("@categoryId", SqlDbType.Int, direction: ParameterDirection.Output)
                .Parameter("@retVal", SqlDbType.Int, direction: ParameterDirection.ReturnValue)
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

## What the future holds
* Async Await implementations will be added 
* Multiple result sets handling will be added
