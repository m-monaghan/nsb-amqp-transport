namespace AmqpQueueSample.Receiver {
    using System;
    using Apache.NMS;
    using Apache.NMS.AMQP;
    using Apache.NMS.AMQP.Message;

    class Program {
        static void Main ( string[] args ) {
            var factory = new NmsConnectionFactory ( "failover:(amqp://localhost:5672,amqp://localhost:5672)?nms.clientId=AmqpQueueSample.Server.&failover.maxReconnectAttempts=20" );
            var connection = factory.CreateConnection ( "guest", "guest" );
            var session = connection.CreateSession ( AcknowledgementMode.AutoAcknowledge );
            var queue = session.GetQueue ( "Sample.Queue" );
            var consumer = session.CreateConsumer ( queue );
            consumer.Listener += HandleConsumerMessage;

            connection.Start ();

            Console.WriteLine ( "Receiver connected to broker." );
            Console.ReadLine ();
            Console.WriteLine ( "Shutting doon!" );
            session.Close ();
            connection.Close ();
        }

        private static void HandleConsumerMessage ( IMessage message ) {
            Console.WriteLine ( "Received " + ( (ITextMessage)message ).Text );
        }
    }
}
