using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
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
    
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    //[UpdateBefore(typeof(EntityEventSystem))]
    public class SimpleCollisionSystem : SystemBase
    {
        // There's an important timing consideration here at the moment:
        // this detects colliding entity + creates collision events, 
        // >> next frame.
        // explosion system is triggered by existance of collion events + queues delete of colliding entity in ECB.
        // to prevent this system running a second time on the same entities, 
        // it needs to update after the simulation command buffer end.

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
                        var hit = new Hit
                        {
                            Position = detector.Position + (pos.Value - detector.Position),
                            Target = entity
                        };
                        var collision = new CollisionEvent
                        {
                            Source = detector,
                            Hit = hit,
                        }; 
                        
                        //Debug.Log($"Collision Source={collision.Source.Entity} Target={hit.Target}");
                        collisionEvents.Enqueue(collision);
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
