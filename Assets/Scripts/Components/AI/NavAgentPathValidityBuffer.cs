using Unity.Entities;

namespace Components.AI
{
    public struct NavAgentPathValidityBuffer : IBufferElementData
    {
        public bool IsPathInvalid;
    }
}
