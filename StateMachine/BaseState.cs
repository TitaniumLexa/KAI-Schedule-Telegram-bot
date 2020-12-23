using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using KAI_Schedule.Data;

namespace KAI_Schedule.StateMachine
{
    [Serializable]
    public abstract class BaseState
    {
        public abstract Task Handle(ChatContext context, string message, Providers.Telegram telegram);
        
        public static byte[] Serialize(BaseState state)
        {
            var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, state);

            return stream.ToArray();
        }

        public static BaseState Deserialize(byte[] binary)
        {
            if (binary.Length == 0)
                return null;

            var stream = new MemoryStream(binary);
            var formatter = new BinaryFormatter();

            return (BaseState)formatter.Deserialize(stream);
        }
    }
}
