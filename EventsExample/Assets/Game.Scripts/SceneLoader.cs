using Assets.Game.Scripts.Components;
using Assets.Game.Scripts.Components.Events;
using Assets.Scripts.UI;
using SubjectNerd.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vella.Events;

namespace Assets.Game.Scripts
{
    public enum SceneId
    {
        None = 0,
        MainMenu,
        Scene1,
    }

    public enum SceneGroupId
    {
        None = 0,
        OutOfGame,
        Levels,
    }

    public class SceneLoader : MonoBehaviour
    {
        public SceneId StartScene;

        [Reorderable]
        public List<SceneEntry> Scenes;

        [Serializable]
        public class SceneEntry
        {
            public SceneId Id;
            public SceneCategory Category;
            public SceneReference Asset;

        }

        private void Start()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            world.GetOrCreateSystem<SceneLoadingSystem>().Initiailize(Scenes);
            world.GetOrCreateSystem<EntityEventSystem>().Enqueue(new RequestSceneLoadEvent
            {
                 Id = StartScene
            });
        }

        public class CleanupSystem : SystemBase
        {
            protected override void OnUpdate()
            {
                Entities.ForEach((in SceneLoadedEvent e) =>
                {
                    Debug.Log($"Scene '{e.Id}' was Loaded");
                    EntityManager.DestroyEntity(e.ProgressEntity);

                }).WithStructuralChanges().Run();

                Entities.ForEach((in SceneUnloadedEvent e) =>
                {
                    Debug.Log($"Scene '{e.Id}' was Unloaded");
                    EntityManager.DestroyEntity(e.ProgressEntity);

                }).WithStructuralChanges().Run();
            }
        }

        [UpdateInGroup(typeof(InitializationSystemGroup))]
        public class SceneLoadingSystem : SystemBase
        {
            public struct AsyncOperationTracker
            {
                public SceneEntry SceneEntry;
                public Entity Entity;
                public AsyncOperation Operation;
                public SceneId SubsequentSceneId;
            }

            private Dictionary<SceneId, SceneEntry> _scenesById;
            private List<AsyncOperationTracker> _currentlyLoading;
            private List<AsyncOperationTracker> _currentlyUnloading;
            private EntityQuery _unloadingInProgressQuery;
            private EntityQuery _loadingInProgressQuery;
            private EntityQuery _requestSceneLoadQuery;
            private EntityQuery _requestSceneUnloadQuery;
            private EntityEventSystem _eventSystem;

            public void Initiailize(List<SceneEntry> scenes)
            {
                _scenesById = scenes.Distinct().ToDictionary(k => k.Id, v => v);
            }

            protected override void OnCreate()
            {
                _currentlyLoading = new List<AsyncOperationTracker>();
                _currentlyUnloading = new List<AsyncOperationTracker>();

                _requestSceneLoadQuery = GetEntityQuery(ComponentType.ReadWrite<RequestSceneLoadEvent>());
                _requestSceneUnloadQuery = GetEntityQuery(ComponentType.ReadWrite<RequestSceneUnloadEvent>());
                _unloadingInProgressQuery = GetEntityQuery(ComponentType.ReadWrite<SceneUnloadingProgress>());
                _loadingInProgressQuery = GetEntityQuery(ComponentType.ReadWrite<SceneLoadingProgress>());

                _eventSystem = World.GetOrCreateSystem<EntityEventSystem>();

                RequireForUpdate(GetEntityQuery(new EntityQueryDesc
                {
                    Any = new[]
                    {
                        ComponentType.ReadWrite<SceneUnloadingProgress>(),
                        ComponentType.ReadWrite<SceneLoadingProgress>(),
                        ComponentType.ReadWrite<RequestSceneLoadEvent>(),
                        ComponentType.ReadWrite<RequestSceneUnloadEvent>(),
                    }
                }));
            }

            protected override void OnUpdate()
            {
                ProcessUnloadRequests();
                ProcessLoadRequests();
                UpdateProgress();
            }

            private void ProcessUnloadRequests()
            {
                if (!_requestSceneUnloadQuery.IsEmptyIgnoreFilter)
                {
                    var requests = _requestSceneUnloadQuery.ToComponentDataArray<RequestSceneUnloadEvent>(Allocator.TempJob);
                    for (int i = 0; i < requests.Length; i++)
                    {
                        var request = requests[i];

                        var sceneEntry = GetSceneEntryById(request.Id);

                        RemoveFromLoadingTracker(sceneEntry);

                        UnloadScene(sceneEntry, request);
                    }
                    requests.Dispose();
                }
            }

