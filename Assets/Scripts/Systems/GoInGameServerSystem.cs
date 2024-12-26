using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {   
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((
                     RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest, Entity rpcEntity)
            in
            SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())
        {
            
            Debug.Log("Client connected to server!! ");
            // entity on server who sent the rpc 
            var entity = receiveRpcCommandRequest.ValueRO.SourceConnection;
            ecb.AddComponent<NetworkStreamInGame>(entity);
            
            // Consume the rpc
            ecb.DestroyEntity(rpcEntity);
        }
        ecb.Playback(state.EntityManager);
    }
}
