namespace NServiceBus.Transport.Amqp {
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Transport;

    sealed class QueueCreator : ICreateQueues {
        public Task CreateQueueIfNecessary ( QueueBindings queueBindings, string identity ) {
            foreach (var address in queueBindings.SendingAddresses) {
                Console.WriteLine ($"Would create sending address [${address}");
            }

            foreach (var address in queueBindings.ReceivingAddresses) {
                Console.WriteLine ( $"Would create receiving address [${address}" );
            }

            return Task.CompletedTask;
        }
    }
}
