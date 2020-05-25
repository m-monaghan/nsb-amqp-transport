docker run -it --rm \
  -e ARTEMIS_USERNAME=guest \
  -e ARTEMIS_PASSWORD=guest \
  -p 8161:8161 \
  -p 61616:61616 \
  -p 5672:5672 \
  vromero/activemq-artemis
