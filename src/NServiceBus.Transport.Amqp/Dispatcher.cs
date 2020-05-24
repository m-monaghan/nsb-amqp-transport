/*
 * The dispatcher is responsible for translating a message (its binary body
 * and headers) and placing it onto the underlying transport technology.
 */
namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;

    public class Dispatcher {
        public Task Dispatch ( TransportOperations outgoingMessages,
            TransportTransaction transaction,
            ContextBag context ) {

            foreach (var operation in outgoingMessages.UnicastTransportOperations) {
                Console.WriteLine ( $"Sending message to queue [{operation.Destination}]" );

                //File.WriteAllBytes ( bodyPath, operation.Message.Body );
            }

            return Task.CompletedTask;
        }
    }
}
