namespace NsbAmqpSample.Server {
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NsbSample.Integration.Messages.Commands;
    using NServiceBus;
    using NServiceBus.Logging;

    public class SayHelloWorldHandler :
        IHandleMessages<SayHelloWorld> {

        static ILog log = LogManager.GetLogger<SayHelloWorldHandler> ();

        public SayHelloWorldHandler () {
        }

        public Task Handle (
            SayHelloWorld message,
            IMessageHandlerContext context ) {
            log.Info ( $"Hello {message.WhoIsTheHelloDirectedAt}, top " +
                $"of the world to you! FLR [{GetHeaderValue ( context.MessageHeaders, Headers.ImmediateRetries )}] " +
                $"Deferred [{GetHeaderValue ( context.MessageHeaders, Headers.IsDeferredMessage )}] " +
                $"Retries [{GetHeaderValue ( context.MessageHeaders, Headers.DelayedRetries )}] "
            );

            // throw new System.Exception ( "Purposful exception" );
            return Task.CompletedTask;
        }

        private string GetHeaderValue ( IReadOnlyDictionary<string, string> headers, string name ) {
            if (headers.TryGetValue ( name, out string value )) {
                return value;
            }

            return string.Empty;
        }
    }
}
