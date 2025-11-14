FROM rabbitmq:4.2.0-management

RUN apt-get update && \
apt-get install -y wget

RUN wget -O /opt/rabbitmq/plugins/rabbitmq_delayed_message_exchange.ez https://github.com/rabbitmq/rabbitmq-delayed-message-exchange/releases/download/v4.2.0/rabbitmq_delayed_message_exchange-4.2.0.ez

RUN rabbitmq-plugins enable rabbitmq_delayed_message_exchange