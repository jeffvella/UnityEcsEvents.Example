using Assets.Scripts.Components.Events;
using Assets.Scripts.UI;
using SubjectNerd.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class AudioManager : MonoBehaviour, IEventObserver<PlayAudioEvent>
{
    [Reorderable]
    public List<AudioEntry> Clips = new List<AudioEntry>();
    public EventRouter EventSource;

    [Serializable]
    public class AudioEntry
    {
        public SoundCategory Category;
        public AudioClip Clip;
    }
 
    private List<AudioPlayer> _active = new List<AudioPlayer>();
    private Stack<AudioPlayer> _pool = new Stack<AudioPlayer>();
    private List<AudioPlayer> _keepBuffer = new List<AudioPlayer>();
    private ILookup<SoundCategory, AudioEntry> _clipLookup;

    private void Start()
    {
        _clipLookup = Clips.ToLookup(k => k.Category, v => v);
        EventSource.AddListener<AudioManager, PlayAudioEvent>(this);
    }

    private void OnDestroy()
    {
        EventSource.RemoveListener<AudioManager, PlayAudioEvent>(this);
    }

    public class AudioPlayer
    {
        public float EndTime;
        public AudioSource Source;
        public Entity Entity;
    }

    public void OnEvent(PlayAudioEvent e)
    {
        if (e.Sound == SoundCategory.None)
        {
            Debug.LogError($"PlaySoundEvent detected with no sound specified");
        }

        if (_clipLookup.Contains(e.Sound))
        {
            var entry = _clipLookup[e.Sound].First();
            var entity = e.AssociatedEntity;

            if (!IsPlayingForEntity(entry.Clip, entity))
                PlayForEntity(entry.Clip, entity);
        }
    }

    private void PlayForEntity(AudioClip clip, Entity entity, float3? position = null)
    {
        AudioPlayer player = default;
        if (_pool.Count > 0)
            player = _pool.Pop();
        else
            player = CreatePlayer();

        player.EndTime = Time.time + clip.length;
        player.Entity = entity;

        if (position != null)
            player.Source.transform.position = position.Value;

        player.Source.clip = clip;
        player.Source.Play();
        player.Source.gameObject.name = $"{entity} {clip.name}";
        _active.Add(player);
    }

    private AudioPlayer CreatePlayer()
    {
        var go = new GameObject();
        go.transform.parent = transform;
        var source = go.AddComponent<AudioSource>();
        var player = new AudioPlayer { Source = source };
        return player;
    }

    private bool IsPlayingForEntity(AudioClip clip, Entity entity)
    {
        var isPlaying = false;
        foreach (AudioPlayer player in _active)
        {
            if (!player.Source.isPlaying)
            {
                _pool.Push(player);
            }
            else
            {
                if (player.Entity == entity && clip == player.Source.clip)
                    isPlaying = true;
              
                _keepBuffer.Add(player);
            }
        }

        var tmp = _active;
        _active = _keepBuffer;
        _keepBuffer = tmp;
        _keepBuffer.Clear();

        return isPlaying;
    }
}