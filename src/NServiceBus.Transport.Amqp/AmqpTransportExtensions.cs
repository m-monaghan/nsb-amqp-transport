namespace NServiceBus.Transport.Amqp {
    using System;
    using NServiceBus.Configuration.AdvancedExtensibility;

    public static class AmqpTransportExtensions {
        public static void SetUsernameAndPassword( this TransportExtensions<AmqpTransport> transport,
            string username,
            string password) {

            var settings = transport.GetSettings ();
            settings.Set ( "username", username );
            settings.Set ( "password", password );
        }
    }
}
