using UnityEditor.VersionControl;
using UnityEngine;

namespace Unibas.DBIS.VREP
{
    public class MultiUserSyncImpl : multiUserSync.multiUserSyncBase
    {
        public override Task getPlayer(IAsyncStreamReader<RequestPlayer> requestStream, IServerStreamWriter<Player> responseStream,
            ServerCallContext context)
        {
            return base.getPlayer(requestStream, responseStream, context);
        }

        public override Task setPlayer(IAsyncStreamReader<Player> requestStream, IServerStreamWriter<Response> responseStream,
            ServerCallContext context)
        {
            return base.setPlayer(requestStream, responseStream, context);
        }

        GameObject player = GameObject.Find("Player");
        
        
    }
}