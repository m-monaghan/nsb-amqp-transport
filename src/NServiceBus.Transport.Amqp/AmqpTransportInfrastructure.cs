﻿namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Apache.NMS;
    using Apache.NMS.AMQP;
    using NServiceBus;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Logging;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transport;
    using NServiceBus.Transport.Amqp.Receiving;
    using NServiceBus.Transport.Amqp.Sending;

    sealed class AmqpTransportInfrastructure : TransportInfrastructure {
        static readonly ILog logger = LogManager.GetLogger<AmqpTransportInfrastructure> ();

        readonly NmsConnectionFactory factory;
        readonly IConnection connection;
        readonly ISession session;
        readonly SettingsHolder settings;

        public AmqpTransportInfrastructure ( SettingsHolder settings, string connectionString ) {
            this.factory = new NmsConnectionFactory ( connectionString );
            this.connection = this.factory.CreateConnection ( "guest", "guest" );
            this.session = connection.CreateSession ( AcknowledgementMode.AutoAcknowledge );
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
                messagePumpFactory: () => new MessagePump ( this.session ),
                queueCreatorFactory: () => new QueueCreator (),
                preStartupCheck: () => Task.FromResult ( StartupCheckResult.Success ) );
        }

        public override TransportSendInfrastructure ConfigureSendInfrastructure () {
            return new TransportSendInfrastructure (
                () => new Dispatcher ( this.session ),
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

        public override Task Start () {
            logger.Info ("Starting AMQP transport");
            this.connection.Start ();
            return base.Start ();
        }

        public override async Task Stop () {
            logger.Info ( "Stopping AMQP transport" );
            this.session.Close ();
            this.connection.Close ();
            await base.Stop ();
        }
    }
}
