using Unity.Entities;
using Unity.Mathematics;

public struct NavGlobalProperties : IComponentData
{
    public int MaxIteration;
    public int MaxPathSize;
    public int MaxPathNodePoolSize;
    public float3 Extents;
    public bool DynamicPathFinding;
    public float MinimumDistanceToWaypoint;
    public bool AgentMovementEnabled;
    public float3 Units;
    public bool SetGlobalRelativeLocation;
    public float DynamicPathRecalculatingFrequency;
    public float UnitsInForwardDirection;
    public bool RetracePath;
    public float AgentSpeed;
    public float RotationSpeed;
}
