using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Regi.Services.Grpc
{
    public class HandshakeService : Handshake.HandshakeBase
    {
        public override Task<HandshakeReply> ShakeHands(HandshakeRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HandshakeReply
            {
                Pipename = "it worked!"
            });
        }
    }
}
