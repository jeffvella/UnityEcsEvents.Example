using Assets.Game.Scripts.Components.Events;
using Assets.Scripts.Components.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using Vella.Events;
using Assets.Scripts.Components;

namespace Assets.Game.Scripts.Systems
{
    public class LevelCreateSystem : SystemBase
    {
        private EventQueue<SpawnActorEvent> _spawnActorEvents;
        private EventQueue<SpawnPlayerEvent> _spawnPlayerEvents;

        protected override void OnCreate()
        {
            var eventsystem = World.GetOrCreateSystem<EntityEventSystem>();
            _spawnActorEvents = eventsystem.GetQueue<SpawnActorEvent>();
            _spawnPlayerEvents = eventsystem.GetQueue<SpawnPlayerEvent>();
        }

        protected override void OnUpdate()
        {
            var spawnPlayerEvents = _spawnPlayerEvents;

            Entities.ForEach((in SceneLoadedEvent e) =>
            {
                if (e.SceneCategory != SceneCategory.Level)
                    return;
              
                //spawnPlayerEvents.Enqueue(new SpawnPlayerEvent
                //{
                //    Category = ActorCategory.HumanPlayer
                //});
               
            }).Run();

            var spawnActorEvents = _spawnActorEvents;

            Entities.ForEach((in PlayerCreatedEvent e) =>
            {
                spawnActorEvents.Enqueue(new SpawnActorEvent
                {
                    OwnerId = e.Id,
                    Amount = 1,
                    Catetory = ActorCategory.Defender
                });

            }).Run();
        }
    }
}
