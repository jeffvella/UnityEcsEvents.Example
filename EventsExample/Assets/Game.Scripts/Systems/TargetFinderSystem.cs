using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Components.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vella.Events;
using Vella.Events.Extensions;

namespace Assets.Scripts.Systems
{
    public class TargetFinderSystem : SystemBase
    {
        private EventQueue<TargetAcquiredEvent> _targetEvents;
        private EndSimulationEntityCommandBufferSystem _commandSystem;
        private NativeList<TargetInfo> _targets;
        private EntityQuery _hasNoTargetQuery;

        public struct TargetInfo
        {
            public float3 Position;
            public Entity Entity;
        }

        protected override void OnCreate()
        {
            _targetEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<TargetAcquiredEvent>();
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _targets = new NativeList<TargetInfo>(1, Allocator.Persistent);
            RequireForUpdate(_hasNoTargetQuery);
        }

        protected override void OnUpdate()
        {
            var commands = _commandSystem.CreateCommandBuffer();
            var events = _targetEvents;
            var potentialTargets = _targets;
            potentialTargets.Clear();

            Entities.WithAll<DefenderTag>().ForEach((Entity entity, in Translation pos) =>
            {
                potentialTargets.Add(new TargetInfo
                {
                    Position = pos.Value,
                    Entity = entity
                });

            }).Run();

            Entities.WithNone<MovementTarget>().ForEach((Entity attackerEntity, ref BrainState state, in Translation pos, in Rotation rot, in VisionInfo vision) =>
            {
                for (int i = 0; i < potentialTargets.Length; i++)
                {
                    var target = potentialTargets[i];

                    var effectiveSightDistance = state.Flags.IsFlagSet(BrainFlags.Alerted)
                        ? vision.Distance * 2.5
                        : vision.Distance;

                    if (math.distance(target.Position, pos.Value) > effectiveSightDistance)
                        continue;

                    state.Activity = BrainActivity.InPursuit;

                    commands.AddComponent(attackerEntity, new MovementTarget
                    {
                        Entity = target.Entity
                    });

                    events.Enqueue(new TargetAcquiredEvent
                    {
                         Source = attackerEntity,
                         SourcePosition = pos.Value,
                         Target = target.Entity,
                         TargetPosition = target.Position
                    });

                    break;
                }
            })
            .WithAll<AttackerTag>()
            .WithStoreEntityQueryInField(ref _hasNoTargetQuery)
            .Run();
        }

        protected override void OnDestroy()
        {
            _targets.Dispose();
        }
    }
}
