using Unity.Mathematics;
using UnityEngine;
using Unity.Entities;

public class NavGloablPropsAuthoring : MonoBehaviour
{
    [Header("Navigation Global Properties")]
    public int MaxIteration;
    public int MaxPathSize;
    public int MaxPathNodePoolSize;
    public float3 Extents;
    public bool DynamicPathFinding;
    public float DynamicPathRecalculatingFrequency;
    public float UnitsInForwardDirection;

    [Header("Agent")]
    public bool SetGlobalRelativeLocation;
    public float3 Units;

    [Header("Agent Movement")]
    public bool AgentMovementEnabled;
    public float MinimumDistanceToWaypoint;
    public float agentSpeed;
    public float RotationSpeed;
    public bool RetracePath;
}

public class NavGlobalPropsBaker : Baker<NavGloablPropsAuthoring>
{
    public override void Bake(NavGloablPropsAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new NavGlobalProperties
        {
            MaxIteration = authoring.MaxIteration,
            MaxPathSize = authoring.MaxPathSize,
            MaxPathNodePoolSize = authoring.MaxPathNodePoolSize,
            Extents = authoring.Extents,
            DynamicPathFinding = authoring.DynamicPathFinding,
            MinimumDistanceToWaypoint = authoring.MinimumDistanceToWaypoint,
            AgentMovementEnabled = authoring.AgentMovementEnabled,
            Units = authoring.Units,
            SetGlobalRelativeLocation = authoring.SetGlobalRelativeLocation,
            DynamicPathRecalculatingFrequency = authoring.DynamicPathRecalculatingFrequency,
            UnitsInForwardDirection = authoring.UnitsInForwardDirection,
            RetracePath = authoring.RetracePath,
            AgentSpeed = authoring.agentSpeed,
            RotationSpeed = authoring.RotationSpeed
        });
    }
}
