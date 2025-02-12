using Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

[RequireMatchingQueriesForUpdate]
//[UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(EndFixedStepSimulationEntityCommandBufferSystem))]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(PhysicsSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct BulletCollisionSystem : ISystem
{
    
    ComponentLookup<Bullet> _bulletLookup;
    ComponentLookup<Zombie> _zombieLookup;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<SimulationSingleton>();
        state.RequireForUpdate<Bullet>();
        
        _bulletLookup = state.GetComponentLookup<Bullet>(true);
        _zombieLookup = state.GetComponentLookup<Zombie>(true);
    }
    

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        _bulletLookup.Update(ref state);
        _zombieLookup.Update(ref state);
        //CollisionEvents events = SystemAPI.GetSingleton<SimulationSingleton>().AsSimulation().CollisionEvents;
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var collisionJob = new BulletCollisionJob
        {
            BulletLookup = _bulletLookup,
            ZombieLookup = _zombieLookup,
            Ecb = ecb.AsParallelWriter()
        };
        
        state.Dependency = collisionJob.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        // TODO  CHECK if we need to call this 
        state.Dependency.Complete();
        
        ecb.Playback(state.EntityManager);
        ecb.Dispose();
        
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

[BurstCompile]
public struct BulletCollisionJob : ICollisionEventsJob
{
   [ReadOnly] public ComponentLookup<Bullet> BulletLookup;
   [ReadOnly] public ComponentLookup<Zombie> ZombieLookup;
    public EntityCommandBuffer.ParallelWriter Ecb;
    
    public void Execute(CollisionEvent collisionEvent)
    {
        Entity entityA = collisionEvent.EntityA;
        Entity entityB = collisionEvent.EntityB;

        bool isBulletA = BulletLookup.HasComponent(entityA);
        bool isBulletB = BulletLookup.HasComponent(entityB);
        bool isZombieA = ZombieLookup.HasComponent(entityA);
        bool isZombieB = ZombieLookup.HasComponent(entityB);

        // Check if Bullet hit Zombie
        if ((isBulletA && isZombieB) || (isBulletB && isZombieA))
        {
            Entity bullet = isBulletA ? entityA : entityB;
            Entity zombie = isBulletA ? entityB : entityA;

            // Destroy bullet
            Ecb.DestroyEntity(0, bullet);
            Ecb.DestroyEntity(0, zombie);
            Debug.Log("hit zombie ");

            // Apply damage or other effects to zombie
        }
    }
}
