// MIT LICENSE Copyright (c) 2020 Jeffrey Vella
// https://github.com/jeffvella/UnityEcsEvents.Example/blob/master/LICENSE

using System;
using System.Collections.Generic;
using Assets.Game.Scripts.Components;
using Assets.Game.Scripts.Components.Events;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.SceneManagement;
using Vella.Events;

namespace Assets.Game.Scripts
{
    public enum SceneCategory
    {
        None = 0,
        Level,
        Menu,
        Resource,
    }

    public enum SceneId
    {
        None = 0,
        Common,
        MainMenu,
        Scene1
    }

    /// <summary>
    /// Defines the <see cref="ISceneDatabase" />.
    /// </summary>
    public interface ISceneDatabase
    {
        bool IsSceneLoaded(SceneId id);

        bool TryGetScene(SceneId id, out RuntimeScene sceneEntry);
    }

    /// <summary>
    /// A scene wrapper merging compile and run time information.
    /// </summary>
    public class RuntimeScene
    {
        public SceneDefinition Definition { get; set; }

        public bool IsLoaded { get; set; }

        public LoadSceneMode Mode { get; set; }

        public Scene Scene { get; set; }
    }

    /// <summary>
    /// Defines the <see cref="SceneDatabase" />.
    /// </summary>
    public class SceneDatabase : ISceneDatabase
    {
        private readonly Dictionary<SceneId, RuntimeScene> _sceneById = new Dictionary<SceneId, RuntimeScene>();

        private readonly Dictionary<string, RuntimeScene> _sceneByPath = new Dictionary<string, RuntimeScene>();

        public SceneDatabase(List<SceneDefinition> definitions)
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
            SceneManager.sceneUnloaded += SceneManager_sceneUnloaded;
            var mode = SceneManager.sceneCount > 1 ? LoadSceneMode.Additive : LoadSceneMode.Single;

            foreach (var sceneDefinition in definitions)
            {
                string path = sceneDefinition.Asset.ScenePath;
                var scene = SceneManager.GetSceneByPath(path);
                var runtimeScene = new RuntimeScene
                {
                    Definition = sceneDefinition,
                    Scene = scene,
                    Mode = mode,
                    IsLoaded = scene != null && scene.isLoaded
                };

                _sceneById[sceneDefinition.Id] = runtimeScene;
                _sceneByPath[sceneDefinition.Asset.ScenePath] = runtimeScene;
            }
        }

        public bool IsSceneLoaded(SceneId id)
            => TryGetScene(id, out var runtimeScene) && runtimeScene.IsLoaded;

        public bool TryGetScene(SceneId id, out RuntimeScene scene)
            => _sceneById.TryGetValue(id, out scene);

        public bool TryGetScene(string path, out RuntimeScene scene)
            => _sceneByPath.TryGetValue(path, out scene);

        private void SceneManager_sceneLoaded(Scene loadedScene, LoadSceneMode mode)
        {
            if (TryGetScene(loadedScene.path, out var runtimeScene))
            {
                runtimeScene.Scene = loadedScene;
                runtimeScene.IsLoaded = true;
                runtimeScene.Mode = mode;
            }
        }

