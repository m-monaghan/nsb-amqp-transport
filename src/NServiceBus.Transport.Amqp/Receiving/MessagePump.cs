/*
 * The message pump is responsible for reading messages from the underlying
 * transport and pushing them into the message handling pipeline.
 */
namespace NServiceBus.Transport.Amqp.Receiving {
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Amqp;
    using NServiceBus;
    using NServiceBus.Logging;
    using NServiceBus.Transport;

    sealed class MessagePump : IPushMessages, IDisposable {
        static readonly ILog logger = LogManager.GetLogger<MessagePump> ();
        static readonly TransportTransaction transportTransaction = new TransportTransaction ();

        private string messagePumpName;
        private Session session;
        private ReceiverLink receiver;
        private Func<MessageContext, Task> passMessageToNsbApplicationHook;
        private Func<ErrorContext, Task<ErrorHandleResult>> letNsbKnowAboutAnErrorHook;
        private CriticalError criticalError;
        private PushSettings nsbSettings;
        private SemaphoreSlim semaphore;
        private CancellationTokenSource cancellationTokenSource;
        private int maxConcurrency;

        public MessagePump ( Session session ) {
            this.session = session;
        }

        public Task Init (
            Func<MessageContext, Task> onMessage,
            Func<ErrorContext, Task<ErrorHandleResult>> onError,
            CriticalError criticalError,
            PushSettings settings ) {

            this.passMessageToNsbApplicationHook = onMessage;
            this.letNsbKnowAboutAnErrorHook = onError;
            this.criticalError = criticalError;
            this.nsbSettings = settings;
            this.messagePumpName = $"MessagePump-{this.nsbSettings.InputQueue}";

            return Task.CompletedTask;
        }

        public void Start ( PushRuntimeSettings limitations ) {
            logger.Info ( $"Starting MessagePump for [{this.messagePumpName}] Concurrency [{limitations.MaxConcurrency}]" );

            this.maxConcurrency = limitations.MaxConcurrency;
            this.semaphore = new SemaphoreSlim ( this.maxConcurrency, this.maxConcurrency );
            this.cancellationTokenSource = new CancellationTokenSource ();

            this.receiver = new ReceiverLink ( this.session,
                this.messagePumpName,
                this.nsbSettings.InputQueue );

            this.receiver.Start ( 10, async ( link, message ) => {
                var eventRaisingThreadId = Thread.CurrentThread.ManagedThreadId;

                try {
                    await semaphore.WaitAsync ( this.cancellationTokenSource.Token ).ConfigureAwait ( false );
                }
                catch (OperationCanceledException) {
                    return;
                }

                try {
                    if (Thread.CurrentThread.ManagedThreadId == eventRaisingThreadId) {
                        await Task.Yield ();
                    }

                    await ProcessMessageAsync ( link, message ).ConfigureAwait ( false );
                }
                catch (Exception exception) {
                    logger.Error ( $"Processing message failed", exception );
                    link.Reject ( message );

                    return;
                }
            } );

        }

        public async Task Stop () {
            logger.Info ( $"Stopping MessagePump for {this.messagePumpName}" );
            this.cancellationTokenSource.Cancel ();

            //while (this.semaphore.CurrentCount != 0) {
            //    logger.Info ( $"Stopping, waiting for threads, remaining [{this.semaphore.CurrentCount}]" );
            //    await Task.Delay ( 50 ).ConfigureAwait ( false );
            //}

            await this.receiver.CloseAsync ();
        }

        public void Dispose () {
            this.semaphore?.Dispose ();
            this.cancellationTokenSource?.Dispose ();
        }

        private async Task ProcessMessageAsync ( IReceiverLink receiverLink, Message amqpMessage ) {
            string messageId = amqpMessage.Properties.MessageId;
            if (string.IsNullOrWhiteSpace ( messageId )) {
                logger.Error ( "Missing Message ID" );
                await this.SendMessageToErrorQueue ( amqpMessage ).ConfigureAwait ( false );
                receiverLink.Accept ( amqpMessage );
                return;
            }

            var amqpPropertiesAsNsbHeaders = amqpMessage.GetProperties ();
            var errorHandled = false;
            var numberOfImmediateProcessingFailures = 0;

            using var tokenSource = new CancellationTokenSource ();

            while (!errorHandled) {
                try {
                    await this.LetNsbHandleMessage (
                        receiverLink,
                        amqpMessage,
                        messageId,
                        amqpPropertiesAsNsbHeaders,
                        tokenSource ).ConfigureAwait ( false );
                    return;
                }
                catch (Exception processingException) {
                    numberOfImmediateProcessingFailures++;
                    var contextBag = amqpMessage.GetNewContextBag ();

                    var errorContext = new ErrorContext (
                        processingException,
                        amqpPropertiesAsNsbHeaders,
                        messageId,
                        (byte[])amqpMessage.Body,
                        transportTransaction,
                        numberOfImmediateProcessingFailures,
                        contextBag );

                    try {
                        errorHandled = await this.letNsbKnowAboutAnErrorHook ( errorContext ).ConfigureAwait ( false ) == ErrorHandleResult.Handled;
                        if (!errorHandled) {
                            amqpPropertiesAsNsbHeaders = amqpMessage.GetProperties ();
                        }
                    }
                    catch (Exception errorException) {
                        criticalError.Raise ( $"Recoverability policy failed for message ID: [{messageId}]", errorException );
                        if (!receiverLink.IsClosed)
                            receiverLink.Reject ( amqpMessage );
                        return;
                    }
                }
            }
        }

        private async Task LetNsbHandleMessage ( IReceiverLink receiverLink, Message amqpMessage, string messageId, Dictionary<string, string> headers, CancellationTokenSource tokenSource ) {
            var contextBag = amqpMessage.GetNewContextBag ();

            var messageContext = new MessageContext (
                messageId,
                headers,
                (byte[])amqpMessage.Body,
                transportTransaction,
                tokenSource,
                contextBag );

            await this.passMessageToNsbApplicationHook ( messageContext ).ConfigureAwait ( false );

            if (tokenSource.IsCancellationRequested) {
                logger.Warn ( $"Cancellation requested whilst processing message ID: [{messageId}]" );
                receiverLink.Reject ( amqpMessage );
                return;
            }

            receiverLink.Accept ( amqpMessage );
            return;
        }

        private async Task SendMessageToErrorQueue ( Message message ) {
            var errorSenderLink = new SenderLink (
                this.session,
                $"nsb-error",
                this.nsbSettings.ErrorQueue );
            await errorSenderLink.SendAsync ( message ).ConfigureAwait ( false );
        }
    }
}
