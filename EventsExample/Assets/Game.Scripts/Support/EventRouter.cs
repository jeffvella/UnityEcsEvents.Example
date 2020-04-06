using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Vella.Events;
using Vella.Events.Extensions;

namespace Assets.Scripts.UI
{
    public unsafe interface IEventObserver<T> where T : unmanaged, IComponentData
    {
        void OnEvent(T eventData);
    }

    public class EventRouter : MonoBehaviour
    {
        private World _world;
        private EventDispatcher _dispatcherSystem;
        private EntityEventSystem _eventSystem;

        public void FireEvent<T>(T eventData) where T : struct, IComponentData
        {
            _eventSystem.Enqueue(eventData);
        }

        private void Awake()
        {
            _world = World.DefaultGameObjectInjectionWorld;
            _dispatcherSystem = _world.GetOrCreateSystem<EventDispatcher>();
            _eventSystem = _world.GetOrCreateSystem<EntityEventSystem>();
        }

        public unsafe interface IDelegateInvoker
        {
            void Execute(void* ptr);

            void ExecuteDefault();
        }

        public unsafe struct DelegateInvoker<THandler, T> : IDelegateInvoker
            where THandler : IEventObserver<T>
            where T : unmanaged, IComponentData
        {
            public void* Ptr;
            public THandler Handler;

            public void Execute(void* ptr) => Handler.OnEvent(*(T*)ptr);

            public void ExecuteDefault() => Handler.OnEvent(default);
        }

        internal void AddListener<THandler, T>(THandler handler = default)
            where THandler : IEventObserver<T>
            where T : unmanaged, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            var component = ComponentType.FromTypeIndex(typeIndex);
            var invoker = new DelegateInvoker<THandler, T>
            {
                Handler = handler
            };

            _dispatcherSystem.Add(component, invoker);
        }

        public class EventDispatcher : SystemBase
        {
            private Dictionary<ComponentType, List<IDelegateInvoker>> _actions;
            private EntityQuery _query;

            public EventDispatcher()
            {
                _actions = new Dictionary<ComponentType, List<IDelegateInvoker>>();
            }

            protected override void OnCreate()
            {
                _actions = new Dictionary<ComponentType, List<IDelegateInvoker>>();
                _query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<EntityEvent>());

                RequireForUpdate(_query);
            }

            internal void Add<THandler, T>(ComponentType component, DelegateInvoker<THandler, T> invoker)
                where THandler : IEventObserver<T>
                where T : unmanaged, IComponentData
            {
                if (!_actions.ContainsKey(component))
                {
                    _actions[component] = new List<IDelegateInvoker> { invoker };
                }
                else
                {
                    _actions[component].Add(invoker);
                }
            }

            protected unsafe override void OnUpdate()
            {
                var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);

                foreach (var chunk in chunks)
                {
                    var uem = new UnsafeEntityManager(chunk);
                    var componentTypeIndex = uem.GetComponentPtr<EntityEvent>(chunk)->ComponentTypeIndex;
                    if (componentTypeIndex != 0)
                    {
                        var componentType = ComponentType.FromTypeIndex(componentTypeIndex);
                        if (_actions.TryGetValue(componentType, out var list))
                        {
                            var componentsPtr = uem.GetComponentPtr(chunk, componentTypeIndex);
                            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);

                            for (int i = 0; i < list.Count; i++)
                            {
                                for (int j = 0; j < chunk.Count; j++)
                                {
                                    if (typeInfo.IsZeroSized)
                                    {
                                        list[i].ExecuteDefault();
                                    }
                                    else
                                    {
                                        list[i].Execute(componentsPtr + j * typeInfo.SizeInChunk);
                                    }
                                }
                            }
                        }
                    }
                }

                chunks.Dispose();
            }

        }
    }

}