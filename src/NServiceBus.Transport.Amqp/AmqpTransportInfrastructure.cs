namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::Amqp;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;

    public class AmqpTransportInfrastructure : TransportInfrastructure {

        readonly Address address;
        readonly Connection connection;


        public AmqpTransportInfrastructure ( SettingsHolder settings, string connectionString ) {
            this.address = new Address ( connectionString );
            this.connection = new Connection ( address );
        }

        public override IEnumerable<Type> DeliveryConstraints => new List<Type> { typeof ( DiscardIfNotReceivedBefore ), typeof ( NonDurableDelivery ), typeof ( DoNotDeliverBefore ), typeof ( DelayDeliveryWith ) };

        public override OutboundRoutingPolicy OutboundRoutingPolicy => new OutboundRoutingPolicy ( OutboundRoutingType.Unicast, OutboundRoutingType.Multicast, OutboundRoutingType.Unicast );

        public override TransportTransactionMode TransactionMode => TransportTransactionMode.ReceiveOnly;

        public override EndpointInstance BindToLocalEndpoint ( EndpointInstance instance ) => instance;

        public override TransportReceiveInfrastructure ConfigureReceiveInfrastructure () {
            return new TransportReceiveInfrastructure (
                messagePumpFactory: () => new MessagePump ( this.connection ),
                queueCreatorFactory: () => new QueueCreator (),
                preStartupCheck: () => Task.FromResult ( StartupCheckResult.Success ) );
        }

        public override TransportSubscriptionInfrastructure ConfigureSubscriptionInfrastructure () {
            throw new NotImplementedException ();
        }
    }
}
