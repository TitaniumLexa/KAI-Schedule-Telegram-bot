using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using KAI_Schedule.Configuration;
using KAI_Schedule.Entities;
using Newtonsoft.Json;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace KAI_Schedule.Providers
{
    public class Telegram
    {
        private TelegramBotClient _telegramBotClient;
        private Schedule _schedule;

        private List<ChatSettings> _chatSettings = new List<ChatSettings>();
        private string _path = "chatSettings.json";

        public Telegram()
        {
            _telegramBotClient = new TelegramBotClient(ConfigManager.Config.TelegramToken);
            _schedule = new Schedule();

            ImportChatSettings();

            _telegramBotClient.OnMessage += OnMessage;
            _telegramBotClient.OnCallbackQuery += OnCallbackQuery;

            _telegramBotClient.StartReceiving();
        }

        private void ImportChatSettings()
        {
            if (System.IO.File.Exists(_path))
            {
                var json = System.IO.File.ReadAllText(_path, new UTF8Encoding(false));
                if (json.Length != 0)
                    _chatSettings = (List<ChatSettings>)JsonConvert.DeserializeObject(json, typeof(List<ChatSettings>));
            }
            //var sr = new StreamReader(".json");
            //string res = sr.ReadToEnd();
            //if (res.Length != 0)
            //    _chatSettings = (List<ChatSettings>)JsonConvert.DeserializeObject(res, typeof(List<ChatSettings>));
            //sr.Close();
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Chat chat = e.Message.Chat;
                string message = e.Message.Text;

                //await _telegramBotClient.SendTextMessageAsync(chat, "Clear", replyMarkup: new ReplyKeyboardRemove());

                if (message.Contains("/schedule"))
                {
                    string group = GetGroup(e.Message.Chat);
                    if (group == null)
                    {
                        await _telegramBotClient.SendTextMessageAsync(chat, "Сначала нужно настроить ваши данные. Используйте /setup.");
                        return;
                    }

                    var keyboard = new InlineKeyboardMarkup(new InlineKeyboardButton[][]
                        {
                            new[]
                            {
                               InlineKeyboardButton.WithCallbackData("Сегодня", "Today"),
                               InlineKeyboardButton.WithCallbackData("Завтра", "Tomorrow")
                            },
                            new[]
                            {
                               InlineKeyboardButton.WithCallbackData(DateTime.Now.AddDays(2).ToString("dd.MM"), "Date " + DateTime.Now.AddDays(2).ToString("dd.MM")),
                               InlineKeyboardButton.WithCallbackData(DateTime.Now.AddDays(3).ToString("dd.MM"), "Date " + DateTime.Now.AddDays(3).ToString("dd.MM")),
                               InlineKeyboardButton.WithCallbackData(DateTime.Now.AddDays(4).ToString("dd.MM"), "Date " + DateTime.Now.AddDays(4).ToString("dd.MM"))
                            }

                    });
                    await _telegramBotClient.SendTextMessageAsync(chat, "Выберите день:", replyMarkup: keyboard);



                }
                else if (message.Contains("/setup"))
                {
                    await _telegramBotClient.SendTextMessageAsync(chat, "Введите номер группы.");
                }
                else
                {
                    Regex regex = new Regex("[0-9]{4}");
                    string group;
                    if (regex.Match(message).Success)
                    {
                        group = regex.Match(message).Value;
                        Console.WriteLine("Группа: " + group);
                        await _telegramBotClient.SendTextMessageAsync(chat, "Хорошо. Теперь можно смотреть расписание.");
                        SetupCommand(chat, group);
                    }
                }
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (sender == null)
                return;

            string groupId = await _schedule.GetGroupID(GetGroup(e.CallbackQuery.Message.Chat));
            if (e.CallbackQuery.Data == DayType.Today.ToString())
            {
                string scheduleMessage = await _schedule.GetScheduleDay(groupId, DayType.Today);
                await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, scheduleMessage);
            }
            else if (e.CallbackQuery.Data == DayType.Tomorrow.ToString())
            {
                string scheduleMessage = await _schedule.GetScheduleDay(groupId, DayType.Tomorrow);
                await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, scheduleMessage);
            }
            else if (e.CallbackQuery.Data.StartsWith(DayType.Date.ToString()))
            {
                string scheduleMessage = await _schedule.GetScheduleDay(groupId, DayType.Date, e.CallbackQuery.Data.Remove(0, 5));
                await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, scheduleMessage);
            }
            await _telegramBotClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id);
        }

        private void SetupCommand(Chat chat, string group)
        {
            _chatSettings.Add(new ChatSettings()
            {
                Chat = chat,
                Group = group
            });

            string json = JsonConvert.SerializeObject(_chatSettings);
            System.IO.File.WriteAllText(_path, json, new UTF8Encoding(false));
            //var sw = new StreamWriter(_path, false);
            //string ser = JsonConvert.SerializeObject(_chatSettings);
            //sw.WriteLine(ser);
            //sw.Close();
        }

        public string GetGroup(Chat chat)
        {
            ChatSettings us = _chatSettings.Find(x => x.Chat.Id == chat.Id);

            if (us.Group != null)
                return us.Group;
            return null;
        }


    }
}
