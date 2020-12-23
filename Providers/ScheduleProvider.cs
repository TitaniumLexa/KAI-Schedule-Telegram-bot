using KAI_Schedule.Configuration;
using KAI_Schedule.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KAI_Schedule.Providers
{
    public class ScheduleProvider
    {
        private readonly HttpClient _httpClient;

        public ScheduleProvider()
        {
            _httpClient = new HttpClient();
        }

        public async Task<string> GetGroupId(string group)
        {
            var groupResult = await (await _httpClient.GetAsync(ConfigManager.Config.RequestURLs.GroupID + group)).Content.ReadAsStringAsync();
            dynamic groupJson = JsonConvert.DeserializeObject(groupResult);

            return groupJson?.First?.id;
        }

        public async Task<string> GetRawSchedule(string groupId)
        {
            if (groupId == null)
                return null;

            return await (await _httpClient.GetAsync(ConfigManager.Config.RequestURLs.Schedule + groupId)).Content.ReadAsStringAsync();
        }

        public async Task<Schedule> ParseScheduleByGroup(string groupId)
        {
            string schedule = await GetRawSchedule(groupId);
            return ParseSchedule(schedule);
        }

        public static Schedule ParseSchedule(string scheduleStringJson)
        {
            if (scheduleStringJson == null)
                return null;

            dynamic scheduleJson = JsonConvert.DeserializeObject(scheduleStringJson);

            var schedule = new Schedule();

            for (int dayNumber = 1; dayNumber <= 6; dayNumber++)
            {
                var scheduleDay = new Day();

                foreach (JObject classJson in scheduleJson[dayNumber.ToString()])
                {
                    var scheduleClass = new Class();
                    
                    // Parsing dates
                    // Maybe trim spaces from end and use Switch-Case
                    if (classJson["dayDate"].ToString().StartsWith("нечет"))
                        scheduleClass.DateType = DateType.OddWeek;
                    else if (classJson["dayDate"].ToString().StartsWith("чет"))
                        scheduleClass.DateType = DateType.EvenWeek;
                    else if (classJson["dayDate"].ToString().StartsWith("еженедельно"))
                        scheduleClass.DateType = DateType.Weekly;
                    else
                    {
                        var regMatches = Regex.Matches(classJson["dayDate"].ToString(), "(\\d{1,2}\\.\\d{2}\\,?)");
                        foreach (Match match in regMatches)
                        {
                            scheduleClass.Dates.Add(DateTime.Parse(match.Value));
                        }

                        if (regMatches.Count > 0)
                            scheduleClass.DateType = DateType.Date;
                    }

                    // Parsing class type
                    switch (classJson["disciplType"].ToString().TrimEnd(' '))
                    {
                        case "лек": scheduleClass.ClassType = ClassType.Lecture; break;
                        case "пр": scheduleClass.ClassType = ClassType.Practice; break;
                        case "л.р.": scheduleClass.ClassType = ClassType.Laboratory; break;
                        default: continue; //throw new NotSupportedException("Unknown disciplType");
                    }
                    
                    // TODO: Separate Organisation Unit and Auditorium
                    string temp = classJson["audNum"].ToString().TrimEnd(' ', '-');
                    scheduleClass.Auditorium = (temp == "") ? "на " + classJson["orgUnitName"].ToString().TrimEnd(' ') : "в аудитории №" + temp;

                    //scheduleClass.Auditorium = classJson["audNum"].ToString().TrimEnd(' ', '-');
                    //scheduleClass.OrganisationUnit = classJson["orgUnitName"].ToString().TrimEnd(' ');

                    // TODO: Add BuildNum and IsDistance 

                    scheduleClass.Time = classJson["dayTime"].ToString().TrimEnd(' ');
                    scheduleClass.Discipline = classJson["disciplName"].ToString();
                    scheduleClass.LecturerName = classJson["prepodName"].ToString();

                    scheduleDay.Classes.Add(scheduleClass);
                }
                schedule.Days.Add(scheduleDay);
            }

            return schedule;
        }
    }
}
