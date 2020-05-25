/*
 * The dispatcher is responsible for translating a message (its binary body
 * and headers) and placing it onto the underlying transport technology.
 */
namespace NServiceBus.Transport.Amqp.Sending {
    using System.Threading.Tasks;
    using global::Amqp;
    using NServiceBus.Extensibility;

    sealed class Dispatcher : IDispatchMessages {
        private Session session;
        private SenderLinkCache senderLinkCache;

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

                sender.Send ( amqpMessage );
            }

            return Task.CompletedTask;
        }
    }
}
