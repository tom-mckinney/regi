using Grpc.Core;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Regi.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly IConsole _console;

        public GreeterService(IConsole console)
        {
            _console = console;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
