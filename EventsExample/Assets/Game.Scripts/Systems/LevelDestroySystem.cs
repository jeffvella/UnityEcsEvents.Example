using Assets.Game.Scripts.Components.Events;
using Assets.Scripts.Components.Tags;
using Unity.Collections;
using Unity.Entities;

namespace Assets.Game.Scripts.Systems
{
    public class LevelDestroySystem : SystemBase
    {
        private EndSimulationEntityCommandBufferSystem _commandSystem;
        private EntityQuery _actorsQuery;

        protected override void OnCreate()
        {
            _commandSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            _actorsQuery = EntityManager.CreateEntityQuery(new EntityQueryDesc
            {
                Any = new[]
                {
                    ComponentType.ReadWrite<PlayerTag>(),
                    ComponentType.ReadWrite<AttackerTag>(),
                    ComponentType.ReadWrite<DefenderTag>(),
                }
            });
        }

        protected override void OnUpdate()
        {
            // todo: purge EntityEventSystem queues.

            // todo: is there a more efficient way to delete entities with children?

            // ArgumentException: DestroyEntity(EntityQuery query) is destroying entity Entity(55:1) 
            // which contains a LinkedEntityGroup and the entity Entity(93:1) in that group is not 
            // included in the query. If you want to destroy entities using a query all linked 
            // entities must be contained in the query..

            var entities = _actorsQuery.ToEntityArray(Allocator.TempJob);
            var buffers = GetBufferFromEntity<LinkedEntityGroup>();
            var commands = _commandSystem.CreateCommandBuffer();

            Entities.ForEach((in SceneUnloadedEvent e) =>
            {
                if (e.Category != SceneCategory.Level)
                    return;

                for (int i = 0; i < entities.Length; i++)
                {
                    var entity = entities[i];
                    if (buffers.Exists(entity))
                    {
                        var children = buffers[entity];
                        for (int j = 0; j < children.Length; j++)
                            commands.DestroyEntity(children[j].Value);
                    }
                    commands.DestroyEntity(entity);
                }

            }).Run();

            entities.Dispose();
        }
    }
}
