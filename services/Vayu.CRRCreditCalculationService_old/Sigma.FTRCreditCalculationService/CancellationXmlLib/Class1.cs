using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vayu.CancellationXmlLib
{


    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {

        private SubmitRequest submitRequestField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://eftr.pjm.com/ftr/xml")]
        public SubmitRequest SubmitRequest
        {
            get
            {
                return this.submitRequestField;
            }
            set
            {
                this.submitRequestField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://eftr.pjm.com/ftr/xml", IsNullable = false)]
    public partial class SubmitRequest
    {

        private SubmitRequestDeleteByTransaction deleteByTransactionField;

        /// <remarks/>
        public SubmitRequestDeleteByTransaction DeleteByTransaction
        {
            get
            {
                return this.deleteByTransactionField;
            }
            set
            {
                this.deleteByTransactionField = value;
            }
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
    public partial class SubmitRequestDeleteByTransaction
    {

        private int transactionIDField;

        /// <remarks/>
        public int TransactionID
        {
            get
            {
                return this.transactionIDField;
            }
            set
            {
                this.transactionIDField = value;
            }
        }
    }





}
