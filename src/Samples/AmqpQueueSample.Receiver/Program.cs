namespace AmqpQueueSample.Receiver {
    using System;
    using Amqp;
  
    class Program {
        static void Main ( string[] args ) {
            Address address = new Address ( "amqp://guest:guest@localhost:5672" );
            Connection connection = new Connection ( address );
            Session session = new Session ( connection );
            ReceiverLink receiver = new ReceiverLink ( session, "receiver-link", "Sample.Queue" );

            Console.WriteLine ( "Receiver connected to broker." );
            Message message = receiver.Receive ();
            Console.WriteLine ( "Received " + message.Body );
            receiver.Accept ( message );

            receiver.Close ();
            session.Close ();
            connection.Close ();
        }
    }
}
