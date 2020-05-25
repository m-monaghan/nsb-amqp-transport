/*
 * The message pump is responsible for reading messages from the underlying
 * transport and pushing them into the message handling pipeline.
 */
namespace NServiceBus.Transport.Amqp.Receiving {
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Amqp;
    using NServiceBus;
    using NServiceBus.Extensibility;
    using NServiceBus.Logging;
    using NServiceBus.Transport;

    sealed class MessagePump : IPushMessages {
        static readonly ILog logger = LogManager.GetLogger<MessagePump> ();
        static readonly TransportTransaction transportTransaction = new TransportTransaction ();

        private string messagePumpName;
        private Session session;
        private ReceiverLink receiver;
        private Func<MessageContext, Task> passMessageToNsb;
        private Func<ErrorContext, Task<ErrorHandleResult>> letNsbKnowAboutAnError;
        private CriticalError criticalError;
        private PushSettings nsbSettings;

        public MessagePump ( Session session ) {
            this.session = session;
        }

        public Task Init (
            Func<MessageContext, Task> onMessage,
            Func<ErrorContext, Task<ErrorHandleResult>> onError,
            CriticalError criticalError,
            PushSettings settings ) {

            this.passMessageToNsb = onMessage;
            this.letNsbKnowAboutAnError = onError;
            this.criticalError = criticalError;
            this.nsbSettings = settings;
            this.messagePumpName = $"MessagePump-{this.nsbSettings.InputQueue}";

            return Task.CompletedTask;
        }

        public void Start ( PushRuntimeSettings limitations ) {
            logger.Info ( $"Starting MessagePump for {this.messagePumpName}" );

            this.receiver = new ReceiverLink ( this.session,
                this.messagePumpName,
                this.nsbSettings.InputQueue );

            this.receiver.Start ( 10, async ( link, message ) => {
                try {
                    await ProcessMessageAsync ( message ).ConfigureAwait ( false );
                    link.Accept ( message );
                }
                catch (Exception e) {
                    logger.Error ( $"Processing a message", e );
                    link.Reject ( message );
                    return;
                }
            } );

        }

        public async Task Stop () {
            logger.Info ( $"Stopping MessagePump for {this.messagePumpName}" );
            await this.receiver.CloseAsync ();
        }

        private async Task ProcessMessageAsync ( Message message ) {
            var headers = message.GetProperties ();
            string messageId = message.Properties.MessageId;
            var contextBag = message.GetContextBag ();
            using var tokenSource = new CancellationTokenSource ();
            var messageContext = new MessageContext (
                messageId,
                headers,
                (byte[])message.Body,
                transportTransaction,
                tokenSource,
                contextBag );

            await this.passMessageToNsb ( messageContext ).ConfigureAwait ( false );
        }
    }
}
