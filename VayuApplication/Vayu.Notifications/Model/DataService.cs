
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Vayu.CommonAccessLibrary;

namespace Vayu.Notifications.Model
{
    public class DataService : IDataService
    {
        private SqlConnection VayuConnection;
        private SqlCommand SelectNotification;
        private SqlCommand SelectSubmissionNotification;
        private SqlCommand SelectSearchSubmissionNotification;
        private SqlCommand SelectMessages;
        private SqlCommand SelectSearchMessages;
        public DataService()
        {
            loadDBCommands();
        }

        public void loadDBCommands()
        {
            VayuConnection = new VayuDBConnection().GetInstance().GetSqlConnection();

            SelectNotification = VayuConnection.CreateCommand();
            SelectNotification.CommandText = "SELECT * FROM ErcotListenerMesages WHERE IssuedTime BETWEEN @StartDate AND @EndDate";
            SelectNotification.Parameters.AddWithValue("@StartDate", "IssuedTime");
            SelectNotification.Parameters.AddWithValue("@EndDate", "IssuedTime");

            SelectSubmissionNotification = VayuConnection.CreateCommand();
            SelectSubmissionNotification.CommandText = " SELECT * from ErcotListenerSubmissionMesages where MarketDate  BETWEEN @StartDate AND @EndDate";
            SelectSubmissionNotification.Parameters.AddWithValue("@StartDate", "SubmittedDateTime");
            SelectSubmissionNotification.Parameters.AddWithValue("@EndDate", "SubmittedDateTime");

            SelectSearchSubmissionNotification = VayuConnection.CreateCommand();
            SelectSearchSubmissionNotification.CommandText = "SELECT * from ErcotListenerSubmissionMesages where MarketDate  BETWEEN @StartDate AND @EndDate and Status=@Status";
            SelectSubmissionNotification.Parameters.AddWithValue("@StartDate", "SubmittedDateTime");
            SelectSubmissionNotification.Parameters.AddWithValue("@EndDate", "SubmittedDateTime");
            SelectSubmissionNotification.Parameters.AddWithValue("@Status", "Status");


            SelectMessages = new SqlCommand();
            SelectMessages.CommandText = "SELECT * FROM OperationMessages_OR WHERE CreateDate BETWEEN @StartDate AND @EndDate order by CreateDate desc";
            SelectMessages.Parameters.AddWithValue("@StartDate", "CreateDate");
            SelectMessages.Parameters.AddWithValue("@EndDate", "CreateDate");
            SelectMessages.Connection = VayuConnection;

            //SelectSearchMessages = VayuDBConnection.CreateCommand();
            SelectSearchMessages = new SqlCommand();
            SelectSearchMessages.CommandText = "SELECT * from OperationMessages_OR where CreateDate  BETWEEN @StartDate AND @EndDate and Status=@Status order by CreateDate desc";
            SelectSearchMessages.Parameters.AddWithValue("@StartDate", "CreateDate");
            SelectSearchMessages.Parameters.AddWithValue("@EndDate", "CreateDate");
            SelectSearchMessages.Parameters.AddWithValue("@Status", "Status");
            SelectSearchMessages.Connection = VayuConnection;
        }

        public List<Notification> GetNotification(DateTime StartDate, DateTime Endate, string NotificationType, string status)
        {
            List<Notification> lstNotification = new List<Notification>();
            try
            {
                Notification notification = new Notification();
                using (SqlConnection con = new SqlConnection(VayuConnection.ConnectionString))
                {
                    using (SqlCommand cmd = con.CreateCommand())
                    {
                        con.Open();
                        if (NotificationType == "General Notification")
                        {
                            cmd.CommandText = SelectNotification.CommandText;
                            cmd.Parameters.Add("@StartDate", StartDate);
                            cmd.Parameters.Add("@EndDate", Endate);
                        }
                        else
                        {
                            if (status == "Nostatus")
                            {
                                cmd.CommandText = SelectSubmissionNotification.CommandText;
                                cmd.Parameters.Add("@StartDate", StartDate);
                                cmd.Parameters.Add("@EndDate", Endate);
                            }
                            else
                            {
                                cmd.CommandText = SelectSearchSubmissionNotification.CommandText;
                                cmd.Parameters.Add("@StartDate", StartDate);
                                cmd.Parameters.Add("@EndDate", Endate);
                                cmd.Parameters.Add("@Status", status);
                            }
                        }
                        SqlDataReader reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (NotificationType == "General Notification")
                            {
                                notification = new Notification();
                                notification.Verb = reader.GetString(0);
                                notification.Noun = reader.GetString(1);
                                notification.Source = reader.GetString(2);
                                notification.UserId = reader.GetString(3);
                                notification.MessageId = reader.GetString(4);
                                notification.QSE = reader.GetString(5);
                                notification.ID = reader.GetString(6);
                                notification.MessageType = reader.GetString(7);
                                notification.Priority = reader.GetString(8);
                                notification.MessageSource = reader.GetString(9);
                                notification.IssuedTime = reader.GetDateTime(10);
                                notification.MessageSummary = reader.GetString(11);
                            }
                            else
                            {
                                notification = new Notification();
                                notification.MarketDate = reader.GetDateTime(0);
                                notification.SubmittedDateTime = reader.GetDateTime(1);
                                notification.MRID = reader.GetString(2);
                                notification.Status = reader.GetString(3);
                                notification.Error = reader.GetString(4);
                                notification.ErrorSeverity = reader.GetString(5);
                            }
                            lstNotification.Add(notification);
                        }

                    }
                }
            }
            catch
            {
            }
            return lstNotification.ToList();
        }

        public List<Message> GetMessages(DateTime StartDate, DateTime EndDate, string status)
        {
            List<Message> lstMessage = new List<Message>();
            Message message = new Message();
            try
            {
                if (VayuConnection.State == ConnectionState.Closed)
                    VayuConnection.Open();

                if (status != "All")
                {
                    SelectSearchMessages.Parameters["@StartDate"].Value = StartDate;
                    SelectSearchMessages.Parameters["@EndDate"].Value = EndDate;
                    SelectSearchMessages.Parameters["@Status"].Value = status;
                    SelectSearchMessages.Connection = VayuConnection;
                    SqlDataReader sReader = SelectSearchMessages.ExecuteReader();


                    while (sReader.Read())
                    {
                        message = new Message();
                        message.Date = sReader.GetString(1);
                        message.Messages = sReader.GetString(2);
                        message.Type = sReader.GetString(3);
                        message.Status = sReader.GetString(4);
                        message.CreateDate = sReader.GetDateTime(5);
                        lstMessage.Add(message);
                    }
                    sReader.Close();
                    SelectSearchMessages.Connection.Close();
                }
                else
                {
                    SelectMessages.Parameters["@StartDate"].Value = StartDate;
                    SelectMessages.Parameters["@EndDate"].Value = EndDate;
                    SelectMessages.Connection = VayuConnection;
                    SqlDataReader reader1 = SelectMessages.ExecuteReader();

                    while (reader1.Read())
                    {
                        message = new Message();
                        message.Date = reader1.GetString(1);
                        message.Messages = reader1.GetString(2);
                        message.Type = reader1.GetString(3);
                        message.Status = reader1.GetString(4);
                        message.CreateDate = reader1.GetDateTime(5);
                        lstMessage.Add(message);
                    }
                    reader1.Close();
                    SelectMessages.Connection.Close();
                }
            }
            catch (SqlException ex)
            {
                var x = ex.Message;
            }
            return lstMessage.ToList();
        }
    }
}
