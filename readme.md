To run, ensure that rabbitmq is runnin on localhost on its default port (either locally or via docker) and that you have a DNS entry in your hostfile that points the hostname 'rabbitmq' to 127.0.0.1. Configure Visual Studio to run Producer, Consumer1a, and Consumer1b at the same time.

## Test Parameters
* A central sqlite database is shared between the 3 services (producer and 2 consumers) to help simulate a shared resource

## Cases
1. Commands are used to simulate a P2P message.
2. Events are used to simulate messages sent to all Topic subscribers.
    * Each consumer has a different name (Consumer1a and Consumer1b) to ensure they are not receiving from the same queue. This simulates a scenario where there are multiple instances of the same service in an implementation where an "at most once" delivery guarantee cannot be made.
    * The update database command is idempotent (full replace) to demonstrate that this isn't enough to guarantee data consistency without the appropriate delivery guarantees.
3.  Same as option 2, but shows how unpredictable the data can become under load when messages are allowed to double process.
