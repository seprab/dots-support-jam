using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;



[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    private Unity.Mathematics.Random _random;
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        state.RequireForUpdate<NetworkId>();
        _random = Random.CreateFromIndex(0);

        state.Enabled = false;
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (SystemAPI.HasSingleton<GameSetup>())
        {
            var entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            // new for Character Controller 
            GameSetup gameSetup = SystemAPI.GetSingleton<GameSetup>();
            
            foreach ((RefRO<ReceiveRpcCommandRequest> receiveRpcCommandRequest, Entity rpcEntity)
                     in
                     SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<GoInGameRequestRpc>()
                         .WithEntityAccess()){

                // entity on server who sent the rpc 
                var entityConnection = receiveRpcCommandRequest.ValueRO.SourceConnection;
                
                // Spawn character, player, and camera ghost prefabs
                Entity characterEntity = ecb.Instantiate(gameSetup.CharacterPrefab);
                Entity playerEntity = ecb.Instantiate(gameSetup.PlayerPrefab);
                Entity cameraEntity = ecb.Instantiate(gameSetup.CameraPrefab);
                    
                // Add spawned prefabs to the connection entity's linked entities, so they get destroyed along with it
                ecb.AppendToBuffer(entityConnection, new LinkedEntityGroup { Value = characterEntity });
                ecb.AppendToBuffer(entityConnection, new LinkedEntityGroup { Value = playerEntity });
                ecb.AppendToBuffer(entityConnection, new LinkedEntityGroup { Value = cameraEntity });
                
                // Setup the owners of the ghost prefabs (which are all owner-predicted) 
                // The owner is the client connection that sent the join request
                int clientConnectionId = SystemAPI.GetComponent<NetworkId>(entityConnection).Value;
                ecb.SetComponent(characterEntity, new GhostOwner { NetworkId = clientConnectionId });
                ecb.SetComponent(playerEntity, new GhostOwner { NetworkId = clientConnectionId });
                ecb.SetComponent(cameraEntity, new GhostOwner { NetworkId = clientConnectionId });

                // Setup links between the prefabs
                ThirdPersonPlayer player = SystemAPI.GetComponent<ThirdPersonPlayer>(gameSetup.PlayerPrefab);
                //player.ControlledCharacter = characterEntity;
                //player.ControlledCamera = cameraEntity;
                //ecb.SetComponent(playerEntity, player);
                
                // Place character at a random point around world origin
                ecb.SetComponent(characterEntity, LocalTransform.FromPosition(_random.NextFloat3(new float3(-5f,0f,-5f), new float3(5f,0f,5f))));

                
                // Allow this client to stream in game
                ecb.AddComponent<NetworkStreamInGame>(entityConnection);
                // Consume the rpc
                ecb.DestroyEntity(rpcEntity);
            }

            ecb.Playback(state.EntityManager);
        }
    }
}
