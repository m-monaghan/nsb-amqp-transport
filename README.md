# nsb-amqp-transport
NServiceBus AMQP Transport

The "NServiceBus AMQP Transport" name is self-explanatory, it’s a library that provides a transport option for NServiceBus to connect to ActiveMq/AMQ. 

It’s currently under development and isn’t suitable for production use.

The core library will be based on AmqpNetLite (https://github.com/Azure/amqpnetlite).

Testing again

## Roadmap 

- [ ] Ability to connect to an ActiveMq broker
- [ ] Ability to send a message to a queue
- [ ] Ability to send a message to a topic
- [ ] Ability to receive a message from a queue
- [ ] Ability to receive a message from a topic
- [ ] Ability to dynamically create queues
- [ ] Ability to control message transactions
- [ ] Route failed messages to a pre-defined queue
- [ ] Support/prove the mailbox pattern
- [ ] Ability to replay dead letter messages

## Samples

The samples have been created and tested against a local docker ActiveMq instance obtained from https://hub.docker.com/r/rmohr/activemq.

### Notes

To run the docker image:
docker pull rmohr/activemq
docker run -p 61616:61616 -p 8161:8161i -p 5672:5672 rmohr/activemq

## References

- NServiceBus: https://github.com/Particular/NServiceBus
- AmqpNetLite: https://github.com/Azure/amqpnetlite
- AMQP: http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-overview-v1.0-os.html
