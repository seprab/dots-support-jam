using Components.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine.Experimental.AI;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

partial struct AgentNavSystem : ISystem, ISystemStartStop
{
    NativeArray<JobHandle> pathFindingJobs;
    NativeArray<JobHandle> pathValidyJobs;
    private EntityQuery eqNavAgents;
    private EntityQuery eqNavProps;
    RefRW<NavGlobalProperties> properties;
    NativeArray<NavMeshQuery> pathFindingQueries;
    NativeArray<NavMeshQuery> pathRecaclulatingQueries;
    public void OnStartRunning(ref SystemState state)
    {
        
    }

    public void OnStopRunning(ref SystemState state)
    {
    }
    
    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        eqNavAgents = state.EntityManager.CreateEntityQuery(typeof(NavAgent));
        eqNavProps = state.EntityManager.CreateEntityQuery(typeof(NavGlobalProperties));
        int globalProps = eqNavProps.CalculateEntityCount();
        if (globalProps != 1) return;
        int i = 0;
        pathFindingQueries = new NativeArray<NavMeshQuery>(eqNavAgents.CalculateEntityCount(), Allocator.Temp);
        properties = SystemAPI.GetSingletonRW<NavGlobalProperties>();
        pathFindingJobs = new NativeArray<JobHandle>(eqNavAgents.CalculateEntityCount(), Allocator.Temp);
        foreach (AgentNavAspect ana in SystemAPI.Query<AgentNavAspect>())
        {
            if (properties.ValueRO.DynamicPathFinding && ana.agentPathValidityBuffer.Length > 0 && ana.agentPathValidityBuffer.ElementAt(0).IsPathInvalid)
            {
                //Checks if dynamic pathfinding is enabled and the agent's path is invalid. If so, it resets the agent's path buffer, path calculation state, and validity buffer to force a new path calculation.
                ana.agentBuffer.Clear();
                ana.agentMovement.ValueRW.CurrentBufferIndex = 0;
                ana.agent.ValueRW.PathCalculated = false;
                ana.agentPathValidityBuffer.Clear();
            }
            if (properties.ValueRO.AgentMovementEnabled && properties.ValueRO.RetracePath && ana.agent.ValueRW.UsingGlobalRelativeLoction && ana.agentMovement.ValueRO.Reached)
            {
                // Checks if agent movement, path retracing, and global relative location are enabled, and if the agent has reached its destination. If so, resets the path buffer and state, and marks the agent as not having reached the destination.
                ana.agentBuffer.Clear();
                ana.agentMovement.ValueRW.CurrentBufferIndex = 0;
                ana.agent.ValueRW.PathCalculated = false;
                ana.agentPathValidityBuffer.Clear();
                ana.agentMovement.ValueRW.Reached = false;
            }
            if (!ana.agent.ValueRO.PathCalculated || ana.agentBuffer.Length == 0)
            {
                // If the agent's path has not been calculated or the path buffer is empty, it creates a new NavMeshQuery and schedules a navigation job.
                pathFindingQueries[i] = new NavMeshQuery(NavMeshWorld.GetDefaultWorld(), Allocator.TempJob, properties.ValueRO.MaxPathNodePoolSize);
                ana.agent.ValueRW.PathFindingQueryIndex = i;
                if (properties.ValueRO.SetGlobalRelativeLocation && !ana.agent.ValueRO.UsingGlobalRelativeLoction)
                {
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
            pathRecaclulatingQueries = new NativeArray<NavMeshQuery>(eqNavAgents.CalculateEntityCount(), Allocator.Temp);
            pathValidyJobs = new NativeArray<JobHandle>(eqNavAgents.CalculateEntityCount(), Allocator.Temp);
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
