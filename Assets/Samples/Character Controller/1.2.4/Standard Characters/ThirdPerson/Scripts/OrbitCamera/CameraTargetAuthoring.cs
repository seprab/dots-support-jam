using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[DisallowMultipleComponent]
public class CameraTargetAuthoring : MonoBehaviour
{
    public GameObject Target;
    public Transform shootPoint;

    public class Baker : Baker<CameraTargetAuthoring>
    {
        public override void Bake(CameraTargetAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CameraTarget
            {
                TargetEntity = GetEntity(authoring.Target, TransformUsageFlags.Dynamic),
            });
            
            AddComponent(entity, new ShootPoint
            {
                Value = GetEntity(authoring.shootPoint, TransformUsageFlags.Dynamic),
            });
        }
    }
}