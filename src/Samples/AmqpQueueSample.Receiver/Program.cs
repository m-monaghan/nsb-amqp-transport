namespace AmqpQueueSample.Receiver {
    using System;
    using Apache.NMS;
    using Apache.NMS.AMQP;

    class Program {
        static void Main ( string[] args ) {
            NmsConnectionFactory factory = new NmsConnectionFactory ( "amqp://localhost:5672" );
            var connection = factory.CreateConnection ( "guest", "guest" );
            var session = connection.CreateSession ( AcknowledgementMode.AutoAcknowledge );
            var queue = session.GetQueue ( "Sample.Queue" );
            var consumer = session.CreateConsumer ( queue );

            connection.Start ();

            Console.WriteLine ( "Receiver connected to broker." );
            while(true) {
                var message = consumer.Receive ( TimeSpan.FromMilliseconds ( 5000 ) );
                if (message != null)
                    Console.WriteLine ( "Received " + ( (ITextMessage)message ).Text );
                else
                    break;
            }

            Console.WriteLine ( "Shutting doon!" );
            // consumer.Close ();
            session.Close ();
            connection.Close ();
        }
    }
}
