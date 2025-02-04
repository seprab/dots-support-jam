using Unity.Entities;
using Unity.Mathematics;

namespace Components.AI
{
    public struct NavAgentMovement : IComponentData
    {
        public int CurrentBufferIndex;
        public bool Reached;
        public float3 WaypointDirection;
    }
}
