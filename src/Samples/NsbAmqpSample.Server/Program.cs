namespace NsbAmpqSample.Server {
    using System;
    using System.Threading.Tasks;
    using NServiceBus;
    using NServiceBus.Features;
    using NServiceBus.Transport.Amqp;

    class Program {
        static async Task Main () {
            Console.Title = "Nsb Sample - Server";

            var endpointConfiguration = new EndpointConfiguration ( "NsbSample.Server" );
            var transport = endpointConfiguration.UseTransport<AmqpTransport> ();
            transport.ConnectionString ( "amqp://localhost:5672" );
            transport.SetUsernameAndPassword ( "guest", "guest" );

            endpointConfiguration.UsePersistence<InMemoryPersistence> ();
            endpointConfiguration.DisableFeature<TimeoutManager> ();

            var endpointInstance = await Endpoint.Start ( endpointConfiguration )
                .ConfigureAwait ( false );

            Console.WriteLine ( "Press Enter to exit the server ..." );
            Console.ReadLine ();

            await endpointInstance.Stop ()
                .ConfigureAwait ( false );
        }
    }
}
