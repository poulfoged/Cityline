
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cityline.WebTests.Providers
{
    public class EnvironmentProducer : ICitylineProducer
    {
        public string Name => "environment";
        private static DateTime started = DateTime.UtcNow;

        public Task<object> GetFrame(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<object>();

            if (myState != null)
                return Task.FromResult((object)null);

            ticket.UpdateTicket(new {});
            return Task.FromResult((object)new 
            { 
                Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version,
                Started = started,
                ClientStarted = DateTime.UtcNow
            });
        }
    }
}