using Unity.Entities;
using Unity.Mathematics;

namespace Components.AI
{
    public struct NavAgent : IComponentData
    {
        public float3 ToLocation;
        public bool PathCalculated;
        public bool UsingGlobalRelativeLoction;
        public float ElapsedSinceLastPathCalculation;
        public int PathFindingQueryIndex;
        public bool PathFindingQueryDisposed;
    }
}
