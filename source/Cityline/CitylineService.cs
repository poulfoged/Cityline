using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Cityline;

namespace Cityline
{
    public class CitylineService
    {
        IEnumerable<ICitylineProvider> _providers;
        public CitylineService(IEnumerable<ICitylineProvider> providers) 
        {
            _providers = providers;
        }

        public async Task<CitylineResponse> GetCarriage(CitylineRequest request, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var linked = CancellationTokenSource.CreateLinkedTokenSource(timeoutSource.Token, cancellationToken);
            var token = linked.Token;

            while (!token.IsCancellationRequested)
            {
                var result = await DoGetCarriage(request, context);

                if (result.Carriages.Count > 0)
                    return result;

                await Task.Delay(250);
            }

            return null;
        }

        private async Task<CitylineResponse> DoGetCarriage(CitylineRequest request, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var train = new Dictionary<string, Carriage>();

            do 
            {
                List<Task> tasks = new List<Task>();
                foreach (var provider in _providers)
                {
                    TicketHolder ticket = null;

                    if (request?.Tickets != null)
                    {
                        if (request.Tickets.ContainsKey(provider.Name))
                            ticket = new TicketHolder(request.Tickets[provider.Name]);
                    }

                    ticket = ticket ?? new TicketHolder();

                    tasks.Add(Task.Run(async () =>
                    {
                        await RequestCarriage(provider, ticket, context, train, cancellationToken);
                    }, cancellationToken));

                }

                await Task.WhenAll(tasks.ToArray());

                if (train.Count == 0)
                    await Task.Delay(1000);

            } while (train.Count == 0);


            return new CitylineResponse
            {
                Carriages = train
            };
        }

        private async Task RequestCarriage(ICitylineProvider provider, TicketHolder ticketHolder, IContext context, IDictionary<string, Carriage> train, CancellationToken cancellationToken = default(CancellationToken)) 
        {
            var response = await provider.GetCarriage(ticketHolder, context);
            
            if (response == null)
                return;

            train.Add(provider.Name, new Carriage {
                Ticket = ticketHolder.AsString(),
                Cargo = response
            });
        }
    }
}
