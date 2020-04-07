using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Vella.Events;

#if (UNITY_EDITOR && ENABLE_UNITY_COLLECTIONS_CHECKS)

namespace Assets.Scripts.Systems
{
    /// <summary>
    /// Adds friendly names to Events for easier viewing in the EntityDebugger
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class EventDebugSystem : SystemBase
    {
        private EntityQuery _allEvents;

        protected override void OnUpdate()
        {
            Entities.ForEach((Entity entity, in EntityEvent eventInfo) =>
            {
                var name = TypeManager.GetType(eventInfo.ComponentTypeIndex).Name;
                EntityManager.SetName(entity, $"{entity}, {name}: {eventInfo.Id}");

            }).WithStructuralChanges().WithStoreEntityQueryInField(ref _allEvents).Run();
        }
    }

}

#endif