        private void SceneManager_sceneUnloaded(Scene unloadedScene)
        {
            if (TryGetScene(unloadedScene.path, out var runtimeScene))
            {
                runtimeScene.Scene = unloadedScene;
                runtimeScene.IsLoaded = false;
            }
        }
    }

    /// <summary>
    /// Static data/configuration for a scene.
    /// </summary>
    [Serializable]
    public class SceneDefinition
    {
        public SceneReference Asset;

        public SceneCategory Category;

        public SceneId Id;
    }

    /// <summary>
    /// Stores scene setup information from the Editor.
    /// </summary>
    public class SceneLoader : MonoBehaviour
    {
        [SubjectNerd.Utilities.Reorderable]
        public List<SceneId> AutoLoad;

        [SubjectNerd.Utilities.Reorderable]
        public List<SceneDefinition> Scenes;

        private void Start()
        {
            var sceneDatabase = new SceneDatabase(Scenes);
            var world = World.DefaultGameObjectInjectionWorld;
            var loadingSystem = world.GetOrCreateSystem<SceneLoadingSystem>();
            loadingSystem.Initialize(sceneDatabase);
            loadingSystem.LoadScenes(AutoLoad);
        }

        /// <summary>
        /// Controls loading of scenes from ECS events.
        /// </summary>
        [UpdateInGroup(typeof(InitializationSystemGroup))]
        public class SceneLoadingSystem : SystemBase
        {
            private List<AsyncOperationTracker> _currentlyLoading;

            private List<AsyncOperationTracker> _currentlyUnloading;

            private EntityEventSystem _eventSystem;

            private EntityQuery _loadingInProgressQuery;

            private EntityQuery _requestSceneLoadQuery;

            private EntityQuery _requestSceneUnloadQuery;

            private ISceneDatabase _sceneDatabase;

            private SceneLoadingQueue _sceneQueue;

            private EntityQuery _unloadingInProgressQuery;

            public event Action<SceneId> SceneLoaded;

            public event Action<SceneId> SceneUnloaded;

            public Entity CreateEntity<T>(T componentData = default) where T : struct, IComponentData
            {
                var entity = EntityManager.CreateEntity(ComponentType.ReadWrite<T>());
                EntityManager.SetComponentData(entity, componentData);
                return entity;
            }

            public void Initialize(ISceneDatabase sceneDatabase)
            {
                _sceneDatabase = sceneDatabase;
                _sceneQueue = new SceneLoadingQueue();
            }

            public void LoadScenes(IEnumerable<SceneId> scenes, bool async = false)
            {
                if (async)
                {
                    foreach (var sceneId in scenes)
                    {
                        _eventSystem.Enqueue(new RequestSceneLoadEvent
                        {
                            Id = sceneId
                        });
                    }
                }
                else
                {
                    foreach (var sceneId in scenes)
                    {
                        _sceneQueue.Pending.Enqueue(sceneId);
                    }

                    LoadNextScene();
                }
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
                    ComponentType.ReadWrite<SceneLoadedEvent>(),
                    ComponentType.ReadWrite<SceneUnloadedEvent>()
                }
                }));
            }

            protected override void OnUpdate()
            {
                ProcessUnloadRequests();
                ProcessLoadRequests();
                UpdateProgress();
                CleanUp();
                LoadNextScene();
            }

            private void CleanUp()
            {
                Entities.ForEach((in SceneLoadedEvent e) =>
                {
                    if (_sceneQueue.Current != null && _sceneQueue.Current.Definition.Id == e.Id)
                    {
                        _sceneQueue.Current = null;
                    }

                    UnityEngine.Debug.Log($"Scene '{e.Id}' was Loaded");
                    EntityManager.DestroyEntity(e.ProgressEntity);
                    SceneLoaded?.Invoke(e.Id);

                }).WithStructuralChanges().Run();

                Entities.ForEach((in SceneUnloadedEvent e) =>
                {
                    UnityEngine.Debug.Log($"Scene '{e.Id}' was Unloaded");
                    EntityManager.DestroyEntity(e.ProgressEntity);
                    SceneUnloaded?.Invoke(e.Id);

                }).WithStructuralChanges().Run();
            }

            private RuntimeScene GetSceneById(SceneId id)
            {
                if (id == SceneId.None)
                    throw new ArgumentException($"'Scene.{SceneId.None}' is not a valid argument");

                if (!_sceneDatabase.TryGetScene(id, out var scene))
                    throw new KeyNotFoundException($"Scene '{id}' was not found");

                return scene;
            }

            private void LoadNextScene()
            {
                if (_sceneQueue.Current == null)
                {
                    while (_sceneQueue.Pending.Count > 0)
                    {
                        var nextId = _sceneQueue.Pending.Dequeue();
                        if (_sceneDatabase.TryGetScene(nextId, out var runtimeScene) && !runtimeScene.IsLoaded)
                        {
                            _sceneQueue.Current = runtimeScene;
                            _eventSystem.Enqueue(new RequestSceneLoadEvent
                            {
                                Id = nextId
                            });
                            break;
                        }
                    }
                }
            }

            private void LoadScene(SceneDefinition sceneEntry)
            {
                UnityEngine.Debug.Log($"Loading Scene: '{sceneEntry.Id}'");

                var op = SceneManager.LoadSceneAsync(sceneEntry.Asset.ScenePath, LoadSceneMode.Additive);
                var entity = CreateEntity<SceneLoadingProgress>();

                _currentlyLoading.Add(new AsyncOperationTracker
                {
                    Entity = entity,
                    Operation = op,
                    SceneEntry = sceneEntry
                });
            }

            private void ProcessLoadRequests()
            {
                if (!_requestSceneLoadQuery.IsEmptyIgnoreFilter)
                {
                    var requests = _requestSceneLoadQuery.ToComponentDataArray<RequestSceneLoadEvent>(Allocator.TempJob);

                    for (int i = 0; i < requests.Length; i++)
                    {
                        var request = requests[i];
                        var scene = GetSceneById(request.Id);
                        if (scene.IsLoaded)
                        {
                            UnityEngine.Debug.LogError($"The scene '{request.Id}' is already loaded.");
                        }
                        else
                        {
                            LoadScene(scene.Definition);
                        }
                    }

                    requests.Dispose();
                }
            }

            private void ProcessUnloadRequests()
            {
                if (!_requestSceneUnloadQuery.IsEmptyIgnoreFilter)
                {
                    var requests = _requestSceneUnloadQuery.ToComponentDataArray<RequestSceneUnloadEvent>(Allocator.TempJob);
                    for (int i = 0; i < requests.Length; i++)
                    {
                        var request = requests[i];
                        var scene = GetSceneById(request.Id);
                        if (!scene.IsLoaded)
                        {
                            UnityEngine.Debug.LogError($"The scene '{request.Id}' can't be unloaded because it isn't loaded.");
                        }
                        else
                        {
                            RemoveFromLoadingTracker(scene.Definition);
                            UnloadScene(scene.Definition, request);
                        }
                    }

                    requests.Dispose();
                }
            }

            private void RemoveFromLoadingTracker(SceneDefinition sceneEntry)
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

            private void UnloadScene(SceneDefinition sceneEntry, RequestSceneUnloadEvent request)
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

            /// <summary>
            /// Keeps track of all information related to an ongoing scene load.
            /// </summary>
            public struct AsyncOperationTracker
            {
                public Entity Entity;

                public AsyncOperation Operation;

                public SceneDefinition SceneEntry;

                public SceneId SubsequentSceneId;
            }

            /// <summary>
            /// A collection of scenes to be loaded sequentially.
            /// </summary>
            public class SceneLoadingQueue
            {
                public RuntimeScene Current;

                public Queue<SceneId> Pending = new Queue<SceneId>();
            }
        }
    }
}
