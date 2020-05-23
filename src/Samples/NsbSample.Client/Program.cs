namespace NsbSample.Client {
    using System;
    using System.Threading.Tasks;
    using NsbSample.Integration.Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;

    class Program {
        private static ILog log = LogManager.GetLogger<Program> ();

        static async Task Main () {
            Console.Title = "Nsb Sample - Client";

            var endpointConfiguration = new EndpointConfiguration ( "NsbSample.Client" );
            var transport = endpointConfiguration.UseTransport<LearningTransport> ();

            var endpointInstance = await Endpoint.Start ( endpointConfiguration )
                .ConfigureAwait ( false );

            await RunLoop ( endpointInstance )
                .ConfigureAwait ( false );

            await endpointInstance.Stop ()
                .ConfigureAwait ( false );
        }

        private static async Task RunLoop ( IEndpointInstance endpointInstance ) {
            while (true) {
                log.Info ( "Press 'S' say hello, or 'Q' to quit." );
                var key = Console.ReadKey ();
                Console.WriteLine ();

                switch (key.Key) {
                    case ConsoleKey.S:
                    var command = new SayHelloWorld {
                        WhoIsTheHelloDirectedAt = "Jane Doe"
                    };

                    // Send the command to the local endpoint
                    log.Info ( $"Sending SayHelloWorld command, to {command.WhoIsTheHelloDirectedAt}" );
                    await endpointInstance.SendLocal ( command )
                        .ConfigureAwait ( false );

                    break;

                    case ConsoleKey.Q:
                    return;

                    default:
                    log.Info ( "Unknown input. Please try again." );
                    break;
                }
            }
        }
    }
}