            private SceneEntry GetSceneEntryById(SceneId id)
            {
                if (id == SceneId.None)
                    throw new ArgumentException($"'Scene.{SceneId.None}' is not a valid argument");

                if (!_scenesById.TryGetValue(id, out SceneEntry sceneEntry))
                    throw new KeyNotFoundException($"Scene '{id}' was not found");

                return sceneEntry;
            }

            private void UnloadScene(SceneEntry sceneEntry, RequestSceneUnloadEvent request)
            {
                var buildId = SceneManager.GetSceneByPath(sceneEntry.Asset.ScenePath);
                var op = SceneManager.UnloadSceneAsync(buildId);
                var entity = CreateEntity<SceneUnloadingProgress>();

                _currentlyUnloading.Add(new AsyncOperationTracker
                {
                    Entity = entity,
                    Operation = op,
                    SceneEntry = sceneEntry,
                    SubsequentSceneId = request.ThenLoad
                });
            }

            private void RemoveFromLoadingTracker(SceneEntry sceneEntry)
            {
                for (int j = 0; j < _currentlyLoading.Count; j++)
                {
                    var item = _currentlyLoading[j];
                    if (item.SceneEntry.Id == sceneEntry.Id)
                    {
                        // todo: Fire load aborted?
                        _currentlyLoading.RemoveAt(j);
                        EntityManager.DestroyEntity(item.Entity);
                        break;
                    }
                }
            }

            private void ProcessLoadRequests()
            {
                if (!_requestSceneLoadQuery.IsEmptyIgnoreFilter)
                {
                    var requests = _requestSceneLoadQuery.ToComponentDataArray<RequestSceneLoadEvent>(Allocator.TempJob);
                    for (int i = 0; i < requests.Length; i++)
                    {
                        var request = requests[i];

                        var sceneEntry = GetSceneEntryById(request.Id);

                        LoadScene(sceneEntry);
                    }
                    requests.Dispose();
                }
            }

            private void LoadScene(SceneEntry sceneEntry)
            {
                Debug.Log($"Loading Scene: '{sceneEntry.Id}'");
                var op = SceneManager.LoadSceneAsync(sceneEntry.Asset.ScenePath, LoadSceneMode.Additive);
                var entity = CreateEntity<SceneLoadingProgress>();
                _currentlyLoading.Add(new AsyncOperationTracker
                {
                    Entity = entity,
                    Operation = op,
                    SceneEntry = sceneEntry,
                });
            }

            private void UpdateProgress()
            {
                if (!_loadingInProgressQuery.IsEmptyIgnoreFilter)
                {
                    for (int i = _currentlyLoading.Count - 1; i != -1; i--)
                    {
                        var current = _currentlyLoading[i];
                        if (current.Operation.isDone)
                        {
                            _eventSystem.Enqueue(new SceneLoadedEvent
                            {
                                Id = current.SceneEntry.Id,
                                ProgressEntity = current.Entity,
                                SceneCategory = current.SceneEntry.Category
                            });
                            _currentlyLoading.RemoveAt(i);
                        }

                        EntityManager.SetComponentData(current.Entity, new SceneLoadingProgress
                        {
                            PercentComplete = current.Operation.progress
                        });
                    }
                }

                if (!_unloadingInProgressQuery.IsEmptyIgnoreFilter)
                {
                    for (int i = _currentlyUnloading.Count - 1; i != -1; i--)
                    {
                        var current = _currentlyUnloading[i];
                        if (current.Operation.isDone)
                        {
                            _eventSystem.Enqueue(new SceneUnloadedEvent
                            {
                                Id = current.SceneEntry.Id,
                                ProgressEntity = current.Entity,
                                Category = current.SceneEntry.Category
                            });
                            _currentlyUnloading.RemoveAt(i);

                            if (current.SubsequentSceneId != SceneId.None)
                            {
                                _eventSystem.Enqueue(new RequestSceneLoadEvent
                                {
                                    Id = current.SubsequentSceneId
                                });
                            }
                        }

                        EntityManager.SetComponentData(current.Entity, new SceneUnloadingProgress
                        {
                            PercentComplete = current.Operation.progress
                        });
                    }
                }
            }

            public Entity CreateEntity<T>(T componentData = default) where T : struct, IComponentData
            {
                var entity = EntityManager.CreateEntity(ComponentType.ReadWrite<T>());
                EntityManager.SetComponentData(entity, componentData);
                return entity;
            }

        }
    }
}
