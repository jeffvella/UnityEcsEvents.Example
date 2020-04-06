using Assets.Scripts.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Assets.Scripts.Systems
{
    public class MovementSystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _commandSystem;

        protected override void OnCreate()
        {
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            var delta = Time.DeltaTime;
            var commands = _commandSystem.CreateCommandBuffer().ToConcurrent();

            Entities.ForEach((Entity entity, int nativeThreadIndex, ref MovementTarget target, in MovementInfo info) =>
            {
                float3 origin = GetComponent<Translation>(entity).Value;

                float3 destination = target.Entity != Entity.Null
                    ? GetComponent<Translation>(target.Entity).Value
                    : target.Position;

                commands.SetComponent(nativeThreadIndex, entity, new Translation
                {
                    Value = origin + math.normalize(destination - origin) * info.Speed * delta
                });

            }).Schedule();
        }

    }
}