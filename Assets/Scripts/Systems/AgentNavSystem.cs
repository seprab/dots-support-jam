using Components.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Experimental.AI;
using Unity.Mathematics;

partial struct AgentNavSystem : ISystem, ISystemStartStop
{
    NativeArray<JobHandle> pathFindingJobs;
    NativeArray<JobHandle> pathValidyJobs;
    EntityQuery eq;
    RefRW<NavGlobalProperties> properties;
    NativeArray<NavMeshQuery> pathFindingQueries;
    NativeArray<NavMeshQuery> pathRecaclulatingQueries;
    private int count;
    
    public void OnStartRunning(ref SystemState state)
    {
        eq = state.EntityManager.CreateEntityQuery(typeof(NavAgent));
        count = state.EntityManager.CreateEntityQuery(typeof(NavGlobalProperties)).CalculateEntityCount();
    }

    public void OnStopRunning(ref SystemState state)
    {
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        if (count != 1) return;
        int i = 0;
        pathFindingQueries = new NativeArray<NavMeshQuery>(eq.CalculateEntityCount(), Allocator.Temp);
        properties = SystemAPI.GetSingletonRW<NavGlobalProperties>();
        pathFindingJobs = new NativeArray<JobHandle>(eq.CalculateEntityCount(), Allocator.Temp);
        foreach (AgentNavAspect ana in SystemAPI.Query<AgentNavAspect>())
        {
            if (properties.ValueRO.DynamicPathFinding && ana.agentPathValidityBuffer.Length > 0 && ana.agentPathValidityBuffer.ElementAt(0).IsPathInvalid)
            {
                ana.agentBuffer.Clear();
                ana.agentMovement.ValueRW.CurrentBufferIndex = 0;
                ana.agent.ValueRW.PathCalculated = false;
                ana.agentPathValidityBuffer.Clear();
            }
            if (properties.ValueRO.AgentMovementEnabled && properties.ValueRO.RetracePath && ana.agent.ValueRW.UsingGlobalRelativeLoction && ana.agentMovement.ValueRO.Reached)
            {
                ana.agent.ValueRW.ToLocation = new float3(ana.agent.ValueRW.ToLocation.x, ana.agent.ValueRW.ToLocation.y, -ana.agent.ValueRW.ToLocation.z);
                ana.agentBuffer.Clear();
                ana.agentMovement.ValueRW.CurrentBufferIndex = 0;
                ana.agent.ValueRW.PathCalculated = false;
                ana.agentPathValidityBuffer.Clear();
                ana.agentMovement.ValueRW.Reached = false;
            }
            if (!ana.agent.ValueRO.PathCalculated || ana.agentBuffer.Length == 0)
            {
                pathFindingQueries[i] = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, properties.ValueRO.MaxPathNodePoolSize);
                ana.agent.ValueRW.PathFindingQueryIndex = i;
                if (properties.ValueRO.SetGlobalRelativeLocation && !ana.agent.ValueRO.UsingGlobalRelativeLoction)
                {
                    ana.agent.ValueRW.ToLocation = ana.trans.ValueRO.Position + properties.ValueRO.Units;
                    ana.agent.ValueRW.UsingGlobalRelativeLoction = true;
                }
                pathFindingJobs[i] = new Navigate
                {
                    query = pathFindingQueries[i],
                    ab = ana.agentBuffer,
                    fromLocation = ana.trans.ValueRO.Position,
                    toLocation = ana.agent.ValueRO.ToLocation,
                    extents = properties.ValueRO.Extents,
                    maxIteration = properties.ValueRO.MaxIteration,
                    maxPathSize = properties.ValueRO.MaxPathSize
                }.Schedule(state.Dependency);
                ana.agent.ValueRW.PathCalculated = true;
                ana.agent.ValueRW.PathFindingQueryDisposed = false;
            }
            i++;
        }
        JobHandle.CompleteAll(pathFindingJobs);
        foreach (AgentNavAspect ana in SystemAPI.Query<AgentNavAspect>())
        {
            if (ana.agent.ValueRO.PathCalculated && !ana.agent.ValueRW.PathFindingQueryDisposed)
            {
                pathFindingQueries[ana.agent.ValueRW.PathFindingQueryIndex].Dispose();
                ana.agent.ValueRW.PathFindingQueryDisposed = true;
            }
        }
        pathFindingQueries.Dispose();

        if (properties.ValueRO.DynamicPathFinding)
        {
            int j = 0;
            pathRecaclulatingQueries = new NativeArray<NavMeshQuery>(eq.CalculateEntityCount(), Allocator.Temp);
            pathValidyJobs = new NativeArray<JobHandle>(eq.CalculateEntityCount(), Allocator.Temp);
            foreach (AgentNavAspect ana in SystemAPI.Query<AgentNavAspect>())
            {
                if (!ana.agentMovement.ValueRO.Reached)
                {
                    ana.agent.ValueRW.ElapsedSinceLastPathCalculation += SystemAPI.Time.DeltaTime;
                    if (ana.agent.ValueRW.ElapsedSinceLastPathCalculation > properties.ValueRO.DynamicPathRecalculatingFrequency)
                    {
                        pathRecaclulatingQueries[j] = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, properties.ValueRO.MaxPathNodePoolSize);
                        ana.agent.ValueRW.ElapsedSinceLastPathCalculation = 0;
                        pathValidyJobs[j] = new PathValidity
                        {
                            query = pathRecaclulatingQueries[j],
                            extents = properties.ValueRO.Extents,
                            currentBufferIndex = ana.agentMovement.ValueRW.CurrentBufferIndex,
                            trans = ana.trans.ValueRW,
                            unitsInDirection = properties.ValueRO.UnitsInForwardDirection,
                            ab = ana.agentBuffer,
                            apvb = ana.agentPathValidityBuffer
                        }.Schedule(state.Dependency);
                        j++;
                    }
                }
            }
            JobHandle.CompleteAll(pathValidyJobs);

            for (int k = 0; k < j; k++)
            {
                pathRecaclulatingQueries[k].Dispose();
            }
            pathRecaclulatingQueries.Dispose();
        }

        if (properties.ValueRO.AgentMovementEnabled)
        {
            new MoveJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                minDistance = properties.ValueRO.MinimumDistanceToWaypoint,
                agentSpeed = properties.ValueRO.AgentSpeed,
                agentRotationSpeed = properties.ValueRO.RotationSpeed
            }.ScheduleParallel();
        }
    }

    [BurstCompile]
    public partial struct MoveJob : IJobEntity
    {
        public float deltaTime;
        public float minDistance;
        public float agentSpeed;
        public float agentRotationSpeed;
        public void Execute(AgentNavAspect ana)
        {
            ana.moveAgent(deltaTime, minDistance, agentSpeed, agentRotationSpeed);
        }
    }
}
