using Components.AI;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct AgentNavAspect : IAspect
{
    public readonly RefRW<NavAgent> agent;
    public readonly RefRW<NavAgentMovement> agentMovement;
    public readonly DynamicBuffer<NavAgentBuffer> agentBuffer;
    public readonly DynamicBuffer<NavAgentPathValidityBuffer> agentPathValidityBuffer;
    public readonly RefRW<LocalTransform> trans;
    
    public void moveAgent(float deltaTime, float minDistanceReached, float agentSpeed, float agentRotationSpeed)
    {
        if (agentBuffer.Length > 0 && agent.ValueRO.PathCalculated && !agentMovement.ValueRO.Reached)
        {
            agentMovement.ValueRW.WaypointDirection = math.normalize(agentBuffer[agentMovement.ValueRO.CurrentBufferIndex].WayPoints - trans.ValueRO.Position);
            if (!float.IsNaN(agentMovement.ValueRW.WaypointDirection.x))
            {
                trans.ValueRW.Position += agentMovement.ValueRW.WaypointDirection * agentSpeed * deltaTime;
                trans.ValueRW.Rotation = math.slerp(
                    trans.ValueRW.Rotation, 
                    quaternion.LookRotation(agentMovement.ValueRW.WaypointDirection, math.up()), 
                    deltaTime * agentRotationSpeed);
                if (math.distance(trans.ValueRO.Position, agentBuffer[agentBuffer.Length - 1].WayPoints) <= minDistanceReached)
                {
                    agentMovement.ValueRW.Reached = true;
                }
                else if (math.distance(trans.ValueRO.Position, agentBuffer[agentMovement.ValueRO.CurrentBufferIndex].WayPoints) <= minDistanceReached)
                {
                    agentMovement.ValueRW.CurrentBufferIndex = agentMovement.ValueRW.CurrentBufferIndex + 1;
                }
            }
            else if (!agentMovement.ValueRO.Reached)
            {
                agentMovement.ValueRW.CurrentBufferIndex = agentMovement.ValueRW.CurrentBufferIndex + 1;
            }
        }
    }
}
