﻿using KAI_Schedule.Entities;

namespace KAI_Schedule.Configuration
{
    public class Config
    {
        public string TelegramToken { get; set; }
        public RequestURLs RequestURLs { get; set; }

        public Config()
        {
            TelegramToken = "ChangeIt";
            RequestURLs = new RequestURLs
            {
                GroupID = "https://kai.ru/raspisanie?p_p_id=pubStudentSchedule_WAR_publicStudentSchedule10&p_p_lifecycle=2&p_p_resource_id=getGroupsURL&query=",
                Schedule = "https://kai.ru/raspisanie?p_p_id=pubStudentSchedule_WAR_publicStudentSchedule10&p_p_lifecycle=2&p_p_resource_id=schedule&groupId="
            };
        }
    }

    
}
