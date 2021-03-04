using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Framework.Scalability.Agent.Infrastructure;
using Framework.Scalability.Core;
using Framework.Scalability.Core.Actions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Framework.Scalability.Agent.Controllers
{
    [ApiController]
    [Route("api/[action]")]
    public class APIController : ControllerBase
    {
        private readonly AgentInfo _identity;

        public APIController(AgentInfo identity)
        {
            _identity = identity;
        }

        [HttpPost]
        public IActionResult NINO(string action)
        {
            APIControllerIOC.OnNINOInvoked(this, new RemoteActionInvocationArgs(action));
            return Content("ack");
        }

        [HttpPost]
        public IActionResult SINO(string action, [FromBody] InstanceDescriptor[] parameters)
        {
            APIControllerIOC.OnSINOInvoked(this, new RemoteActionInvocationArgs(action, parameters));
            return Content("ack");
        }

        [HttpPost]
        public IActionResult NISO(string action)
        {
            var args = new RemoteActionInvocationArgs(action);
            var id = ActionCallbackHandler.Register(args);
            APIControllerIOC.OnNISOInvoked(this, args);
            return Created("NISO", id.ToString());
        }

        [HttpPost]
        public IActionResult SISO(string action, [FromBody] InstanceDescriptor[] parameters)
        {
            var args = new RemoteActionInvocationArgs(action, parameters);
            var id = ActionCallbackHandler.Register(args);
            APIControllerIOC.OnSISOInvoked(this, args);
            return Created("NISO", id.ToString());
        }

        [HttpGet]
        public IActionResult Identity() => Content(JsonConvert.SerializeObject(_identity));

        [HttpGet]
        public IActionResult Up() => Content("client up");
    }
}
