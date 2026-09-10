using System;
using System.Collections.Generic;

namespace Vayu.Invoice.Models
{
    public interface IDataService
    {
        void loadDBCommands();
        void GetInvoiceData(Action<List<InvoiceSettlement>, Exception> callback, string desc);
    }
    public class InvoiceSettlement
    {
        public string InvoiceId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public double InvoiceTotal { get; set; }
        public string StatementType { get; set; }
        public string StatementId { get; set; }
        public DateTime OperatingDate { get; set; }
        public double StatementAmount { get; set; }
    }
}
