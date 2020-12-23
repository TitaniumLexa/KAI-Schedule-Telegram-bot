using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KAI_Schedule.Data;
using Telegram.Bot.Types.ReplyMarkups;

namespace KAI_Schedule.StateMachine
{
    [Serializable]
    public class CommandState : BaseState
    {
        public override async Task Handle(ChatContext context, string message, Providers.Telegram telegram)
        {
            if (message.Contains("/schedule"))
            {
                context.CurrentState = new ScheduleState();

                await context.CurrentState.Handle(context, message, telegram);
            }
            else if (message.Contains("/subscribe"))
            {
                context.CurrentState = new SubscribeState();

                await context.CurrentState.Handle(context, message, telegram);
            }
            else if (message.Contains("/setup"))
            {
                context.CurrentState = new SetupState();

                await context.CurrentState.Handle(context, message, telegram);
            }
        }
    }

    [Serializable]
    public class SetupState : BaseState
    {
        public override async Task Handle(ChatContext context, string message, Providers.Telegram telegram)
        {
            var groupRegex = new Regex("[0-9]{4}");
            var match = groupRegex.Match(message);

            if (match.Success)
            {
                string group = match.Value;
                context.Group = group;

                await telegram.SendTextMessageAsync(context.GetChat(), "Хорошо. Теперь можно смотреть расписание.");

                context.CurrentState = new CommandState();
            }
            else if (message.Contains("/quit"))
            {
                context.CurrentState = new CommandState();
            }
            else
            {
                await telegram.SendTextMessageAsync(context.GetChat(), "Введите номер группы или /quit для выхода из режима настройки.");
            }
        }
    }

    [Serializable]
    public class ScheduleState : BaseState
    {
        public override async Task Handle(ChatContext context, string message, Providers.Telegram telegram)
        {
            string group = context.Group;
            if (group == null)
            {
                await telegram.SendTextMessageAsync(context.GetChat(), "Сначала нужно настроить ваши данные. Используйте /setup.");

                context.CurrentState = new CommandState();
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

            await telegram.SendTextMessageAsync(context.GetChat(), "Выберите день:", keyboard);

            context.CurrentState = new CommandState();
        }
    }

    [Serializable]
    public class SubscribeState : BaseState
    {
        public override async Task Handle(ChatContext context, string message, Providers.Telegram telegram)
        {
            if (message.Contains("/subscribe"))
            {
                await telegram.SendTextMessageAsync(context.GetChat(), "Введите время получения ежедневных уведомлений. \n Рекомендуемый формат времени 'hh:mm'");
            }
            else if (message.Contains("/quit"))
            {
                context.CurrentState = new CommandState();
            }
            else if (TimeSpan.TryParse(message, out TimeSpan time))
            {
                var interval = TimeSpan.FromDays(1);
                var lastNotification = time >= DateTime.Now.TimeOfDay ?  DateTime.Today + time - interval : DateTime.Today + time;

                var subscription = new Subscription()
                {
                    ChatContext = context,
                    NotificationTime = time,
                    LastNotificationTime = lastNotification,
                    NotificationInterval = interval

                };
                context.Subscriptions.Add(subscription);

                await telegram.SendTextMessageAsync(context.GetChat(),
                    $"Отлично. Теперь вы будете получать расписание в {time}.");

                context.CurrentState = new CommandState();
            }
            else
            {
                await telegram.SendTextMessageAsync(context.GetChat(), "Введите время для уведомлений или /quit для выхода из режима настройки.");
            }
        }
    }
}
