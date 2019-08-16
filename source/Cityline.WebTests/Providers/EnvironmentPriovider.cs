
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Cityline.WebTests.Providers
{
    public class EnvironmentProvider : ICitylineProvider
    {
        public string Name => "environment";
        private static DateTime started = DateTime.UtcNow;

        public async Task<object> GetCarriage(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<object>();

            if (myState != null)
                return null;

            ticket.UpdateTicket(new {});
            return new 
            { 
                Version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version,
                Started = started,
                ClientStarted = DateTime.UtcNow
            };
        }
    }
}