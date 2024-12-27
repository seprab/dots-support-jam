using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Systems
{
    // This runs in the client and the server
    [CreateAfter(typeof(NetworkTimeSystem))]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct ShootSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
            state.RequireForUpdate<NetcodePlayerInput>();
            state.RequireForUpdate<EntitiesReferences>();
        }
        //[BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            NetworkTime networkTime = SystemAPI.GetSingleton<NetworkTime>();
            EntitiesReferences entitiesReferences = SystemAPI.GetSingleton<EntitiesReferences>();
            
            EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach ((RefRO<NetcodePlayerInput> playerInput,
                     RefRO<LocalTransform> localTransform,
                     RefRO<GhostOwner> ghostOwner
                     )in SystemAPI.Query<
                         RefRO<NetcodePlayerInput>,
                         RefRO<LocalTransform>,
                         RefRO<GhostOwner>
                     >().WithAll<Simulate>())
            {
                if (networkTime.IsFirstTimeFullyPredictingTick)
                {
                    if (playerInput.ValueRO.shoot.IsSet)
                    {
                        Debug.Log("shoot player" + state.World);
                        Entity bulletEntity = ecb.Instantiate(entitiesReferences.bulletPrefabEntity);
                        ecb.SetComponent(bulletEntity, LocalTransform.FromPosition(localTransform.ValueRO.Position));
                        ecb.SetComponent(bulletEntity, new GhostOwner{NetworkId = ghostOwner.ValueRO.NetworkId});
                    }
                }
            }
            ecb.Playback(state.EntityManager);
        }
    }
}