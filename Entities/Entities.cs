using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Telegram.Bot.Types;

namespace KAI_Schedule.Entities
{
    public enum DayType
    {
        Today,
        Tomorrow,
        Date
    }

    public enum SubscribeType
    {
        Daily
    }

    public class RequestURLs
    {
        public string GroupID { get; set; }
        public string Schedule { get; set; }
    }

    [JsonObject(MemberSerialization.Fields)]
    public struct ChatSettings
    {
        public Chat Chat { get; set; }
        public string Group { get; set; }
    }

    [JsonObject(MemberSerialization.Fields)]
    public struct SubscribeSettings
    {
        public Chat Chat { get; set; }
        public TimeSpan Time { get; set; }
    }
}
