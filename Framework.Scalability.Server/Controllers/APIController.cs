using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

using Framework.Scalability.Core;
using Framework.Scalability.Server.Infrastructure;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Framework.Scalability.Server.Controllers
{
    [ApiController]
    [Route("api/[action]")]
    public class APIController : ControllerBase
    {
        private readonly List<Agent> _agents;
        private static readonly Random _random = new Random();

        private static readonly IList<int> _pendingCallbacks;
        private static readonly object _pendingCallbackLockObj;

        private static readonly Dictionary<int, object> _completedCallbacks;
        private static readonly object _completedCallbackLockObj;

        static APIController()
        {
            _pendingCallbacks = new List<int>();
            _completedCallbacks = new Dictionary<int, object>();
            _pendingCallbackLockObj = new object();
            _completedCallbackLockObj = new object();
        }
        public APIController(List<Agent> agents)
        {
            _agents = agents;
        }

        [HttpGet]
        public IActionResult Up() => Content("server up");

        [HttpPost]
        public async Task<IActionResult> NoInputNoOutputTest(string action)
        {
            var result = new List<Agent>();
            await Task.WhenAll(_agents.Where(x => x.Status == AgentStatus.Reachable).Select(x =>
                  Task.Run(async () =>
                  {
                      var httpHelper = new HttpHelper(x.Address);
                      await httpHelper.POSTSimpleAsync("/api/NINO?action=" + action);
                  })
            ));
            return Content(JsonConvert.SerializeObject(result, Formatting.Indented));
        }

        [HttpPost]
        public async Task<IActionResult> SomeInputNoOutputTest(string action, [FromBody] InstanceDescriptor[] parameters)
        {
            var result = new List<Agent>();
            await Task.WhenAll(_agents.Where(x => x.Status == AgentStatus.Reachable).Select(x =>
                  Task.Run(async () =>
                  {
                      var httpHelper = new HttpHelper(x.Address);
                      await httpHelper.POSTObjectAsync("/api/SINO?action=" + action, parameters);
                  })
            ));
            return StatusCode(201);
        }

        [HttpPost]
        public async Task<IActionResult> NoInputSomeOutputTest(string action)
        {
            var result = new List<Agent>();
            var ids = await Task.WhenAll(_agents.Where(x => x.Status == AgentStatus.Reachable).Select(x =>
                  Task.Run(async () =>
                  {
                      var httpHelper = new HttpHelper(x.Address);
                      return await httpHelper.POSTRequestAsync<int>("/api/NISO?action=" + action);
                  })
            ));
            foreach (var id in ids)
            {
                lock (_pendingCallbackLockObj)
                {
                    _pendingCallbacks.Add(id);
                }
            }
            return Created(Request.Path.ToUriComponent(), JsonConvert.SerializeObject(ids));
        }

        [HttpPost]
        public async Task<IActionResult> SomeInputSomeOutputTest(string action, [FromBody] InstanceDescriptor[] parameters)
        {
            var result = new List<Agent>();
            var ids = await Task.WhenAll(_agents.Where(x => x.Status == AgentStatus.Reachable).Select(x =>
                  Task.Run(async () =>
                  {
                      var httpHelper = new HttpHelper(x.Address);
                      return await httpHelper.POSTRequestAsync<int>("/api/SISO?action=" + action, parameters);
                  })
            ));
            foreach (var id in ids)
            {
                lock (_pendingCallbackLockObj)
                {
                    _pendingCallbacks.Add(id);
                }
            }
            return Created(Request.Path.ToUriComponent(), JsonConvert.SerializeObject(ids));
        }

        [HttpPost]
        public IActionResult InvokeCallback(int id, [FromBody] object result)
        {
            lock (_pendingCallbackLockObj)
            {
                _pendingCallbacks.Remove(id);
            }
            lock (_completedCallbackLockObj)
            {
                _completedCallbacks.Add(id, result);
            }
            return Ok();
        }

        [HttpGet]
        public IActionResult Callbacks() => Content(JsonConvert.SerializeObject(_completedCallbacks.Select(x => x.Key)));

        [HttpGet]
        public object Callback(int id)
        {
            if (!_completedCallbacks.ContainsKey(id))
            {
                return NotFound(id);
            }
            return _completedCallbacks[id];
        }

        [HttpGet]
        public IActionResult PendingCallbacks() => Content(JsonConvert.SerializeObject(_pendingCallbacks));

        [HttpPost]
        public IActionResult Register(int port)
        {
            var ip = HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
            if (ip == "0.0.0.1")
            {
                ip = "127.0.0.1";
            }
            var agent = new Agent()
            {
                Address = "http://" + ip + ":" + port,
                Environment = "ENV_" + _random.Next(2),
                Name = "AGENT_" + _random.Next(100000),
                LastUpdate = DateTime.Now,
                Status = AgentStatus.Reachable
            };
            _agents.Add(agent);
            Console.WriteLine($"Registered agent @{ip}:{port}");
            return Created(Request.Path.ToUriComponent(), agent as AgentInfo);
        }

        [HttpGet]
        public IActionResult Agents() => Content(JsonConvert.SerializeObject(_agents, Formatting.Indented));
    }
}
