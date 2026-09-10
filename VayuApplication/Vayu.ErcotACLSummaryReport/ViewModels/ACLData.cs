using System;

namespace Vayu.ErcotACLSummaryReport.ViewModel
{
    public class ACLData
    {
        public DateTime BusinessDate { get; set; }
        public double Cash { get; set; }
        public double TotalSecuredCollateral { get; set; }
        public double TPES { get; set; }
        public double IndependentAmount { get; set; }
        public double RemainderCollateral { get; set; }
        public double LetterOfCredit { get; set; }
        public double SuretyBond { get; set; }
        //public double Guarantee { get; set; }
        //public double UnsecuredCreditLimit { get; set; }
        public double ApprovedCRRBilateralTrades { get; set; }
        public double CRRLockedACL { get; set; }
    }
    public class CRRACLData
    {
        public DateTime BusinessDate { get; set; }
        public double TPES { get; set; }
        public double IndependentAmount { get; set; }
        public double ApprovedBilateralTrades { get; set; }
        public double ACLLockedForCRR { get; set; }
        public double TPESInExcessOfSecuredCollateral { get; set; }
        public double OutstandingSecuredCollateralRequest { get; set; }
        public double AdditionalSecuredCollateralRequired { get; set; }
        public double TPESAsPerOfSecuredCollateral { get; set; }
        public double CRRACL { get; set; }
        public double AdjustedCRRACL { get; set; }
        public double ACLSentToCRR { get; set; }

    }

    public class DAMACLData
    {
        public DateTime BusinessDate { get; set; }
        public double TPEA { get; set; }
        public double RemainderCollateral { get; set; }
        public double TPEAAsPerOfAnyCollateralAndUnsecuredCreditLimit { get; set; }
        public double DAMACL { get; set; }
        public double AdjustedDAMACL { get; set; }
        public double ACLSentToDAM { get; set; }
        // public double Guarantee { get; set; }
        //public double UnsecuredCreditLimit { get; set; }
        public double TPEAInExcessOfRemainderCollateralAndUnsecuredCredit { get; set; }
        public double OutstandingAnyCollateralRequest { get; set; }
        public double AdditionalAnyCollateralRequired { get; set; }

    }
    public class ClTranACLData
    {
        public DateTime BusinessDate { get; set; }
        public string CollateralType { get; set; }
        public double BeginningBalance { get; set; }
        public string TransactionDate { get; set; }
        public double TransactionAmount { get; set; }
        public double EndingBalance { get; set; }
        public string Description { get; set; }

    }


}