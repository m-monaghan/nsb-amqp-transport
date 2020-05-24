namespace NServiceBus.Transport.Amqp {
    using NServiceBus.Settings;

    public class AmqpTransport : TransportDefinition {
        public override bool RequiresConnectionString => true;

        public override TransportInfrastructure Initialize ( SettingsHolder settings, string connectionString ) {
            return new AmqpTransportInfrastructure ();
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "amqp://guest:guest@localhost:5672";
    }
}
