using System;
using System.Collections.Generic;

namespace Vayu.Notifications.Model
{
    public interface IDataService
    {

        List<Notification> GetNotification(DateTime StartDate, DateTime Endate, string NotificationType, string status);

        // List<Message> GetMessages(DateTime date1, DateTime date2, string status);
        List<Message> GetMessages(DateTime date1, DateTime date2, string status);
        //List<Message> GetMessages(object date1, object date2, string status);
    }
}
