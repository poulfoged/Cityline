
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cityline.WebTests.Providers
{
    public class RandomProvider : ICitylineProvider
    {
        public string Name => "random";
        private static Random random = new Random();

        public async Task<object> GetCarriage(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<MyState>();

            if (myState != null)
                if (DateTime.UtcNow < myState.NextRefresh)
                    return null;

            var seconds = random.Next(0, 30);
            ticket.UpdateTicket(new MyState { NextRefresh = DateTime.UtcNow.AddSeconds(seconds)});

            // simulate some work
            await Task.Delay(2);

            return new { NextResponseInSeconds = seconds };
        }

        class MyState 
        { 
            public DateTime NextRefresh { get; set; }
        }
    }
}