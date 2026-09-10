
/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
public partial class Envelope
{

    private EnvelopeBody bodyField;

    /// <remarks/>
    public EnvelopeBody Body
    {
        get
        {
            return this.bodyField;
        }
        set
        {
            this.bodyField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
public partial class EnvelopeBody
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

    private SubmitRequestFTRQuotes fTRQuotesField;

    /// <remarks/>
    public SubmitRequestFTRQuotes FTRQuotes
    {
        get
        {
            return this.fTRQuotesField;
        }
        set
        {
            this.fTRQuotesField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
public partial class SubmitRequestFTRQuotes
{

    private SubmitRequestFTRQuotesFTRQuote[] fTRQuoteField;

    private string marketField;

    private byte roundField;

    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("FTRQuote")]
    public SubmitRequestFTRQuotesFTRQuote[] FTRQuote
    {
        get
        {
            return this.fTRQuoteField;
        }
        set
        {
            this.fTRQuoteField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string market
    {
        get
        {
            return this.marketField;
        }
        set
        {
            this.marketField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public byte round
    {
        get
        {
            return this.roundField;
        }
        set
        {
            this.roundField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
public partial class SubmitRequestFTRQuotesFTRQuote
{

    private SubmitRequestFTRQuotesFTRQuotePath pathField;

    private string classField;

    private string periodField;

    private string hedgeField;

    private decimal mwField;

    private decimal priceField;

    private string tradeField;

    /// <remarks/>
    public SubmitRequestFTRQuotesFTRQuotePath Path
    {
        get
        {
            return this.pathField;
        }
        set
        {
            this.pathField = value;
        }
    }

    /// <remarks/>
    public string Class
    {
        get
        {
            return this.classField;
        }
        set
        {
            this.classField = value;
        }
    }

    /// <remarks/>
    public string Period
    {
        get
        {
            return this.periodField;
        }
        set
        {
            this.periodField = value;
        }
    }

    /// <remarks/>
    public string Hedge
    {
        get
        {
            return this.hedgeField;
        }
        set
        {
            this.hedgeField = value;
        }
    }

    /// <remarks/>
    public decimal MW
    {
        get
        {
            return this.mwField;
        }
        set
        {
            this.mwField = value;
        }
    }

    /// <remarks/>
    public decimal Price
    {
        get
        {
            return this.priceField;
        }
        set
        {
            this.priceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string trade
    {
        get
        {
            return this.tradeField;
        }
        set
        {
            this.tradeField = value;
        }
    }
}

/// <remarks/>
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
public partial class SubmitRequestFTRQuotesFTRQuotePath
{

    private string sourceField;

    private string sinkField;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string source
    {
        get
        {
            return this.sourceField;
        }
        set
        {
            this.sourceField = value;
        }
    }

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public string sink
    {
        get
        {
            return this.sinkField;
        }
        set
        {
            this.sinkField = value;
        }
    }
}








///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
//public partial class Envelope
//{

//    private EnvelopeBody bodyField;

//    /// <remarks/>
//    public EnvelopeBody Body
//    {
//        get
//        {
//            return this.bodyField;
//        }
//        set
//        {
//            this.bodyField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
//public partial class EnvelopeBody
//{

//    private SubmitRequest submitRequestField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://eftr.pjm.com/ftr/xml")]
//    public SubmitRequest SubmitRequest
//    {
//        get
//        {
//            return this.submitRequestField;
//        }
//        set
//        {
//            this.submitRequestField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
//[System.Xml.Serialization.XmlRootAttribute(Namespace = "http://eftr.pjm.com/ftr/xml", IsNullable = false)]
//public partial class SubmitRequest
//{

//    private SubmitRequestFTRQuotes fTRQuotesField;

//    /// <remarks/>
//    public SubmitRequestFTRQuotes FTRQuotes
//    {
//        get
//        {
//            return this.fTRQuotesField;
//        }
//        set
//        {
//            this.fTRQuotesField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
//public partial class SubmitRequestFTRQuotes
//{

//    private SubmitRequestFTRQuotesFTRQuote[] fTRQuoteField;

//    private string marketField;

//    private byte roundField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlElementAttribute("FTRQuote")]
//    public SubmitRequestFTRQuotesFTRQuote[] FTRQuote
//    {
//        get
//        {
//            return this.fTRQuoteField;
//        }
//        set
//        {
//            this.fTRQuoteField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string market
//    {
//        get
//        {
//            return this.marketField;
//        }
//        set
//        {
//            this.marketField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public byte round
//    {
//        get
//        {
//            return this.roundField;
//        }
//        set
//        {
//            this.roundField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
//public partial class SubmitRequestFTRQuotesFTRQuote
//{

//    private SubmitRequestFTRQuotesFTRQuotePath pathField;

//    private string classField;

//    private string periodField;

//    private string hedgeField;

//    private decimal mwField;

//    private decimal priceField;

//    private string tradeField;

//    /// <remarks/>
//    public SubmitRequestFTRQuotesFTRQuotePath Path
//    {
//        get
//        {
//            return this.pathField;
//        }
//        set
//        {
//            this.pathField = value;
//        }
//    }

//    /// <remarks/>
//    public string Class
//    {
//        get
//        {
//            return this.classField;
//        }
//        set
//        {
//            this.classField = value;
//        }
//    }

//    /// <remarks/>
//    public string Period
//    {
//        get
//        {
//            return this.periodField;
//        }
//        set
//        {
//            this.periodField = value;
//        }
//    }

//    /// <remarks/>
//    public string Hedge
//    {
//        get
//        {
//            return this.hedgeField;
//        }
//        set
//        {
//            this.hedgeField = value;
//        }
//    }

//    /// <remarks/>
//    public decimal MW
//    {
//        get
//        {
//            return this.mwField;
//        }
//        set
//        {
//            this.mwField = value;
//        }
//    }

//    /// <remarks/>
//    public decimal Price
//    {
//        get
//        {
//            return this.priceField;
//        }
//        set
//        {
//            this.priceField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string trade
//    {
//        get
//        {
//            return this.tradeField;
//        }
//        set
//        {
//            this.tradeField = value;
//        }
//    }
//}

///// <remarks/>
//[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://eftr.pjm.com/ftr/xml")]
//public partial class SubmitRequestFTRQuotesFTRQuotePath
//{

//    private string sourceField;

//    private string sinkField;

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string source
//    {
//        get
//        {
//            return this.sourceField;
//        }
//        set
//        {
//            this.sourceField = value;
//        }
//    }

//    /// <remarks/>
//    [System.Xml.Serialization.XmlAttributeAttribute()]
//    public string sink
//    {
//        get
//        {
//            return this.sinkField;
//        }
//        set
//        {
//            this.sinkField = value;
//        }
//    }
//}


