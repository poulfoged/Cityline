using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Cityline;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cityline
{
    public class CitylineService
    {
        private static readonly object padLock = new object(); 
        private IEnumerable<ICitylineProducer> _providers;
        private static JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), Formatting = Formatting.None };
    
        public CitylineService(IEnumerable<ICitylineProducer> providers) 
        {
            _providers = providers;
        }

        public async Task WriteStream(Stream stream, CitylineRequest request, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var queue = new Queue<ICitylineProducer>(_providers);
            while (!cancellationToken.IsCancellationRequested)
            {
                if (queue.Count > 0) {
                    var provider = queue.Dequeue();
                    var name = provider.Name();
                    TicketHolder ticket = null;

                    if (request.Tickets == null)
                        request.Tickets = new Dictionary<string, string>();

                    if (request.Tickets.ContainsKey(name))
                        ticket = new TicketHolder(request.Tickets[name]);
                
                    ticket = ticket ?? new TicketHolder();

                    #pragma warning disable 4014
                    Task.Run(async () => 
                    {
                        await RunProducer(provider, stream, ticket, context, cancellationToken);

                        if (request.Tickets.ContainsKey(name))
                            request.Tickets[name] = ticket.AsString();
                        else
                            request.Tickets.Add(name, ticket.AsString());

                        queue.Enqueue(provider);
                    }).ConfigureAwait(false);
                    #pragma warning restore 4014

                    await Task.Delay(200);
                }

                await Task.Delay(200);
            }
        }

        private async Task RunProducer(ICitylineProducer provider, Stream stream, TicketHolder ticket, IContext context, CancellationToken cancellationToken)
        {
            var response = await provider.GetFrame(ticket, context, cancellationToken);

            if (response == null)
                return;

            lock(padLock) 
            {
                using (var writer = new StreamWriter(stream, Encoding.UTF8, 1024, true)) {
                    writer.WriteLine($"id: {ticket.AsString()}");
                    writer.WriteLine($"event: {provider.Name()}");
                    writer.WriteLine($"data: {JsonConvert.SerializeObject(response, settings)}");
                    writer.WriteLine();
                }
            }
        }
    }
}
