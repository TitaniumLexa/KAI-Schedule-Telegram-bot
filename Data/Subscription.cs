using System;
using System.ComponentModel.DataAnnotations;

namespace KAI_Schedule.Data
{
    public class Subscription 
    {
        [Key] public int Id { get; set; }
        public TimeSpan NotificationTime { get; set; }
        public DateTime LastNotificationTime { get; set; }
        public TimeSpan NotificationInterval { get; set; }
        public ChatContext ChatContext { get; set; }
    }
}
