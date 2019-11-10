using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;

namespace Regi.Services.Grpc
{
    public class HandshakeService : Handshake.HandshakeBase
    {
        private readonly IBroadcastService _broadcastService;

        public HandshakeService(IBroadcastService broadcastService)
        {
            _broadcastService = broadcastService;
        }

        public override Task<HandshakeReply> ShakeHands(HandshakeRequest request, ServerCallContext context)
        {
            _broadcastService.OpenStream();

            return Task.FromResult(new HandshakeReply
            {
                Pipename = "regi_Backend"
            });
        }
    }
}
