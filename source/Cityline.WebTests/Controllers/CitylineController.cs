using System;
using System.Collections.Generic;
using System.Linq;
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
        // GET api/values
        [HttpPost]
        public async Task<ActionResult<CitylineResponse>> StartAsync(Cityline.CitylineRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            var service = new CitylineService(new [] {new SampleProvider()});
            var context = new Context { RequestUrl = new Uri(Request.GetEncodedUrl()), User = User };
            return await service.GetCarriage(request, context, cancellationToken);
        }
    }
}
