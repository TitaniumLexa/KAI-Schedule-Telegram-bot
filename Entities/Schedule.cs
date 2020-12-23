using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;

namespace KAI_Schedule.Entities
{
    [JsonObject(MemberSerialization.Fields)]
    [Serializable]
    public class Schedule
    {
        public List<Day> Days { get; set; } = new List<Day>();

        public static byte[] Serialize(Schedule schedule)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, schedule);

            return stream.ToArray();
        }

        public static Schedule Deserialize(byte[] binary)
        {
            if (binary.Length == 0)
                return null;

            var stream = new MemoryStream(binary);
            var formatter = new BinaryFormatter();

            return (Schedule)formatter.Deserialize(stream);
        }

        public string OutputDay(DayType dayType, string date = "")
        {
            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;

            string dayScheduleMessage = "";

            DateTime currentDay;
            switch (dayType)
            {
                case DayType.Today: currentDay = DateTime.Today; break;
                case DayType.Tomorrow: currentDay = DateTime.Today.AddDays(1); break;
                case DayType.Date: currentDay = DateTime.Parse(date); break;
                default: currentDay = DateTime.Today; break; // Maybe send error instead of today schedule?
            }

            DayOfWeek dayOfWeek = cal.GetDayOfWeek(currentDay);
            int weekNum = cal.GetWeekOfYear(currentDay, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            bool even = (weekNum - 1) % 2 == 0;

            var dateString = currentDay.ToString("dd.MM") + "\n\n";
            dayScheduleMessage += dateString;

            if (dayOfWeek == DayOfWeek.Sunday)
                return "Выходной";

            var day = Days[(int)dayOfWeek - 1];

            foreach (var classInfo in day.Classes)
            {
                if ((classInfo.DateType == DateType.EvenWeek && !even) || (classInfo.DateType == DateType.OddWeek && even))
                    continue;

                if (classInfo.Dates.Count > 0)
                {
                    bool noMatches = true;
                    foreach (var dateTime in classInfo.Dates)
                    {
                        if (currentDay == dateTime)
                            noMatches = false;
                    }

                    if (noMatches)
                        continue;
                }

                switch (classInfo.ClassType)
                {
                    case ClassType.Lecture: dayScheduleMessage += "Лекция "; break;
                    case ClassType.Practice: dayScheduleMessage += "Практика "; break;
                    case ClassType.Laboratory: dayScheduleMessage += "Лабораторная работа "; break;
                    //case "конс": return "Выходной";
                    default: return "Хм... Выходной?";
                }

                dayScheduleMessage += "с " + classInfo.Time + " ";
                dayScheduleMessage += classInfo.Auditorium;

                dayScheduleMessage += "\nпо предмету " + classInfo.Discipline + "\n";
                dayScheduleMessage += classInfo.LecturerName + "\n\n";
            }

            if (dayScheduleMessage == dateString)
                dayScheduleMessage += "Выходной";

            return dayScheduleMessage;
        }
    }

    [JsonObject(MemberSerialization.Fields)]
    [Serializable]
    public class Day
    {
        public List<Class> Classes { get; set; } = new List<Class>();
    }

    /// <summary>
    /// Занятие
    /// </summary>
    [JsonObject(MemberSerialization.Fields)]
    [Serializable]
    public class Class
    {
        public DateType DateType { get; set; }
        public List<DateTime> Dates { get; set; } = new List<DateTime>();

        public string Time { get; set; }

        public ClassType ClassType { get; set; }
        public string Discipline { get; set; }

        public string Auditorium { get; set; }
        public string LecturerName { get; set; }
    }

    public enum ClassType
    {
        Lecture,
        Practice,
        Laboratory,
    }

    public enum DateType
    {
        OddWeek,
        EvenWeek,
        Weekly,
        Date
    }
}
