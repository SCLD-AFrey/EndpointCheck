using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using EndpointSvc;
using Newtonsoft.Json;

namespace EndpointCheckSvc
{
    public class EndpointCheckService : EndpointChecker.EndpointCheckerBase
    {
        private readonly ILogger<EndpointCheckService> _logger;
        public EndpointCheckService(ILogger<EndpointCheckService> logger)
        {
            _logger = logger;
        }

        public override Task<EndpointCheckReply> CheckEndpoint(EndpointCheckRequest request, ServerCallContext context)
        {
            var item = JsonConvert.DeserializeObject<EndpointCheck>(request.Json);
            item.StartTime = DateTime.Now;
            item.Success = true;
            item.Message = "";

            //Start Checks

            if (item.Platform.ToLower() != "windows")
            {
                item.Success = false;
                item.Message = "Not Windows";
            }

            //End Checks

            item.EndTime = DateTime.Now;

            var reply = new EndpointCheckReply
            {
                Json = JsonConvert.SerializeObject(item)
            };
            return Task.FromResult(reply);
        }
    }
}