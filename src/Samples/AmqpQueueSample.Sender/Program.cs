namespace AmqpQueueSample.Sender {
    using System;
    using Apache.NMS;
    using Apache.NMS.AMQP;

    class Program {
        static void Main ( string[] args ) {
            NmsConnectionFactory factory = new NmsConnectionFactory ( "amqp://localhost:5672" );
            var connection = factory.CreateConnection ( "guest", "guest" );
            var session = connection.CreateSession ( AcknowledgementMode.AutoAcknowledge );
            var queue = session.GetQueue ( "Sample.Queue" );
            var producer = session.CreateProducer ( queue );

            var message = producer.CreateTextMessage ( "Hello AMQP!" );
            producer.Send ( message );
            Console.WriteLine ( "Sent Hello AMQP!" );

            session.Close ();
            connection.Close ();
        }
    }
}
