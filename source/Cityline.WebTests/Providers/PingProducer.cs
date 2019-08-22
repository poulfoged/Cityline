
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cityline.WebTests.Providers
{
    public class PingProducer : ICitylineProducer
    {
        public string Name => "ping";

        public async Task<object> GetFrame(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<MyState>();

            if (myState != null)
                if ((DateTime.UtcNow - myState.Created).TotalSeconds < 5)
                    return null;

            ticket.UpdateTicket(new MyState());

            // simulate some work
            await Task.Delay(2);

            return new { Ping = DateTime.UtcNow };
        }

        class MyState 
        { 
            public DateTime Created { get; set; } = DateTime.UtcNow;
        }
    }
}