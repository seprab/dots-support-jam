using Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    public partial struct BulletSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            foreach ((
                         RefRW<LocalTransform> localTransform,
                         RefRW<Bullet> bullet,
                         Entity entity
                     )
                     in SystemAPI.Query<
                         RefRW<LocalTransform>,
                         RefRW<Bullet>
                     >().WithEntityAccess().WithAll<Simulate>())
            {
                float moveSpeed = 10;
                float3 moveVector = new float3(0, 0, 1);
                localTransform.ValueRW.Position += moveVector * moveSpeed * SystemAPI.Time.DeltaTime;

                if (state.World.IsServer())
                {
                    bullet.ValueRW.timer -= SystemAPI.Time.DeltaTime;
                    if (bullet.ValueRW.timer < 0)
                    {
                        ecb.DestroyEntity(entity);
                    }
                }
            }
            ecb.Playback(state.EntityManager);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state)
        {

        }
    }
}