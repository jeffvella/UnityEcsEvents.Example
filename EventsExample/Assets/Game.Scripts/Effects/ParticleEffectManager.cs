using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Assets.Game.Scripts.Effects
{
    /// <summary>
    ///     Responsible for spawning/re-using effects.
    ///     <para>Requires that particle systems be prefabs with a <see cref="ParticleEffect" /> MonoBehavior.</para>
    ///     <para>Each <see cref="ParticleSystem" /> should have its 'StopAction' set to Callback and 'PlayOnAwake' enabled</para>
    /// </summary>
    public class ParticleEffectManager : MonoBehaviour
    {
        private PoolGroup<ParticleEffect> _poolsGroup;

        [Header("Setup")] public GameObject ParentContainer;

        public int StartingPoolSize = 5;

        private void Awake()
        {
            _poolsGroup = new PoolGroup<ParticleEffect> {ParentContainer = ParentContainer, StartingPoolSize = StartingPoolSize, OnSpawnedAction = OnSpawned};
        }

        //public ParticleEffect Spawn(ParticleEffect prefab, Transform parentContainer)
        //{
        //    var pool = _poolsGroup.GetPoolForPrefab(prefab.gameObject);
        //    var effect = pool.Spawn(parentContainer.position, parentContainer.rotation);
        //    effect.transform.parent = parentContainer;
        //    return effect;
        //}

        public ParticleEffect Spawn(ParticleEffect prefab, float3 position, Quaternion? rotation = default, float3? scale = default)
        {
            var pool = _poolsGroup.GetPoolForPrefab(prefab.gameObject);
            var effect = pool.Spawn(position, rotation ?? Quaternion.identity);
            if (scale.HasValue)
                effect.transform.localScale = scale.Value;
            return effect;
        }

        public IEnumerator WaitForEffect(ParticleEffect effect, int timeoutSeconds = 10)
        {
            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(timeoutSeconds);
            while (effect.IsSpawned && effect.IsPlaying)
            {
                if (DateTime.UtcNow > timeout)
                    break;

                yield return new WaitForSeconds(0.1f);
            }

            yield return null;
        }

        private void OnSpawned(ParticleEffect effect)
        {
            effect.Restart();
        }

        public void Clear()
        {
            _poolsGroup.Clear();
        }
    }
}