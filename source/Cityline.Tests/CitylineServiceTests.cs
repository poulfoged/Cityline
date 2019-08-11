using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cityline.Tests
{
    [TestClass]
    public class CitylineServiceTests
    {
        [TestMethod]
        public async Task Can_call_provider()
        {
            ////Arrange
            var service = new CitylineService(new[] { new SampleProvider()});

            ////Act
            var result = await service.GetCarriage(new CitylineRequest(), null);

            ////Assert
            var carriage = result.Carriages.First();
            Assert.AreEqual("sample", carriage.Key);
        }
    }

    public class SampleProvider : ICitylineProvider
    {
        public string Name => "sample";

        public Task<object> GetCarriage(ITicketHolder ticket, IContext context, CancellationToken cancellationToken = default(CancellationToken))
        {
            var myState = ticket.GetTicket<MyState>();

            ticket.UpdateTicket(new { created = DateTime.UtcNow });

            return Task.FromResult((object)new { hello = "world"});
        }

        class MyState {}
    }
}
