using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Messaging;
using NServiceBus;

namespace Consumer1b
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new EndpointConfiguration("Consumer1b");
            config.EnableInstallers();
            config.UsePersistence<LearningPersistence>();
            config.LicensePath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nsb_license.xml"));
            var transport = config.UseTransport<RabbitMQTransport>()
                .ConnectionString("host=rabbitmq;username=user;password=bitnami");
            transport.UseConventionalRoutingTopology(); // fanout
            var bus = Endpoint.Start(config).Result;
            Task.Delay(Timeout.Infinite).GetAwaiter().GetResult();
        }
    }

    public class Handler : IHandleMessages<IncrementUserAgeEvent>, IHandleMessages<IncrementUserAgeCommand>, IDisposable
    {
        private readonly DataSource _db;

        public Handler()
        {
            _db = new DataSource();
        }

        public Task Handle(IncrementUserAgeEvent message, IMessageHandlerContext context)
        {
            Update();
            return Task.CompletedTask;
        }

        public Task Handle(IncrementUserAgeCommand message, IMessageHandlerContext context)
        {
            Update();
            return Task.CompletedTask;
        }

        private void Update()
        {
            var age = _db.GetUserAge();
            age++;
            _db.UpdateAge(age);
        }

        public void Dispose()
        {
            _db?.Dispose();
        }
    }
}
