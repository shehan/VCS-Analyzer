using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VCS_Analyzer.Services
{
    public class Notification
    {
        
    }

    public class NotificationArgs :EventArgs
    {
        public NotificationArgs(string Message, DateTime Timestamp, NotificationType Type)
        {
            message = Message;
            timestamp = Timestamp;
            type = Type;
        }

        private string message;
        private DateTime timestamp;
        private NotificationType type;
         
        public string Message { get { return message; } }

        public DateTime Timestamp { get { return timestamp; } }

        public NotificationType Type { get { return type; } }
    }

    public enum NotificationType
    {
        SUCCESS,
        FAILURE,
        INFORMATION
    }
}
