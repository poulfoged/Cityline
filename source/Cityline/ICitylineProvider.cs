using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Cityline
{
    public interface ICitylineProvider
    {
        string Name { get; }
        Task<object> GetCarriage(ITicketHolder ticketHolder, IContext context, CancellationToken cancellationToken = default(CancellationToken)); 
    }
}