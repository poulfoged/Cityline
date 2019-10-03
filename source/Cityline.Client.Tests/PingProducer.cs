using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Cityline.WebTests.Controllers;

namespace Cityline.Client.Tests
{
    internal class PingProducer : ICitylineProducer
    {
        public string Name => "ping";

        public async Task<object> GetFrame(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<MyState>();

            if (myState != null)
                if (DateTime.UtcNow < myState.NextRefresh)
                    return null;
                
            int counter = 0;
            if (myState != null)
                counter = myState.CallCount+1;

            ticket.UpdateTicket(new MyState { NextRefresh = DateTime.UtcNow.AddMilliseconds(100), CallCount = counter });

            // simulate some work
            await Task.Delay(2);

            return new PingResponse { SampleHeader = (context as CustomContext)?.SampleHeader, CallCount = counter };
        }

        class MyState
        {
            public DateTime NextRefresh { get; set; }

            public int CallCount { get; set; }
        }
    }

    public class PingResponse
    {
        public int CallCount { get; set; }
        public string SampleHeader { get; set; }
    }

}