using System.Collections.Generic;
using System.Linq;
using CashSchedulerWebServer.Events.Contracts;

namespace CashSchedulerWebServer.Events
{
    public class EventManager : IEventManager
    {
        private IEnumerable<IEventListener> Listeners { get; }
        
        public EventManager(IEnumerable<IEventListener> listeners)
        {
            Listeners = listeners;
        }

        public async void FireEvent(EventAction action, object entity)
        {
            foreach (var listener in Listeners.Where(l => l.Action == action))
            {
                await listener.Handle(entity);
            }
        }
    }
}
