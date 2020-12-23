using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using KAI_Schedule.StateMachine;
using Telegram.Bot.Types;

namespace KAI_Schedule.Data
{
    public class ChatContext
    {
        [Key] public long ChatId { get; set; }
        public string Group { get; set; }
        public byte[] StateSerialized { get; set; }

        [NotMapped]
        public BaseState CurrentState
        {
            get => BaseState.Deserialize(StateSerialized);
            set => StateSerialized = BaseState.Serialize(value);
        }

        public virtual List<Subscription> Subscriptions { get; set; }
        
        public Chat GetChat()
        {
            return new Chat { Id = ChatId };
        }
    }
}
