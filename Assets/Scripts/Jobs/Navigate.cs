using Components.AI;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine.Experimental.AI;

[BurstCompile]
public struct Navigate : IJob
{
    public NavMeshQuery query;
    [NativeDisableContainerSafetyRestriction] public DynamicBuffer<NavAgentBuffer> ab;
    public float3 fromLocation;
    public float3 toLocation;
    public float3 extents;
    public int maxIteration;
    public int maxPathSize;
    NavMeshLocation nml_FromLocation;
    NavMeshLocation nml_ToLocation;
    PathQueryStatus status;
    PathQueryStatus returningStatus;

    public void Execute()
    {
        nml_FromLocation = query.MapLocation(fromLocation, extents, 0);
        nml_ToLocation = query.MapLocation(toLocation, extents, 0);
        if (query.IsValid(nml_FromLocation) && query.IsValid(nml_ToLocation))
        {
            status = query.BeginFindPath(nml_FromLocation, nml_ToLocation, -1);
            if (status == PathQueryStatus.InProgress)
            {
                status = query.UpdateFindPath(maxIteration, out int iterationPerformed);
                if (status == PathQueryStatus.Success)
                {
                    status = query.EndFindPath(out int polygonSize);
                    NativeArray<NavMeshLocation> res = new NativeArray<NavMeshLocation>(polygonSize, Allocator.Temp);
                    NativeArray<StraightPathFlags> straightPathFlag = new NativeArray<StraightPathFlags>(maxPathSize, Allocator.Temp);
                    NativeArray<float> vertexSide = new NativeArray<float>(maxPathSize, Allocator.Temp);
                    NativeArray<PolygonId> polys = new NativeArray<PolygonId>(polygonSize, Allocator.Temp);
                    int straightPathCount = 0;
                    int a = query.GetPathResult(polys);
                    returningStatus = PathUtils.FindStraightPath(
                        query,
                        fromLocation,
                        toLocation,
                        polys,
                        polygonSize,
                        ref res,
                        ref straightPathFlag,
                        ref vertexSide,
                        ref straightPathCount,
                        maxPathSize
                    );
                    if (returningStatus == PathQueryStatus.Success)
                    {
                        for (int i = 0; i < straightPathCount; i++)
                        {
                            if (!(math.distance(fromLocation, res[i].position) < 1) && query.IsValid(query.MapLocation(res[i].position, extents, 0)))
                            {
                                ab.Add(new NavAgentBuffer { WayPoints = new float3(res[i].position.x, fromLocation.y, res[i].position.z) });
                            }
                        }
                    }
                    res.Dispose();
                    straightPathFlag.Dispose();
                    polys.Dispose();
                    vertexSide.Dispose();
                }
            }
        }
    }
}
