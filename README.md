#Burrows

Burrows is a .Net service bus based on the popular MassTransit service bus.

What is the difference?
Burrows is .Net 4.5 and up.  Why?  Because we wanted to make better use of the Async/Await and the related extensions.
Yes we understand that Async can now be added in .Net 4.0 via the Async Targetting Pack...but we're sticking with 4.5 for now.

Burrows is RabbitMQ only whereas MassTransit covers a variety of transports. This allows Burrows to simplify the codebase substantially 
in order to focus exclusively on Rabbit. As a result, Burrows provides a more full featured implementation of Rabbit including better 
support for publisher confirms while retaining the awesome polymorphic MassTransit routing setup.

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

####So publisher confirms are set up...how do I republish failed messages?
If the publisher is still running, it will check at intervals to determine if Rabbit has again become available for publication and will start publishing again immediately.  However, if the publisher also goes down, how to I republish stored messages at startup?  Just call RepublishStoredMessages() on the publisher after instantiation.

	var publisher = new Publisher(
		sbc => sbc.Configure(@"rabbitmq://localhost/PublishConsole"),
       	ps => ps.UsePublisherConfirms("PublishConsole").WithFileBackingStore("C:\MessageBackup")); 
	publisher.RepublishStoredMessages();

###OK, I published something...how do I Consume it
I'll save the details of how Burrows and RabbitMQ deliver messages to a longer blog post, but Burrows is based on the fantastic work that was put into MassTransit to 
allow polymorphic message consumption.  The first thing to do is set up a consumer service, typically using TopShelf to help.  Having done that, you will need to configure
the service bus on the consumer to receive messages.  Consumer setup is almost always going to be used in coordination with an IOC Container, and Burrows currently supports
Autofac.  The below approach can be modified, but this is demonstrated using an Autofac module to register the service bus.

	public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

			  //Register all of the consumers
            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                   .Where(t => t.Implements<IConsumer>())
                   .AsSelf();

			  //Register the service bus itself
            builder.Register(c => ServiceBusFactory.New(
				  sbc => sbc.Configure(@"rabbitmq://localhost/SubscribeConsole",
                subs => subs.LoadFrom(c.Resolve<ILifetimeScope>())))).SingleInstance();

            //This is the same as:
            //builder.Register(c => ServiceBusFactory.New(sbc =>
            //{
            //    sbc.ReceiveFrom(@"rabbitmq://localhost/SubscribeConsole");
            //    sbc.UseRabbitMq();
            //    sbc.UseControlBus();
            //    sbc.Subscribe(subs => subs.LoadFrom(c.Resolve<ILifetimeScope>()));
            //})).SingleInstance();
        }
    }

In the preceding code, the consumers are all registered with Autofac as classes that implement IConsumer.  The second section of code sets up the service bus.
Note that the service bus needs a queue to consume from, and in addition, it needs an action to give it a source of subscribers/consumers.  This source will
be used to provide instances of the consumers to the service bus when handling messages. The commented section of code shows all of the more detailed calls that are wrapped
by the Configure method.

The final step in consuming is to create a Consumer.  A consumer is a basic class that inherits from the Consumes class.  All messages that are satisfied by the 
"Consumes" type will be routed to this hander.  So in the case below, that would include all messages of the SimpleMessage type or all messages that SimpleMessage inherits 
from or impements (interfaces and classes will work).
 
	public class SubscribeConsoleConsumer : Consumes<SimpleMessage>.All
    {
        public void Consume(SimpleMessage message)
        {
            Console.WriteLine("Just got a message");
        }
    }

### Other Helpful Things
There are now Rabbit utility command classes available under Burrows.RabbitCommands.  These are:
Load, Move, Purge, and Save commands for operating directly on a Rabbit Queue.

##  Change Log
#### 0.3.1.0
* Removed async saving from publisher confirms because this sometimes caused thread locking from the publisher constructur and the method was always being waited anyway.  Planning on a better more full implementation of async from the publisher in the future.
* Removed RepublishStoredMessages() from the publisher constructure and added it to the IPublisher interface.  This must now be called (it is not automatically called in the constructor of the publisher).
* Added Rabbit command classes.

