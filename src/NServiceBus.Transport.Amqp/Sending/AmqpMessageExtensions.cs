namespace NServiceBus.Transport.Amqp.Sending {
    using System;
    using System.Collections.Generic;
    using global::Amqp;
    using global::Amqp.Framing;

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

        private static string GetNsbHeaderValue (
            string nsbHeaderPropertyName,
            Dictionary<string, string> headers,
            string currentHeaderValue ) {
            string headerValue;

            if (headers.TryGetValue ( nsbHeaderPropertyName, out headerValue ))
                return headerValue;

            return currentHeaderValue;
        }

    }
}
