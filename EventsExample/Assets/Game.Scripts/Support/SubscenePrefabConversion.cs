using Assets.Scripts.Components;
using Assets.Scripts.Components.Tags;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class SubscenePrefabConversion : MonoBehaviour, IConvertGameObjectToEntity
{
    public bool StripTransform;
    public bool LinkChildren;
    public bool AddPrefabComponent;
    public bool AddUnprocessedTag;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var components = new List<ComponentType>();
        if (AddPrefabComponent)
            components.Add(ComponentType.ReadWrite<Prefab>());
        if (AddUnprocessedTag)
            components.Add(ComponentType.ReadWrite<UnprocessedTag>());

        if (StripTransform)
        {
            if(dstManager.HasComponent<LocalToParent>(entity))
                dstManager.RemoveComponent<LocalToParent>(entity);
            if (dstManager.HasComponent<LocalToParent>(entity))
                dstManager.RemoveComponent<LocalToWorld>(entity);
            if (dstManager.HasComponent<Translation>(entity))
                dstManager.RemoveComponent<Translation>(entity);
            if (dstManager.HasComponent<Rotation>(entity))
                dstManager.RemoveComponent<Rotation>(entity);
        }

        foreach (var component in components)
            dstManager.AddComponent(entity, component);

        // A linked entity group allows child entities to be instanitated with their parent.
        if (LinkChildren)
        {
            conversionSystem.DeclareLinkedEntityGroup(gameObject);
        }
    }
}

