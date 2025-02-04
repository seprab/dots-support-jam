using Components.AI;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Experimental.AI;

[BurstCompile]
public struct PathValidity : IJob
{
    public NavMeshQuery query;
    public float3 extents;
    public int currentBufferIndex;
    public LocalTransform trans;
    public float unitsInDirection;
    [NativeDisableContainerSafetyRestriction] public DynamicBuffer<NavAgentBuffer> ab;
    [NativeDisableContainerSafetyRestriction] public DynamicBuffer<NavAgentPathValidityBuffer> apvb;
    NavMeshLocation startLocation;
    UnityEngine.AI.NavMeshHit navMeshHit;
    PathQueryStatus status;

    public void Execute()
    {
        if (currentBufferIndex < ab.Length)
        {
            if (!query.IsValid(query.MapLocation(ab.ElementAt(currentBufferIndex).WayPoints, extents, 0)))
            {
                apvb.Add(new NavAgentPathValidityBuffer { IsPathInvalid = true });
            }
            else
            {
                startLocation = query.MapLocation(trans.Position + (trans.Forward() * unitsInDirection), extents, 0);
                status = query.Raycast(out navMeshHit, startLocation, ab.ElementAt(currentBufferIndex).WayPoints);
                
                if (status == PathQueryStatus.Success)
                {
                    if ((math.ceil(navMeshHit.position).x != math.ceil(ab.ElementAt(currentBufferIndex).WayPoints.x)) &&
                        (math.ceil(navMeshHit.position).z != math.ceil(ab.ElementAt(currentBufferIndex).WayPoints.z)))
                    {
                        apvb.Add(new NavAgentPathValidityBuffer { IsPathInvalid = true });
                    }
                }
                else
                {
                    apvb.Add(new NavAgentPathValidityBuffer { IsPathInvalid = true });
                }
            }
        }
    }
}
