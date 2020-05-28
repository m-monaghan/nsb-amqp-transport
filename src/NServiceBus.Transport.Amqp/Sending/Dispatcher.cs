/*
 * The dispatcher is responsible for translating a message (its binary body
 * and headers) and placing it onto the underlying transport technology.
 */
namespace NServiceBus.Transport.Amqp.Sending {
    using System.Threading.Tasks;
    using global::Amqp;
    using NServiceBus.Extensibility;
    using NServiceBus.Logging;

    sealed class Dispatcher : IDispatchMessages {
        static readonly ILog logger = LogManager.GetLogger<Dispatcher> ();
        private readonly Session session;
        private readonly SenderLinkCache senderLinkCache;

        public Dispatcher ( Session session ) {
            this.session = session;
            this.senderLinkCache = new SenderLinkCache ();
        }

        public Task Dispatch (
            TransportOperations outgoingMessages,
            TransportTransaction transaction,
            ContextBag context ) {

            foreach (var operation in outgoingMessages.UnicastTransportOperations) {
                var sender = this.senderLinkCache.GetSenderLink ( operation.Destination );
                if (sender == null)
                    sender = this.senderLinkCache.CacheSenderLink (
                        this.session,
                        operation.Destination,
                        operation.Destination );

                var amqpMessage = new Message ();
                var nsbMessage = operation.Message;

                amqpMessage.PopulatePropertiesFromNsbMessage ( nsbMessage );
                amqpMessage.PopulateApplicationPropertiesFromNsbHeaders (
                    nsbMessage.Headers );
                amqpMessage.SetBodyFromNsbMessage ( nsbMessage );

                try {
                    sender.Send ( amqpMessage );
                } catch(AmqpException amqpException) {

                    logger.Error ( "Could not send message", amqpException );
                    foreach(var keyName in amqpException.Data.Keys) {
                        logger.Info ( $"Error Data [{keyName}]=[{amqpException.Data[keyName]}]" );
                    }

                    if (sender.IsClosed) {
                        logger.Warn ( "SenderLink closed, refreshing" );
                    }

                    throw;
                }
            }

            return Task.CompletedTask;
        }
    }
}
