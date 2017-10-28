using System;
namespace SqlFluent.Web2.Models
{
    public class Address
    {
        public Address()
        {
        }

        public int AddressId { get; set; }
        public Guid AddressGuid { get; set; }
        public string AddressType { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string StateProvince { get; set; }
        public string CountryRegion { get; set; }
        public string PostalCode { get; set; }
    }
}
