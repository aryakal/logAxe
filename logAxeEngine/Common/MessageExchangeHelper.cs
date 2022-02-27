//=====================================================================================================================
// Source : https://github.com/aryakal/
//--------------------------------------------------------------------------------------------------------------------
//=====================================================================================================================

using logAxeEngine.Interfaces;
using logAxeEngine.EventMessages;

namespace logAxeEngine.Common
{
    public class MessageExchangeHelper
    {
        private IMessageBroker _msgEngine;
        public string ClientId { get; }

        public MessageExchangeHelper(IMessageBroker msgEngine, IMessageReceiver receiver)
        {
            _msgEngine = msgEngine;
            ClientId = _msgEngine.RegisterClient(receiver);
        }

        public void Unregister()
        {
            _msgEngine.UnregisterClient(ClientId);
        }

        public void PostMessage(ILogAxeMessage message)
        {
            message.FromClientID = ClientId;
            _msgEngine.SendMessage(message);
        }

        public void PostMessage(LogAxeMessageEnum eventType)
        {
            _msgEngine.SendMessage(
                new LogAxeGenericMessage()
                {
                    FromClientID = ClientId,
                    MessageType = eventType
                });
        }
    }
}
