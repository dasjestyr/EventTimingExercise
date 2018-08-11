using System;
using System.IO;
using System.Threading.Tasks;
using Messaging;
using NServiceBus;

namespace Producer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            /* ***** 
             * This test demonstrates the effect of messaging with multiple consumers.
             * Case 1: A command is sent to a specific consumer so it is only processed once
             * Case 2: A command is sent as an event so it is processed by both instances of the same consumer (demonstrating when competing consumers isn't supported -- e.g. Topic)
             * Case 3: Same as case 2 but exaggerated with more iterations, demonstrating how it can get out of control under load.
             */

            var config = new EndpointConfiguration("Producer");
            config.EnableInstallers();
            config.UsePersistence<LearningPersistence>();
            config.LicensePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nsb_license.xml"));
            var transport = config.UseTransport<RabbitMQTransport>()
                .ConnectionString("host=rabbitmq;username=user;password=bitnami");
            transport.UseConventionalRoutingTopology(); // fanout

            var bus = Endpoint.Start(config).Result;
            
            using (var source = new DataSource())
            {
                source.UpdateAge(21);
                var previousAge = source.GetUserAge();
                
                while (true)
                {
                    Console.Clear();
                    var currentAge = source.GetUserAge();
                    Console.WriteLine();
                    Console.WriteLine($"User age was {previousAge} but is now {currentAge}.");
                    Console.WriteLine("1) Increment age by 1 as COMMAND"); // a queue behavior
                    Console.WriteLine("2) Increment age by 1 as EVENT"); // a topic behavior without "at most once delivery" guarantees
                    Console.WriteLine("3) Increment age by 10 as EVENT"); // topic behavior under load
                    Console.WriteLine("4) Reset age to 21");

                    bool valid;
                    void Toggle(int i)
                    {
                        previousAge = currentAge;
                        valid = true;
                        var wait = i;
                        Console.Write($"Allowing enough time for simulated delays ({ wait / 1000.00}s)...");
                        Task.Delay(wait).Wait(); // let it actual process first
                    }

                    do
                    {
                        Console.Write("Selection: ");
                        var selection = Console.ReadLine();
                        switch (selection)
                        {
                            case "1": // will increment by 1
                                bus.Send<IncrementUserAgeCommand>("Consumer1a", e => { e.Id = Guid.NewGuid(); }).GetAwaiter().GetResult();
                                Toggle(1200);
                                break;
                            case "2": // will increment by 2 because of double processing with a delay
                                bus.Publish<IncrementUserAgeEvent>(e => { e.Id = Guid.NewGuid(); }).GetAwaiter().GetResult();
                                Toggle(1200);
                                break;
                            case "3": // exaggerated view of case 2, data getting really corrupted
                                for (var i = 0; i < 10; i++)
                                    bus.Publish<IncrementUserAgeEvent>(e => { e.Id = Guid.NewGuid(); }).GetAwaiter().GetResult();
                                Toggle(1200);
                                break;
                            case "4":
                                source.UpdateAge(21);
                                Toggle(0);
                                break;
                            default:
                                Console.WriteLine("Invalid selection");
                                valid = false;
                                break;
                        }
                        
                    } while (!valid);
                }
            }
        }
    }
}
