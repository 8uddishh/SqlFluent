using System;
namespace SqlFluent.Web2.Models
{
    public class SalesOrder
    {
        public int SalesOrderId { get; set; }
        public byte RevisionNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime ShipDate { get; set; }
        public string SalesOrderNumber { get; set; }
    }
}
