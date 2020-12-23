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
}
