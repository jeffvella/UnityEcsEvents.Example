using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Vella.Events;

namespace Assets.Scripts.Systems
{
    public struct DetectorInfo
    {
        public Entity Entity;
        public float Range;
        public float3 Position;
        public OwnerRef Owner;
    }

    public struct Hit
    {
        public Entity Target;
        public float3 Position;
    }

    public class SimpleCollisionSystem : SystemBase
    {
        private EventQueue<CollisionEvent> _collisionEvents;
        private NativeList<DetectorInfo> _buffer;

        protected override void OnCreate()
        {
            _collisionEvents = World.GetOrCreateSystem<EntityEventSystem>().GetQueue<CollisionEvent>();
            _buffer = new NativeList<DetectorInfo>(1, Allocator.Persistent);
        }

        protected override void OnUpdate()
        {
            var collisionEvents = _collisionEvents;
            var detectors = _buffer;
            detectors.Clear();

            Entities.ForEach((Entity entity, ref CollisionDetector detector, in Translation pos, in OwnerRef owner) =>
            {
                detectors.Add(new DetectorInfo
                {
                    Entity = entity,
                    Position = pos.Value,
                    Range = detector.Range,
                    Owner = owner
                });

            }).Run();

            Entities.WithNone<CollisionDetector>().ForEach((Entity entity, ref ActorDefinition def, ref Translation pos) =>
            {
                for (int i = 0; i < detectors.Length; i++)
                {
                    var detector = detectors[i];
                    var distance = math.distance(detector.Position, pos.Value);
                    if (distance < detector.Range)
                    {
                        collisionEvents.Enqueue(new CollisionEvent
                        {
                            Source = detector,
                            Hit = new Hit
                            {
                                Position = detector.Position + (pos.Value - detector.Position),
                                Target = entity
                            },
                        });
                    }
                }

            }).WithReadOnly(detectors).WithoutBurst().Run();
        }

        protected override void OnDestroy()
        {
            _buffer.Dispose();
        }
    }
}
