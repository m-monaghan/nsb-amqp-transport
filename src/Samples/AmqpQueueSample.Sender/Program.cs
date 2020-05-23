namespace AmqpQueueSample.Sender {
    using System;
    using Amqp;

    class Program {
        static void Main ( string[] args ) {
            Address address = new Address ( "amqp://guest:guest@localhost:5672" );
            Connection connection = new Connection ( address );
            Session session = new Session ( connection );

            Message message = new Message ( "Hello AMQP!" );
            SenderLink sender = new SenderLink ( session, "sender-link", "Sample.Queue" );
            sender.Send ( message );
            Console.WriteLine ( "Sent Hello AMQP!" );

            sender.Close ();
            session.Close ();
            connection.Close ();
        }
    }
}
