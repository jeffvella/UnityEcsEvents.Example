using Assets.Scripts.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

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
                var origin = GetComponent<Translation>(entity).Value;

                var destination = target.Entity != Entity.Null ? GetComponent<Translation>(target.Entity).Value : target.Position;

                commands.SetComponent(nativeThreadIndex, entity, new Translation {Value = origin + math.normalize(destination - origin) * info.Speed * delta});
            }).Schedule();

            _commandSystem.AddJobHandleForProducer(Dependency);
        }
    }
}