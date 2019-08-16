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
        private CitylineService _citylineService;

        public CitylineController(IEnumerable<ICitylineProvider> providers) 
        {
            _citylineService = new CitylineService(providers);
        }

        // GET api/values
        [HttpPost]
        public async Task<ActionResult<CitylineResponse>> StartAsync(Cityline.CitylineRequest request, CancellationToken cancellationToken = default(CancellationToken))
        {
            //var service = new CitylineService(new ICitylineProvider[] {new PingProvider(), new RandomProvider()});
            var context = new Context { RequestUrl = new Uri(Request.GetEncodedUrl()), User = User };
            return await _citylineService.GetCarriage(request, context, cancellationToken);
        }
    }
}
