namespace NServiceBus.Transport.Amqp {
    using NServiceBus.Settings;

    public class AmqpTransport : TransportDefinition {
        public override bool RequiresConnectionString => true;

        public override TransportInfrastructure Initialize ( SettingsHolder settings, string connectionString ) {
            return new AmqpTransportInfrastructure ( settings, connectionString );
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "amqp://localhost:5672";

        public string Username { get; set; }
        public string Password { get; set; }
    }
}
