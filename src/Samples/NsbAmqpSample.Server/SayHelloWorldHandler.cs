namespace NsbAmqpSample.Server {
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
            log.Info ( $"Hello {message.WhoIsTheHelloDirectedAt}, top of the world to you!" );
            return Task.CompletedTask;
        }
    }
}
