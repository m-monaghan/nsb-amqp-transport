/*
 * The dispatcher is responsible for translating a message (its binary body
 * and headers) and placing it onto the underlying transport technology.
 */
namespace NServiceBus.Transport.Amqp.Sending {
    using System.Threading.Tasks;
    using Apache.NMS;
    using NServiceBus.Extensibility;

    sealed class Dispatcher : IDispatchMessages {
        private ISession session;

        public Dispatcher ( ISession session ) {
            this.session = session;
        }

        public Task Dispatch (
            TransportOperations outgoingMessages,
            TransportTransaction transaction,
            ContextBag context ) {

            foreach (var operation in outgoingMessages.UnicastTransportOperations) {
                var queue = this.session.GetQueue ( operation.Destination );
                var producer = this.session.CreateProducer ( queue );
                producer.DeliveryMode = MsgDeliveryMode.NonPersistent;

                var nsbMessage = operation.Message;
                var amqpMessage = producer.CreateBytesMessage ( nsbMessage.Body );

                amqpMessage.PopulatePropertiesFromNsbMessage ( nsbMessage );
                amqpMessage.PopulateApplicationPropertiesFromNsbHeaders (
                    nsbMessage.Headers );

                producer.Send ( amqpMessage );
            }

            return Task.CompletedTask;
        }
    }
}
