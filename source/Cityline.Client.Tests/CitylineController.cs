using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Extensions;
using System.Threading;

namespace Cityline.WebTests.Controllers
{
    [Route("cityline")]
    [ApiController]
    public class CitylineController : ControllerBase
    {
        private CitylineService _citylineService;

        public CitylineController(IEnumerable<ICitylineProducer> providers) 
        {
            _citylineService = new CitylineService(providers);
        }

        [HttpPost]
        public async Task StartStream(CitylineRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
           var context = new CustomContext { RequestUrl = new Uri(Request.GetEncodedUrl()), User = User, SampleHeader = Request.Headers["sample"] };
            Response.Headers.Add("content-type", "text/event-stream");
            await _citylineService.WriteStream(Response.Body, request, context, cancellationToken);
        }

        [HttpPost("~/cityline-500")]
        public ActionResult Simulate_500(CitylineRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            return new StatusCodeResult(500);
        }
    }

    public class CustomContext : Context 
    {
        public string SampleHeader { get; set; }
    }
}
