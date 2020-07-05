using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public abstract class AnimationInputBase : MonoBehaviour
{
    protected List<Entity> m_RigEntities;

    public abstract Entity ActiveEntity { get; }

    public void RegisterEntity(Entity entity)
    {
        if (m_RigEntities == null)
            m_RigEntities = new List<Entity>();

        m_RigEntities.Add(entity);
    }
}

public abstract class AnimationInputBase<T> : AnimationInputBase where T : struct, ISampleData
{
    private int m_ActiveEntityIndex;

    public override Entity ActiveEntity => m_RigEntities?.Count > 0 ? m_RigEntities[m_ActiveEntityIndex] : Entity.Null;

    private void Awake()
    {
        var spawnerSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<RigSpawnerSystem>();
        spawnerSystem.RegisterInput(this);
    }

    private void Update()
    {
        if (m_RigEntities?.Count > 0)
        {
            // Select next rig entity
            if (Input.GetKeyDown(KeyCode.N))
                m_ActiveEntityIndex = (m_ActiveEntityIndex + 1) % m_RigEntities.Count;

            var currentEntity = m_RigEntities[m_ActiveEntityIndex];
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            if (!entityManager.HasComponent<T>(currentEntity))
                return;

            var data = entityManager.GetComponentData<T>(currentEntity);
            UpdateComponentData(ref data);
            entityManager.SetComponentData(currentEntity, data);
        }
    }

    protected abstract void UpdateComponentData(ref T data);
}