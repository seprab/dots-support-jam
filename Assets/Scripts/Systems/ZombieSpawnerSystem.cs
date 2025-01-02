using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[RequireMatchingQueriesForUpdate]
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct ZombieSpawnerSystem : ISystem
{   
    
    //TODO : is predicted ? ZombieSpawnerSystem 
    // what happens if a player is connected later? 
    
    public Random Random;
    public float waveCreationTime;
    
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EntitiesReferences>();
        Random = new Random((uint)state.WorldUnmanaged.Time.ElapsedTime + 1234);
        //TODO : move this value to a config entity
        waveCreationTime = 0;

    }
    public void OnDestroy(ref SystemState state) { }

    [BurstCompile (Debug = true )]
    public void OnUpdate(ref SystemState state)
    {
        if (waveCreationTime <= 0)
        {
            waveCreationTime = SystemAPI.GetSingleton<EntitiesReferences>().waveTimer;
            Debug.Log (waveCreationTime);
        }

        //var soldierQuery = SystemAPI.QueryBuilder().WithAll<Zombie>().Build();
        //if (soldierQuery.IsEmpty)
        waveCreationTime -= SystemAPI.Time.DeltaTime;
        
        if (waveCreationTime <= 0 )
        {
            var references = SystemAPI.GetSingleton<EntitiesReferences>();
            var entities = CollectionHelper.CreateNativeArray<Entity, RewindableAllocator>(references.waveNumber, ref state.WorldUnmanaged.UpdateAllocator);
            state.EntityManager.Instantiate( references.zombiePrefabEntity , entities);
            Debug.Log("Creating zombies ! ");
            //waveCreationTime  = SystemAPI.GetSingleton<EntitiesReferences>().waveTimer;
            
            var setEntityPosition = new SetEntityPositionJob
            {
                random = Random, 
                circle = 10,
                initialPosition = new float3(0,1,0)
            };
            setEntityPosition.ScheduleParallel ();
        }
    }
}

[BurstCompile]
[WithAll(typeof(Zombie))]
[WithAll(typeof(LocalTransform))]
partial struct SetEntityPositionJob: IJobEntity
{
    public Random random;
    public int circle;
    public float3 initialPosition;
    public int is3DMov;

    [BurstCompile]
    public void Execute( ref LocalTransform transform, in Zombie soldier )
    {
        // Generate a random angle in radians
        float randomAngle = random.NextFloat(0, 2 * math.PI);
        float randomRadius = random.NextFloat(0, circle);
        float x = initialPosition.x + randomRadius * math.cos(randomAngle);
        float z = initialPosition.z + randomRadius * math.sin(randomAngle);
        float y = 0;
        if(is3DMov == 1) y = random.NextFloat(0, circle);
        transform.Position = new float3(x,y,z);
    }
}
