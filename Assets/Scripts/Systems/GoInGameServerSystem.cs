using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {   
        var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
        
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach ((
                     RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest, Entity rpcEntity)
                 in
                 SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>().WithEntityAccess())


        {

            Debug.Log("Client connected to server!! ");
            // entity on server who sent the rpc 
            var entityConnection = receiveRpcCommandRequest.ValueRO.SourceConnection;
            ecb.AddComponent<NetworkStreamInGame>(entityConnection);

            // Consume the rpc
            ecb.DestroyEntity(rpcEntity);

            // create the player prefab
            var playerEntity = ecb.Instantiate(entitiesReferences.playerPrefabEntity);
            // set position
            var pos = new float3(UnityEngine.Random.Range(-10, 10), 0, 0);
            ecb.SetComponent(playerEntity, LocalTransform.FromPosition(pos));
            // configure the owner
            var networkId = SystemAPI.GetComponent<NetworkId>(entityConnection);
            ecb.AddComponent(playerEntity, new GhostOwner
            {
                NetworkId = networkId.Value
            });
            // configure the LinkedEntity on the entityConnection to delete the visual player when the connection is deleted or disconnected
            ecb.AppendToBuffer(entityConnection, new LinkedEntityGroup{
                Value = playerEntity
            });
            
            
        }
        ecb.Playback(state.EntityManager);
    }
}
