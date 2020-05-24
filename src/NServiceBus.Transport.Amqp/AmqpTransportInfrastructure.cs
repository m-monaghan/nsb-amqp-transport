namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using global::Amqp;
    using NServiceBus;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transport;

    sealed class AmqpTransportInfrastructure : TransportInfrastructure {

        readonly Address address;
        readonly Connection connection;
        readonly SettingsHolder settings;


        public AmqpTransportInfrastructure ( SettingsHolder settings, string connectionString ) {
            this.address = new Address ( connectionString );
            this.connection = new Connection ( address );
            this.settings = settings;
        }

        public override IEnumerable<Type> DeliveryConstraints => new List<Type> { typeof ( DiscardIfNotReceivedBefore ), typeof ( NonDurableDelivery ), typeof ( DoNotDeliverBefore ), typeof ( DelayDeliveryWith ) };

        public override OutboundRoutingPolicy OutboundRoutingPolicy {
            get {
                return new OutboundRoutingPolicy (
                    sends: OutboundRoutingType.Unicast,
                    publishes: OutboundRoutingType.Unicast,
                    replies: OutboundRoutingType.Unicast );
            }
        }

        public override TransportTransactionMode TransactionMode => TransportTransactionMode.ReceiveOnly;

        public override EndpointInstance BindToLocalEndpoint ( EndpointInstance instance ) => instance;

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure () {
            return new TransportReceiveInfrastructure (
                messagePumpFactory: () => new MessagePump ( this.connection ),
                queueCreatorFactory: () => new QueueCreator (),
                preStartupCheck: () => Task.FromResult ( StartupCheckResult.Success ) );
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure () {
            return new TransportSendInfrastructure (
                () => new Dispatcher (),
                preStartupCheck: () => Task.FromResult ( StartupCheckResult.Success ) );
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure () {
            throw new NotImplementedException ();
        }

        public override string ToTransportAddress ( LogicalAddress logicalAddress ) {
            var queue = new StringBuilder ( logicalAddress.EndpointInstance.Endpoint );

            if (logicalAddress.EndpointInstance.Discriminator != null) {
                queue.Append ( "-" + logicalAddress.EndpointInstance.Discriminator );
            }

            if (logicalAddress.Qualifier != null) {
                queue.Append ( "." + logicalAddress.Qualifier );
            }

            return queue.ToString ();
        }
    }
}
