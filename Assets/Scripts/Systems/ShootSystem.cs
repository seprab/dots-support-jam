using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Physics;
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
                        var transform = LocalTransform.FromPosition(localTransform.ValueRO.Position );
                        var forward = localTransform.ValueRO.TransformDirection(new float3(0, 0, 1));
                        transform.Position = transform.Position + forward * 2f + new float3(0, 0.8f, 0);;
                        
                        ecb.SetComponent(bulletEntity, transform);
                        ecb.SetComponent(bulletEntity, new GhostOwner{NetworkId = ghostOwner.ValueRO.NetworkId});
                        var speed = 40;
                        var direction = forward;
                        ecb.SetComponent(bulletEntity ,  new PhysicsVelocity{ Linear   = direction * speed  });
                    }
                }
            }
            ecb.Playback(state.EntityManager);
        }
    }
}