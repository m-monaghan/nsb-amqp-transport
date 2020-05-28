namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Collections.Generic;
    using global::Amqp;
    using global::Amqp.Framing;
    using NServiceBus.Extensibility;

    public static class AmqpMessageExtensions {
        public static void PopulatePropertiesFromNsbMessage (
            this Message amqpMessage,
            OutgoingMessage outgoingMessage ) {

            var properties = new Properties {
                MessageId = outgoingMessage.MessageId
            };

            properties.ReplyTo = GetNsbHeaderValue (
                Headers.ReplyToAddress,
                outgoingMessage.Headers,
                properties.ReplyTo );
            properties.CorrelationId = GetNsbHeaderValue (
                Headers.CorrelationId,
                outgoingMessage.Headers,
                properties.CorrelationId );
            properties.CreationTime = GetNsbHeaderDateTimeValue (
                Headers.TimeSent,
                outgoingMessage.Headers,
                properties.CreationTime );
            properties.AbsoluteExpiryTime = GetNsbHeaderDateTimeValue (
                Headers.TimeToBeReceived,
                outgoingMessage.Headers,
                properties.AbsoluteExpiryTime );

            amqpMessage.Properties = properties;
        }

        public static void PopulateApplicationPropertiesFromNsbHeaders (
            this Message amqpMessage,
            Dictionary<string, string> headers ) {

            amqpMessage.ApplicationProperties = new ApplicationProperties ();
            foreach (var headerPropertyName in headers.Keys) {
                amqpMessage.ApplicationProperties[headerPropertyName] = headers[headerPropertyName];
            }
        }

        public static void SetBodyFromNsbMessage (
            this Message amqpMessage,
            OutgoingMessage outgoingMessage ) {

            amqpMessage.BodySection = new Data () {
                Binary = outgoingMessage.Body
            };
        }

        public static Dictionary<string, string> GetProperties (
            this Message amqpMessage ) {

            var headers = new Dictionary<string, string> ();
            headers.Add ( Headers.CorrelationId, amqpMessage.Properties.CorrelationId );
            headers.Add ( Headers.ReplyToAddress, amqpMessage.Properties.ReplyTo );

            if (amqpMessage.ApplicationProperties != null) {
                foreach (var prop in amqpMessage.ApplicationProperties.Map) {
                    if (headers.ContainsKey ( prop.Key.ToString () )) continue;
                    headers.Add ( prop.Key.ToString (), prop.Value.ToString () );
                }
            }

            return headers;
        }

        public static ContextBag GetNewContextBag ( this Message amqpMessage ) {
            var contextBag = new ContextBag ();
            contextBag.Set<Message> ( amqpMessage );
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

        private static DateTime GetNsbHeaderDateTimeValue (
                    string nsbHeaderPropertyName,
                    Dictionary<string, string> headers,
                    DateTime currentHeaderValue ) {
            if (!headers.TryGetValue ( nsbHeaderPropertyName, out string headerValue )) {
                return currentHeaderValue;
            }

            try {
                return DateTimeExtensions.ToUtcDateTime ( headerValue );
            }
            catch (InvalidCastException) {
                return currentHeaderValue;
            }
        }
    }
}
