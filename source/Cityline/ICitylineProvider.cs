using System;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;

namespace Cityline
{
    public interface ICitylineProducer
    {
        Task<object> GetFrame(ITicketHolder ticketHolder, IContext context, CancellationToken cancellationToken = default(CancellationToken)); 
    }
}