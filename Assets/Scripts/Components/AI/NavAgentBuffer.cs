using Unity.Entities;
using Unity.Mathematics;

namespace Components.AI
{
    public struct NavAgentBuffer : IBufferElementData
    {
        public float3 WayPoints;
    }
}
