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
        void OnEvent(T e);
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

        public unsafe struct DelegateInvoker<THandler, T> : IDelegateInvoker, IEquatable<DelegateInvoker<THandler, T>>
            where THandler : IEventObserver<T>
            where T : unmanaged, IComponentData
        {
            public THandler Handler;

            public void Execute(void* ptr) => Handler.OnEvent(*(T*)ptr);

            public void ExecuteDefault() => Handler.OnEvent(default);

            public bool Equals(DelegateInvoker<THandler, T> other)
            {
                return Handler.GetHashCode() == other.Handler.GetHashCode();
            }

            public override int GetHashCode()
            {
                int hash = 13;
                hash = (hash * 7) + Handler.GetHashCode();
                hash = (hash * 7) + typeof(T).GetHashCode();
                hash = (hash * 7) + typeof(THandler).GetHashCode();
                return hash;
            }
        }

        internal void AddListener<THandler, T>(THandler handler = default)
            where THandler : IEventObserver<T>
            where T : unmanaged, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            var invoker = new DelegateInvoker<THandler, T>
            {
                Handler = handler
            };

            _dispatcherSystem.Add(typeIndex, invoker);
        }

        internal void RemoveListener<THandler, T>(THandler handler = default)
            where THandler : IEventObserver<T>
            where T : unmanaged, IComponentData
        {
            var typeIndex = TypeManager.GetTypeIndex<T>();
            var invoker = new DelegateInvoker<THandler, T>
            {
                Handler = handler
            };
            _dispatcherSystem.Remove(typeIndex, invoker);
        }

        public class EventDispatcher : SystemBase
        {
            private Dictionary<int, List<IDelegateInvoker>> _actions;
            private EntityQuery _query;

            public EventDispatcher()
            {
                _actions = new Dictionary<int, List<IDelegateInvoker>>();
            }

            protected override void OnCreate()
            {
                _actions = new Dictionary<int, List<IDelegateInvoker>>();
                _query = EntityManager.CreateEntityQuery(ComponentType.ReadOnly<EntityEvent>());

                RequireForUpdate(_query);
            }

            internal void Add<THandler, T>(int typeIndex, DelegateInvoker<THandler, T> invoker)
                where THandler : IEventObserver<T>
                where T : unmanaged, IComponentData
            {
                if (!_actions.ContainsKey(typeIndex))
                {
                    _actions[typeIndex] = new List<IDelegateInvoker> { invoker };
                }
                else
                {
                    _actions[typeIndex].Add(invoker);
                }
            }

            internal void Remove<THandler, T>(int typeIndex, DelegateInvoker<THandler, T> invoker)
                where THandler : IEventObserver<T>
                where T : unmanaged, IComponentData
            {
                if (_actions.ContainsKey(typeIndex))
                {
                    var invokers = _actions[typeIndex];
                    if (invokers == null || invokers.Count == 0)
                        return;

                    invokers.Remove(invoker);
                }
            }

            protected unsafe override void OnUpdate()
            {
                var chunks = _query.CreateArchetypeChunkArray(Allocator.TempJob);
                var uem = EntityManager.Unsafe;

                foreach (var chunk in chunks)
                {
                    
                    var componentTypeIndex = uem.GetComponentPtr<EntityEvent>(chunk)->ComponentTypeIndex;
                    if (componentTypeIndex != 0)
                    {
                        if (_actions.TryGetValue(componentTypeIndex, out var list))
                        {
                            var componentsPtr = uem.GetComponentPtr(chunk, componentTypeIndex);
                            var typeInfo = TypeManager.GetTypeInfo(componentTypeIndex);
                            var componentType = ComponentType.FromTypeIndex(componentTypeIndex);

                            for (int i = 0; i < list.Count; i++)
                            {
                                for (int j = 0; j < chunk.Count; j++)
                                {
                                    if (componentType.IsZeroSized)
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