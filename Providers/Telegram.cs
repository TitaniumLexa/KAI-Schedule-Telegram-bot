using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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

        // TODO: Perhaps make сommon class
        private List<ChatSettings> _chatSettings = new List<ChatSettings>();
        private List<SubscribeSettings> _subscribeSettings = new List<SubscribeSettings>();

        private string _chatSettingPath = "chatSettings.json";
        private string _subscribeSettingPath = "subscribeSettings.json";

        public Telegram()
        {
            _telegramBotClient = new TelegramBotClient(ConfigManager.Config.TelegramToken);
            _schedule = new Schedule();

            ImportChatSettings();
            ImportSubscribeSettings();

            _telegramBotClient.OnMessage += OnMessage;
            _telegramBotClient.OnCallbackQuery += OnCallbackQuery;

            _telegramBotClient.StartReceiving();
            CheckSubscribersAsync();
        }

        // TODO: Rewrite repeating methods
        private void ImportChatSettings()
        {
            if (System.IO.File.Exists(_chatSettingPath))
            {
                var json = System.IO.File.ReadAllText(_chatSettingPath, new UTF8Encoding(false));
                if (json.Length != 0)
                    _chatSettings = (List<ChatSettings>)JsonConvert.DeserializeObject(json, typeof(List<ChatSettings>));
            }
        }

        // TODO: Rewrite repeating methods
        private void ImportSubscribeSettings()
        {
            if (System.IO.File.Exists(_subscribeSettingPath))
            {
                var json = System.IO.File.ReadAllText(_subscribeSettingPath, new UTF8Encoding(false));
                if (json.Length != 0)
                    _subscribeSettings = (List<SubscribeSettings>)JsonConvert.DeserializeObject(json, typeof(List<SubscribeSettings>));
            }
        }

        // TODO: Write implementation
        private T ImportSettings<T>(string path)
        {
            throw new NotImplementedException();
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                Chat chat = e.Message.Chat;
                string message = e.Message.Text;

                Regex groupRegex = new Regex("[0-9]{4}");

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
                else if (message.Contains("/subscribe"))
                {
                    await _telegramBotClient.SendTextMessageAsync(chat, "Введите время получения ежедневных уведомлений. \n Рекомендуемый формат времени 'xx:yy'");
                }
                else if (message.Contains("/setup"))
                {
                    await _telegramBotClient.SendTextMessageAsync(chat, "Введите номер группы.");
                }
                else if (groupRegex.Match(message).Success)
                {
                    string group = groupRegex.Match(message).Value;
                    Console.WriteLine("Группа: " + group);
                    SaveChatSettings(chat, group);
                    await _telegramBotClient.SendTextMessageAsync(chat, "Хорошо. Теперь можно смотреть расписание.");
                }
                else if (TimeSpan.TryParse(message, out TimeSpan time))
                {
                    SaveSubscribeSettings(chat, time);
                    await _telegramBotClient.SendTextMessageAsync(chat,
                        $"Отлично. Теперь вы будете получать расписание в {time}.");
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

        private async Task CheckSubscribersAsync()
        {
            await Task.Run(CheckSubscribers);
        }

        // TODO: Add cancelation token
        private async void CheckSubscribers()
        {
            TimeSpan delay = TimeSpan.FromMinutes(1);
            while (true)
            {
                TimeSpan time = DateTime.Now - DateTime.Today;
                Console.WriteLine($"Checking subscribers. Time is {DateTime.Now}");
                foreach (var subscribeSetting in _subscribeSettings)
                {
                    if ((subscribeSetting.Time - time).Duration() <= delay.Divide(2))
                    {
                        string groupId = await _schedule.GetGroupID(GetGroup(subscribeSetting.Chat));
                        string scheduleMessage = await _schedule.GetScheduleDay(groupId, DayType.Tomorrow);

                        await _telegramBotClient.SendTextMessageAsync(subscribeSetting.Chat, scheduleMessage);
                    }
                }
                await Task.Delay(delay);
            }
        }

        // TODO: Add exist check
        private void SaveChatSettings(Chat chat, string group)
        {
            _chatSettings.Add(new ChatSettings
            {
                Chat = chat,
                Group = group
            });

            JsonWrite(_chatSettings, _chatSettingPath);
        }

        private void SaveSubscribeSettings(Chat chat, TimeSpan time)
        {
            _subscribeSettings.Add(new SubscribeSettings
            {
                Chat = chat,
                Time = time
            });

            JsonWrite(_subscribeSettings, _subscribeSettingPath);
        }

        private string GetGroup(Chat chat)
        {
            ChatSettings us = _chatSettings.Find(x => x.Chat.Id == chat.Id);

            //TODO: Think about simplification to 'return us.Group' instead of condition
            if (us.Group != null)
                return us.Group;
            return null;
        }

        private void JsonWrite(object value, string path)
        {
            string json = JsonConvert.SerializeObject(value);
            System.IO.File.WriteAllText(path, json, new UTF8Encoding(false));
        }
    }
}
