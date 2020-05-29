# Apache NMS AMQP Transport for NServiceBus

The "NServiceBus AMQP Transport" name is self-explanatory, it’s a library that provides a transport option for NServiceBus to connect to ActiveMq (specifically Artemis). 

The transport utilises the Apache.NMS.AMQP library as the messaging API to a broker.

The following diagram depicts where this library sits within your application structure:

![Library Structure](https://github.com/m-monaghan/nsb-amqp-transport/blob/master/assets/WhatIsThis.png "What is this")

## Samples

The samples have been created and tested against a local docker ActiveMq Artemis instance obtained from https://hub.docker.com/r/vromero/activemq-artemis

### Notes

To run the docker image:
```
docker pull vromero/activemq-artemis
./scripts/run-docker-artemis.sh
```

## References

- NServiceBus: https://github.com/Particular/NServiceBus
- Apache.NMS.AMQP: https://activemq.apache.org/components/nms
- AmqpNetLite: https://github.com/Azure/amqpnetlite
- ActiveMq NMS AMQP:  https://github.com/apache/activemq-nms-amqp
- AMQP: http://docs.oasis-open.org/amqp/core/v1.0/os/amqp-core-overview-v1.0-os.html
