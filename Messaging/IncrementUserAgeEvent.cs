using System;
using NServiceBus;

namespace Messaging
{
    public class IncrementUserAgeEvent : IEvent
    {
        public Guid Id { get; set; }
    }

    public class IncrementUserAgeCommand : ICommand
    {
        public Guid Id { get; set; }
    }
}
