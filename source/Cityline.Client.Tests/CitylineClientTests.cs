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
                client.Subscribe("ping", frame =>
                {
                    pingFlag = true;
                    source.Cancel(); // we are done, cancel    
                });

                ////Act
                await client.StartListening(source.Token);

                ////Assert
                Assert.IsTrue(pingFlag);
            }
        }

        [TestMethod]
        public async Task Can_pass_header_to_producer()
        {
            ////Arrange
            var sampleHeaderValue = Guid.NewGuid().ToString();
            string actualHeaderValue = null;
            var builder = new WebHostBuilder()
                    .ConfigureTestServices(x => x.AddSingleton<ICitylineProducer, PingProducer>())
                    .UseStartup<Startup>();

            using (CancellationTokenSource source = new CancellationTokenSource(2000)) // max run time
            using (var _server = new TestServer(builder))
            using (var client = new CitylineClient(new Uri("/cityline", UriKind.Relative), () => _server.CreateClient(), msg => msg.Headers.Add("sample", sampleHeaderValue)))
            {
                client.Subscribe("ping", frame =>
                {
                    actualHeaderValue = frame.As<PingResponse>().SampleHeader;
                    source.Cancel(); // we are done, cancel    
                });

                ////Act
                await client.StartListening(source.Token);

                ////Assert
                Assert.AreEqual(sampleHeaderValue, actualHeaderValue);
            }
        }

        /*
            Pingproducer counts up every x seconds
            If we disconnect (due to timeout) we expect the counting to continue due to us keeping state
        */
        [TestMethod]
        public async Task Can_resume_state() 
        {
            ////Arrange
            int counter = 0;
            var sampleHeaderValue = Guid.NewGuid().ToString();
            var builder = new WebHostBuilder()
                    .ConfigureTestServices(x => x.AddSingleton<ICitylineProducer, PingProducer>())
                    .UseStartup<Startup>();

            using (var _server = new TestServer(builder)) 
            {
                var httpClient = _server.CreateClient();
                using (var client = new CitylineClient(new Uri("/cityline", UriKind.Relative), () => httpClient, msg => msg.Headers.Add("sample", sampleHeaderValue)))
                {
                    client.Subscribe("ping", frame =>
                    {
                        counter = frame.As<PingResponse>().CallCount;  
                    });

                    ////Act
                    // we will force client to break connection but then resume where it left. 
                    // With no state whis will run forever
                    while (counter <= 8)
                        await client.StartListening(new CancellationTokenSource(200).Token);

                    ////Assert
                    Assert.IsTrue(counter >= 8); // we expect counter to reach at least 8 in 1 second
                }
            }
        }   
    }
}