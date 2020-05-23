namespace NsbSample.Integration.Messages.Commands {
    using NServiceBus;

    public class SayHelloWorld : ICommand {
        public string WhoIsTheHelloDirectedAt { get; set; }
    }
}
