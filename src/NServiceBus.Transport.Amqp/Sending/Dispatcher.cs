/*
 * The dispatcher is responsible for translating a message (its binary body
 * and headers) and placing it onto the underlying transport technology.
 */
namespace NServiceBus.Transport.Amqp.Sending {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Amqp;
    using global::Amqp.Framing;
    using NServiceBus.Extensibility;

    sealed class Dispatcher : IDispatchMessages, IDisposable {
        private Session session;
        private Dictionary<string, SenderLink> senderLinkCache = new Dictionary<string, SenderLink> ();

        public Dispatcher ( Session session ) {
            this.session = session;
        }

        public Task Dispatch ( TransportOperations outgoingMessages,
            TransportTransaction transaction,
            ContextBag context ) {

            foreach (var operation in outgoingMessages.UnicastTransportOperations) {
                var sender = this.GetSenderLink ( operation.Destination );
                var messageToAmqp = new Message ();
                var nsbMessage = operation.Message;

                var properties = new Properties {
                    MessageId = nsbMessage.MessageId
                };

                properties.ReplyTo = GetNsbHeaderValue ( nsbMessage.Headers, properties.ReplyTo, Headers.ReplyToAddress );
                properties.CorrelationId = GetNsbHeaderValue ( nsbMessage.Headers, properties.CorrelationId, Headers.CorrelationId );

                messageToAmqp.Properties = properties;
                messageToAmqp.BodySection = new Data () { Binary = operation.Message.Body };

                sender.Send ( messageToAmqp );
            }

            return Task.CompletedTask;
        }

        public void Dispose () {
            foreach (var senderLink in this.senderLinkCache.Values) {
                senderLink.Close ();
            }
        }

        private string GetNsbHeaderValue ( Dictionary<string, string> headers, string currentHeaderValue, string nsbHeaderPropertyName ) {
            string headerValue;

            if (headers.TryGetValue ( nsbHeaderPropertyName, out headerValue ))
                return headerValue;

            return currentHeaderValue;
        }

        private SenderLink GetSenderLink ( string destinationQueue ) {
            SenderLink senderLink;

            if (this.senderLinkCache.TryGetValue ( $"nsb-{destinationQueue}", out senderLink ))
                return senderLink;

            senderLink = new SenderLink ( this.session, $"nsb-{destinationQueue}", destinationQueue );
            this.senderLinkCache.Add ( $"nsb-{destinationQueue}", senderLink );

            return senderLink;
        }
    }
}
