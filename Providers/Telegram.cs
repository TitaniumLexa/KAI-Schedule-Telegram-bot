using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using KAI_Schedule.Configuration;
using KAI_Schedule.Data;
using KAI_Schedule.Entities;
using KAI_Schedule.StateMachine;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace KAI_Schedule.Providers
{
    public class Telegram
    {
        private readonly TelegramBotClient _telegramBotClient;
        private readonly ScheduleProvider _scheduleProvider;

        public Telegram()
        {
            _telegramBotClient = new TelegramBotClient(ConfigManager.Config.TelegramToken);
            _scheduleProvider = new ScheduleProvider();

            _telegramBotClient.OnMessage += OnMessage;
            _telegramBotClient.OnCallbackQuery += OnCallbackQuery;

            _telegramBotClient.StartReceiving();
            CheckSubscribersAsync();
        }

        public async Task SendTextMessageAsync(Chat chat, string message)
        {
            await _telegramBotClient.SendTextMessageAsync(chat, message);
        }

        public async Task SendTextMessageAsync(Chat chat, string message, IReplyMarkup markup)
        {
            await _telegramBotClient.SendTextMessageAsync(chat, message, replyMarkup: markup);
        }

        private async void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.Message.Text != null)
            {
                var chat = e.Message.Chat;
                var message = e.Message.Text;

                await using var dbContext = new ApplicationDbContext();

                var chatContext = dbContext.ChatContexts
                    .Include(context => context.Subscriptions)
                    .FirstOrDefault(context => context.ChatId == chat.Id);

                if (chatContext == null)
                {
                    chatContext = new ChatContext() { ChatId = chat.Id, CurrentState = new CommandState()};
                    dbContext.ChatContexts.Add(chatContext);
                }

                await chatContext.CurrentState.Handle(chatContext, message, this);
                await dbContext.SaveChangesAsync();
            }
        }

        private async void OnCallbackQuery(object sender, CallbackQueryEventArgs e)
        {
            if (sender == null)
                return;

            try
            {
                await _telegramBotClient.AnswerCallbackQueryAsync(e.CallbackQuery.Id, "Секунду...");
            }
            catch (InvalidParameterException exception)
            {
                Console.WriteLine($"Шеф, все пропало: {exception.Message}");
                return;
            }

            await using var dbContext = new ApplicationDbContext();

            var chatContext = dbContext.ChatContexts.FirstOrDefault(context => context.ChatId == e.CallbackQuery.Message.Chat.Id);
            string group = chatContext?.Group;

            if (group == null)
                return;
            var scheduleEntry = dbContext.Schedules.FirstOrDefault(entry => entry.Group == group);

            var schedule = scheduleEntry?.Schedule;
            if (schedule == null)
            {
                string groupId = await _scheduleProvider.GetGroupId(group);
                schedule = await _scheduleProvider.ParseScheduleByGroup(groupId);

                if (schedule == null)
                {
                    await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, "Произошла ошибка при получении расписания, извините.");
                    return;
                }

                var scheduleDbEntry = new ScheduleDbEntry()
                {
                    Group = group,
                    LastUpdate = DateTime.Now,
                    Schedule = schedule
                };
                dbContext.Schedules.Add(scheduleDbEntry);
            }
            
            if (e.CallbackQuery.Data == DayType.Today.ToString())
            {
                await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, schedule.OutputDay(DayType.Today));
            }
            else if (e.CallbackQuery.Data == DayType.Tomorrow.ToString())
            {
                await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, schedule.OutputDay(DayType.Tomorrow));
            }
            else if (e.CallbackQuery.Data.StartsWith(DayType.Date.ToString()))
            {
                await _telegramBotClient.SendTextMessageAsync(e.CallbackQuery.Message.Chat, schedule.OutputDay(DayType.Date, e.CallbackQuery.Data.Remove(0, 5)));
            }
            
            await dbContext.SaveChangesAsync();
        }


        private async Task CheckSubscribersAsync()
        {
            TimeSpan delay = TimeSpan.FromMinutes(1);
            while (true)
            {
                await using var dbContext = new ApplicationDbContext();

                var currentTime = DateTime.Now;
                var currentDay = DateTime.Today;

                var stopwatch = Stopwatch.StartNew();

                var subscriptions = dbContext.Subscriptions
                    .Include(subscription => subscription.ChatContext);

                stopwatch.Stop();
                Console.WriteLine($"Checking subscribers. Time is {DateTime.Now}. Pending subscriptions count is {subscriptions.Count()}. Elapsed time {stopwatch.ElapsedMilliseconds} ms");

                foreach (var subscription in subscriptions)
                {
                    if (subscription.LastNotificationTime + subscription.NotificationInterval > currentTime)
                    {
                        continue;
                    }

                    var chatContext = subscription.ChatContext;
                    var group = chatContext.Group;

                    var scheduleEntry = dbContext.Schedules.FirstOrDefault(entry => entry.Group == group);
                    
                    var schedule = scheduleEntry?.Schedule;
                    if (schedule == null)
                    {
                        string groupId = await _scheduleProvider.GetGroupId(group);
                        schedule = await _scheduleProvider.ParseScheduleByGroup(groupId);

                        if (schedule == null)
                        {
                            await _telegramBotClient.SendTextMessageAsync(chatContext.GetChat(), "Произошла ошибка при получении расписания, извините.");
                            return;
                        }

                        var scheduleDbEntry = new ScheduleDbEntry()
                        {
                            Group = group,
                            LastUpdate = DateTime.Now,
                            Schedule = schedule
                        };
                        dbContext.Schedules.Add(scheduleDbEntry);
                    }

                    await _telegramBotClient.SendTextMessageAsync(chatContext.GetChat(), schedule.OutputDay(DayType.Today));
                    subscription.LastNotificationTime = currentDay + subscription.NotificationTime;
                }

                await dbContext.SaveChangesAsync();
                await Task.Delay(delay);
            }
        }
    }
}
