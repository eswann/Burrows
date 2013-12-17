Burrows
=======

Burrows is a .Net service bus based on the popular MassTransit service bus.

What is the difference?

Burrows is RabbitMQ only whereas MassTransit covers a variety of transports.  This allows Burrows to simplify the codebase substantially in order to focus exclusively on Rabbit.  This also allows Burrows to provide a more full featured implementation of Rabbit including better support for publisher confirms while retaining the awesome polymorphic MassTransit routing setup.
