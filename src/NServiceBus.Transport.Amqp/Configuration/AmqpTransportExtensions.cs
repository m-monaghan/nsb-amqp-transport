namespace NServiceBus.Transport.Amqp {
    using NServiceBus.Configuration.AdvancedExtensibility;
    using SettingsKeys = Configuration.SettingsKeys;

    public static class AmqpTransportExtensions {
        // <summary>
        /// Sets the username and password to use when creating a connection
        /// </summary>
        /// <param name="transportExtensions"></param>
        /// <param name="username">The username for the connection</param>
        /// <param name="password">The password for the connection</param>
        public static void SetUsernameAndPassword( this TransportExtensions<AmqpTransport> transportExtensions,
            string username,
            string password) {

            var settings = transportExtensions.GetSettings ();
            settings.Set ( SettingsKeys.Username, username );
            settings.Set ( SettingsKeys.Password, password );
        }
    }
}
