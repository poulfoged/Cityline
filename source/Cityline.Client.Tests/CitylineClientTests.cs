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

namespace Cityline.Client.Tests
{
    [TestClass]
    public class CitylineClientTests
    {

        [TestMethod]
        public async Task Can_get_ping()
        {
            ////Arrange
            var pingFlag = false;
            var builder = new WebHostBuilder()
                    .ConfigureTestServices(x => x.AddSingleton<ICitylineProducer, PingProducer>())
                    .UseStartup<Startup>();

            using (CancellationTokenSource source = new CancellationTokenSource(2000)) // max run time
            using (var _server = new TestServer(builder)) 
            using (var client = new CitylineClient(new Uri("/cityline", UriKind.Relative), () => _server.CreateClient()))
            {
                client.Subscribe("ping", frame => { 
                    pingFlag = true; 
                    source.Cancel(); // we are done, cancel    
                });
                
                ////Act
                await client.StartListening(source.Token);

                ////Assert
                Assert.IsTrue(pingFlag);
            }
        }

        static void HandleUserAccount(Frame frame)
        {
            Debug.WriteLine("ping");
        }

        class UserAccount
        {
            public string Username { get; set; }
            public string Id { get; set; }
        }

        class PingProducer : ICitylineProducer
        {
            public string Name => "ping";

            public async Task<object> GetFrame(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<MyState>();

            if (myState != null)
                if (DateTime.UtcNow < myState.NextRefresh)
                    return null;

            ticket.UpdateTicket(new MyState { NextRefresh = DateTime.UtcNow.AddSeconds(3)});

            // simulate some work
            await Task.Delay(2);

            return new { NextResponseInSeconds = 3 };
        }

        class MyState 
        { 
            public DateTime NextRefresh { get; set; }
        }
        }
    }
}