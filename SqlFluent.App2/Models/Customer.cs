using System;
using System.Collections.Generic;

namespace SqlFluent.App.Models
{


    public class Customer
    {
        public Customer()
        {
        }

        public int CustomerId { get; set; }
        public string Title { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public string CompanyName { get; set; }
        public string EmailAddress { get; set; }

        public List<SalesOrder> Orders { get; set; }
        public List<Address> Addresses { get; set; }
    }
}
