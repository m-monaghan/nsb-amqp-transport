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
    using Apache.NMS;
    using NServiceBus;
    using NServiceBus.Extensibility;
    using NServiceBus.Logging;
    using NServiceBus.Transport;
    using IMessage = Apache.NMS.IMessage;

    sealed class MessagePump : IPushMessages {
        static readonly ILog logger = LogManager.GetLogger<MessagePump> ();
        static readonly TransportTransaction transportTransaction = new TransportTransaction ();

        private string messagePumpName;
        private ISession session;
        private IQueue queue;
        private IMessageConsumer consumer;

        private Func<MessageContext, Task> passMessageToNsb;
        private Func<ErrorContext, Task<ErrorHandleResult>> letNsbKnowAboutAnError;
        private CriticalError criticalError;
        private PushSettings nsbSettings;

        public MessagePump ( ISession session ) {
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

            this.queue = session.GetQueue ( this.nsbSettings.InputQueue );
            this.consumer = session.CreateConsumer ( queue );
            this.consumer.Listener += HandleConsumerMessage;

            return Task.CompletedTask;
        }

        public void Start ( PushRuntimeSettings limitations ) {
            logger.Info ( $"Starting MessagePump for {this.messagePumpName}" );
        }

        public Task Stop () {
            logger.Info ( $"Stopping MessagePump for {this.messagePumpName}" );
            return Task.CompletedTask;
        }

        private async void HandleConsumerMessage ( IMessage message ) {
            await this.ProcessMessageAsync ( message ).ConfigureAwait ( false );
        }

        private async Task ProcessMessageAsync ( IMessage message ) {
            var headers = message.GetProperties ();
            string messageId = message.GetMessageId ();
            var contextBag = message.GetContextBag ();
            using var tokenSource = new CancellationTokenSource ();
            var messageContext = new MessageContext (
                messageId,
                headers,
                ( (IBytesMessage)message ).Content,
                transportTransaction,
                tokenSource,
                contextBag );

            await this.passMessageToNsb ( messageContext ).ConfigureAwait ( false );
        }
    }
}
