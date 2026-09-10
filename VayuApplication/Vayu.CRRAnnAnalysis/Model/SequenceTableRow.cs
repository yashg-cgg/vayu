namespace Vayu.CRRAnnAnalysis.Model
{
    public class SequenceTableRow
    {
        public int Year { get; set; }
        public decimal Seq6 { get; set; }
        public decimal Seq5 { get; set; }
        public decimal Seq4 { get; set; }
        public decimal Seq3 { get; set; }
        public decimal Seq2 { get; set; }
        public decimal Seq1 { get; set; }

        public decimal LastMonth1 { get; set; }
        public decimal LastMonth2 { get; set; }
        public decimal LastMonth3 { get; set; }


        public decimal SemMonth1 { get; set; }
        public decimal SemMonth2 { get; set; }
        public decimal SemMonth3 { get; set; }
        public decimal SemMonth4 { get; set; }
        public decimal SemMonth5 { get; set; }
        public decimal SemMonth6 { get; set; }
    }
}
