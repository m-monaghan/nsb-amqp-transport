namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Collections.Generic;
    using Apache.NMS;
    using NServiceBus.Extensibility;
    using IMessage = Apache.NMS.IMessage;

    public static class AmqpMessageExtensions {
        public static void PopulatePropertiesFromNsbMessage (
            this IMessage amqpMessage,
            OutgoingMessage outgoingMessage,
            ISession session ) {

            amqpMessage.NMSMessageId = outgoingMessage.MessageId;
            amqpMessage.NMSCorrelationID = GetNsbHeaderValue (
                Headers.CorrelationId,
                outgoingMessage.Headers,
                amqpMessage.NMSCorrelationID );
            var replyToQueueName = GetNsbHeaderValue (
                Headers.ReplyToAddress,
                outgoingMessage.Headers,
                string.Empty );
            if (!string.IsNullOrEmpty ( replyToQueueName ))
                amqpMessage.NMSReplyTo = session.GetQueue ( replyToQueueName );

            if (outgoingMessage.Headers.ContainsKey( Headers.NonDurableMessage ))
                amqpMessage.NMSDeliveryMode = MsgDeliveryMode.NonPersistent;

            amqpMessage.NMSTimestamp = GetNsbHeaderValue (
                Headers.TimeSent,
                outgoingMessage.Headers,
                amqpMessage.NMSTimestamp );
        }

        public static void PopulateApplicationPropertiesFromNsbHeaders (
            this IMessage amqpMessage,
            Dictionary<string, string> headers ) {

            foreach (var headerPropertyName in headers.Keys) {
                if (!amqpMessage.Properties.Contains ( headerPropertyName ))
                    amqpMessage.Properties.SetString ( headerPropertyName,
                        headers[headerPropertyName] );
            }
        }

        public static Dictionary<string, string> GetProperties (
            this IMessage amqpMessage ) {

            var headers = new Dictionary<string, string> ();
            headers.Add ( Headers.CorrelationId, amqpMessage.NMSCorrelationID );

            if (amqpMessage.NMSReplyTo != null) {
                if (amqpMessage.NMSReplyTo.IsQueue)
                    headers.Add ( Headers.ReplyToAddress,
                        ((IQueue) amqpMessage.NMSReplyTo).QueueName );
                else if (amqpMessage.NMSReplyTo.IsTopic)
                    headers.Add ( Headers.ReplyToAddress,
                        ( (ITopic)amqpMessage.NMSReplyTo ).TopicName );
            }

            foreach (var keyName in amqpMessage.Properties.Keys) {
                if (headers.ContainsKey ( keyName.ToString() )) continue;
                headers.Add ( keyName.ToString(), amqpMessage.Properties[keyName.ToString()].ToString() );
            }

            return headers;
        }

        public static string GetMessageId(this IMessage amqpMessage) {
            return amqpMessage.NMSMessageId;
        }

        public static ContextBag GetContextBag(this IMessage amqpMessage) {
            var contextBag = new ContextBag ();
            contextBag.Set ( amqpMessage );
            return contextBag;
        }

        private static string GetNsbHeaderValue (
            string nsbHeaderPropertyName,
            Dictionary<string, string> headers,
            string currentHeaderValue ) {
            string headerValue;

            if (headers.TryGetValue ( nsbHeaderPropertyName, out headerValue ))
                return headerValue;

            return currentHeaderValue;
        }

        private static DateTime GetNsbHeaderValue (
            string nsbHeaderPropertyName,
            Dictionary<string, string> headers,
            DateTime currentHeaderValue ) {
            string headerValue;

            if (!headers.TryGetValue ( nsbHeaderPropertyName, out headerValue ))
                return currentHeaderValue;

            try {
                return DateTimeExtensions.ToUtcDateTime ( headerValue );
            } catch (Exception) {
                return currentHeaderValue;
            }
        }
    }
}
