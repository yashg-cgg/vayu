using System;

namespace Vayu.Notifications.Model
{
    public class Notification
    {
        public string Verb { get; set; }
        public string Noun { get; set; }
        public string Source { get; set; }
        public string UserId { get; set; }
        public string MessageId { get; set; }
        public string QSE { get; set; }
        public string ID { get; set; }
        public string MessageType { get; set; }
        public string Priority { get; set; }
        public string MessageSource { get; set; }
        public DateTime IssuedTime { get; set; }
        public string MessageSummary { get; set; }

        public DateTime MarketDate { get; set; }
        public DateTime SubmittedDateTime { get; set; }
        public string MRID { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string ErrorSeverity { get; set; }
    }
}
