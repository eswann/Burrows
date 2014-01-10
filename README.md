#Burrows

Burrows is a .Net service bus based on the popular MassTransit service bus.

What is the difference?

Burrows is .Net 4.5 and up.  Why?  Because we wanted to make better use of the Async/Await and the related extensions.  
Yes we understand that Async can now be added in .Net 4.0 via the Async Targetting Pack...but we're sticking with 4.5 for now.

Burrows is RabbitMQ only whereas MassTransit covers a variety of transports.  
This allows Burrows to simplify the codebase substantially in order to focus exclusively on Rabbit.  
As a result, Burrows provides a more full featured implementation of Rabbit including better support for publisher confirms while retaining the awesome polymorphic MassTransit routing setup.

##How Do I Get Started?

The first thing to know is how to set up a publisher and publish a message.

###Set up the publisher:
The easiest way to set up a publisher is with the following command:

	var publisher = new Publisher(sbc => sbc.Configure(@"rabbitmq://localhost/PublishConsole"));

This command will set up the publisher to listen at the specified endpoint, but will also instruct the publisher to use a control bus.  
It is synonymous with this command:

	var publisher = new Publisher(sbc => sbc.Configure(@"rabbitmq://localhost/PublishConsole")
												  .UseRabbitMq().UseLog4Net());

There are many extensions available to customize bus behavior and they are all chainable.

####What if I want to use Publisher Confirms?
Publisher confirms allow RabbitMQ publishers to ensure that messages got to the Rabbit broker.  Although it's an edge case, it is always possible that RabbitMQ may begin 
nacking messages if the broker begins experiencing issues.  If a Nack is encountered, it's likely that some manual intervention will be required on the broker, making it
important to handle this situation and prevent message loss.  In addition, there are situations such as simple network outages that may cause the publisher to lose 
connectivity to the Rabbit broker.  Because Rabbit is a broker architecture, it's up to you to figure out what to do with the these messages.  The publisher can keep 
them in memory, but this could soon become overwhelming.  

Burrows includes some automatic mechanisms for offloading messages and then retrying them in the event of a broker Nack or network outage.  The publisher will store 
the messages on the file system and then attempt to retry them in roughly the same order they came in.  It is possible to implement other storage mechanisms as well,
but the file system is the most basic and available should a network outage occur. 

####Set up publisher Confirms:

	var publisher = new Publisher(
				sbc => sbc.Configure(@"rabbitmq://localhost/PublishConsole"),
              ps => ps.UsePublisherConfirms("PublishConsole").WithFileBackingStore("C:\MessageBackup"));     

The second constructor parameter to the publisher enables publisher confirms to a file backing store: 

	ps.UsePublisherConfirms("PublishConsole").WithFileBackingStore("C:\MessageBackup")    

The call to UsePublisherConfirms accepts an argument which tells this publisher what to call the storage area where these messages will be stored and retrieved.
Typically, this would be based on the name of the publisher.  The extension WithFileBackingStore tells the publisher to store messages on the file system at the root
location specified.  The account the publisher runs under must have persmissions to write and create directories under this root location to organize messages.