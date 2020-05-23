namespace NsbSample.Server {
    using System;
    using System.Threading.Tasks;
    using NServiceBus;

    class Program {
        static async Task Main () {
            Console.Title = "Nsb Sample - Server";

            var endpointConfiguration = new EndpointConfiguration ( "NsbSample.Server" );
            var transport = endpointConfiguration.UseTransport<LearningTransport> ();

            var endpointInstance = await Endpoint.Start ( endpointConfiguration )
                .ConfigureAwait ( false );

            Console.WriteLine ( "Press Enter to exit the server ..." );
            Console.ReadLine ();

            await endpointInstance.Stop ()
                .ConfigureAwait ( false );
        }
    }
}
