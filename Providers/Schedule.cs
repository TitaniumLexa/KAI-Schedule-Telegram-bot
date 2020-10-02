using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KAI_Schedule.Configuration;
using KAI_Schedule.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KAI_Schedule.Providers
{
    public class Schedule
    {
        public HttpClient httpClient;

        public Schedule()
        {
            httpClient = new HttpClient();
        }

        public async Task<string> GetGroupID(string group)
        {
            var groupResult = await (await httpClient.GetAsync(ConfigManager.Config.RequestURLs.GroupID + group)).Content.ReadAsStringAsync();
            dynamic groupJson = JsonConvert.DeserializeObject(groupResult);
            try
            {
                return groupJson.First.id;
            }
            catch
            {
                return "ERROR";
            }
        }

        public async Task<string> GetRawSchedule(string groupID)
        {
            if (groupID == "ERROR")
                return "Wrong groupID";

            return await (await httpClient.GetAsync(ConfigManager.Config.RequestURLs.Schedule + groupID)).Content.ReadAsStringAsync();
        }

        public async Task<string> GetScheduleDay(string groupID, DayType dayType, string date = "")
        {
            dynamic scheduleJSON = JsonConvert.DeserializeObject(await GetRawSchedule(groupID));

            DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
            Calendar cal = dfi.Calendar;



            dynamic daySchedule;
            string schedule = "";

            DateTime day;

            switch (dayType)
            {
                case DayType.Today: day = DateTime.Today; break;
                case DayType.Tomorrow: day = DateTime.Today.AddDays(1); break;
                case DayType.Date: day = DateTime.Parse(date); break;
                default: day = DateTime.Today; break;
            }

            DayOfWeek week = cal.GetDayOfWeek(day);
            int weekNum = cal.GetWeekOfYear(day, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

            bool even = (weekNum - 1) % 2 == 0;

            schedule += day.ToString("dd.MM") + "\n\n";

            switch (week)
            {
                case DayOfWeek.Monday: { daySchedule = scheduleJSON["1"]; break; }
                case DayOfWeek.Tuesday: { daySchedule = scheduleJSON["2"]; break; }
                case DayOfWeek.Wednesday: { daySchedule = scheduleJSON["3"]; break; }
                case DayOfWeek.Thursday: { daySchedule = scheduleJSON["4"]; break; }
                case DayOfWeek.Friday: { daySchedule = scheduleJSON["5"]; break; }
                case DayOfWeek.Saturday: { daySchedule = scheduleJSON["6"]; break; }
                default: return "Выходной";

            }

            foreach (JObject obj in daySchedule)
            {
                if ((obj["dayDate"].ToString().StartsWith("чет") && !even) || (obj["dayDate"].ToString().StartsWith("неч") && even))
                    continue;

                var regMatches = Regex.Matches(obj["dayDate"].ToString(), "(\\d{1,2}\\.\\d{2}\\,?)");
                if (regMatches.Count > 0)
                {
                    bool skip = true;
                    foreach (Match match in Regex.Matches(obj["dayDate"].ToString(), "(\\d{1,2}\\.\\d{2}\\,?)"))
                    {
                        if (day == DateTime.Parse(match.Value))
                            skip = false;
                    }

                    if (skip)
                        continue;
                }

                switch (obj["disciplType"].ToString().TrimEnd(' '))
                {
                    case "лек": schedule += "Лекция "; break;
                    case "пр": schedule += "Практика "; break;
                    case "л.р.": schedule += "Лабораторная работа "; break;
                    case "конс": return "Выходной";
                    default: return "Хм... Выходной?";
                }

                schedule += "с " + obj["dayTime"].ToString().TrimEnd(' ') + " ";

                string temp = obj["audNum"].ToString().TrimEnd(' ', '-');
                schedule += (temp == "") ? "на " + obj["orgUnitName"].ToString().TrimEnd(' ') : "в аудитории №" + temp;

                schedule += "\nпо предмету " + obj["disciplName"] + "\n";
                schedule += obj["prepodName"] + "\n\n";
            }

            return schedule;

        }
    }
}
