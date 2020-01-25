using GrimDamage.GD.Dto;

namespace GrimDamage.GD.Processors
{
    interface IMessageProcessor {
        bool Process(MessageType type, byte[] data);
    }
}
