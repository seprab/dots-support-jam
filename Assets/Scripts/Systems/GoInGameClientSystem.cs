using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GoInGameClientSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();

        state.Enabled = false;
        Debug.Log("Disable GoInGameClientSystem, replace with GameSetupSystem ");
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((RefRO<NetworkId> networkId, Entity entity) in
                 SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess()
                )
        {
            // adds automatically every networkId to ingame 
            // Mark our connection as ready to go in game
            ecb.AddComponent<NetworkStreamInGame>(entity);
            Debug.Log("setting client in game");
            
            // Send an RPC that asks the server if we can join
            Entity rpcEntity = ecb.CreateEntity();
            ecb.AddComponent(rpcEntity, new SendRpcCommandRequest());
            ecb.AddComponent(rpcEntity, new GoInGameRequestRpc());
            
            // Handle initialization for our local character camera (mark main camera entity)
            foreach (var (camera, camEntity) in SystemAPI.Query<OrbitCamera>().WithAll<GhostOwnerIsLocal>().WithNone<LocalInitialized>().WithEntityAccess())
            {
                ecb.AddComponent(camEntity, new MainEntityCamera());
                ecb.AddComponent(camEntity, new LocalInitialized());
            }
            
            
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
