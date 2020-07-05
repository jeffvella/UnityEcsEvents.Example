using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

public class PlayRandomTimeline : MonoBehaviour
{
    private TimeSpan _idleDuration;
    private DateTime _idleStartedTime = DateTime.MinValue;
    private bool _isIdle;
    private bool _isTimelinePlaying;
    private List<TimelineAsset> _timelineAssets = new List<TimelineAsset>();
    public TimelordMixer TimelordMixer;

    private void Start()
    {
        if (TimelordMixer == null)
            TimelordMixer = GetComponent<TimelordMixer>();

        _timelineAssets = TimelordMixer?.Directors.Where(d => d != null).Select(t => t.PlayableDirector.playableAsset as TimelineAsset).ToList();
    }

    private void Update()
    {
        if (TimelordMixer == null)
            return;

        if (TimelordMixer.IsTimelinePlaying)
        {
            _isIdle = false;
            return;
        }

        if (!_isIdle)
        {
            _idleStartedTime = DateTime.UtcNow;
            _idleDuration = TimeSpan.FromSeconds(Random.Range(1, 4));
            _isIdle = true;
        }

        if (_isIdle && DateTime.UtcNow.Subtract(_idleStartedTime) > _idleDuration)
        {
            var randomTimeline = _timelineAssets.ElementAtOrDefault(Random.Range(0, _timelineAssets.Count - 1));
            if (randomTimeline != null)
                TimelordMixer.Play(randomTimeline);
        }
    }
}