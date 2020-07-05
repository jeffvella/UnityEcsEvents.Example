using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Game.Scripts.Effects
{
    public class ParticleEffect : MonoBehaviour, IPoolable<ParticleEffect>
    {
        private readonly List<ParticleSystem> _systems = new List<ParticleSystem>();
        private bool _isPlayOnAwake;
        private IObjectPool<ParticleEffect> _pool;

        public int DespawnDelay;

        public bool IsSpawned => _pool != null;

        public bool IsValid => _pool != null;

        public bool IsPlaying
        {
            get
            {
                for (var i = 0; i < _systems.Count; i++)
                    if (_systems[i].isPlaying)
                        return true;
                return false;
            }
        }

        public void OnSpawned(IObjectPool<ParticleEffect> pool)
        {
            _pool = pool;
        }

        public void OnDespawned()
        {
            _pool = null;
        }

        public void Despawn()
        {
            // Move particle somewhere safe to prevent it being destroyed 
            // if parent becomes inactive/destroyed
            transform.parent = _pool?.ParentContainer;

            if (DespawnDelay > 0)
            {
                SetEmission(false);
                StartCoroutine(DespawnAfterDelay(DespawnDelay));
            }
            else
            {
                _pool?.Despawn(this);
            }
        }

        private void Awake()
        {
            var childSystems = GetComponentsInChildren<ParticleSystem>();
            if (childSystems.Any())
                _systems.AddRange(childSystems);
            _isPlayOnAwake = IsPlayOnAwake();
        }

        private bool IsPlayOnAwake()
        {
            foreach (var system in _systems)
                if (system.main.simulationSpeed > 0)
                    return true;
            return false;
        }

        public void OnParticleSystemStopped()
        {
            Despawn();
        }

        public void Reset()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                var system = _systems[i];
                system.time = 0;
                system.Clear();
                SetEmission(system, true);
            }
        }

        public void Play()
        {
            for (var i = 0; i < _systems.Count; i++)
                _systems[i].Play();
        }

        public void Stop()
        {
            for (var i = 0; i < _systems.Count; i++)
                _systems[i].Stop();
        }

        private void Pause()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                var main = _systems[i].main;
                main.simulationSpeed = 0;
            }
        }

        private void Unpause()
        {
            for (var i = 0; i < _systems.Count; i++)
            {
                var main = _systems[i].main;
                main.simulationSpeed = 1;
            }
        }

        public void Restart()
        {
            Reset();

            if (!_isPlayOnAwake)
                Play();
        }

        public void SetEmission(bool value)
        {
            for (var i = 0; i < _systems.Count; i++)
                SetEmission(_systems[i], value);
        }

        private void OnDestroy()
        {
            //_pool.Despawn(this);
        }

        public void SetEmission(ParticleSystem system, bool value)
        {
            var emission = system.emission;
            emission.enabled = value;
        }

        public IEnumerator DespawnAfterDelay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            _pool?.Despawn(this);
        }
    }
}