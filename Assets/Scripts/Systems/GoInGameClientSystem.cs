using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
partial struct GoInGameClientSystem : ISystem
{
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRO<NetworkId> networkId, Entity entity) in
                 SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess()
                )
        {
            // adds automatically every networkId to ingame 
            ecb.AddComponent<NetworkStreamInGame>(entity);
            Debug.Log("setting client in game");
            
            //RPCs 
            Entity rpcEntity = ecb.CreateEntity();
            ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());
            ecb.AddComponent(rpcEntity, new GoInGameRequestRpc());
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

public struct GoInGameRequestRpc : IRpcCommand {
}
