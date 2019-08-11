using System;
using System.Collections.Generic;
using System.Security.Principal;

namespace Cityline
{
    public interface ITicketHolder
    {
        TTicket GetTicket<TTicket>() where TTicket : class;

        void UpdateTicket<TTicket>(TTicket ticket) where TTicket : class;
    }
}