using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Systems;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIScoreUpdater : MonoBehaviour, IEventObserver<ScoreUpdatedEvent>
    {
        public EventRouter EventSource;
        public Text ScoreText;

        private void Start()
        {
            EventSource.AddListener<UIScoreUpdater, ScoreUpdatedEvent>(this);
        }

        public void OnEvent(ScoreUpdatedEvent e)
        {
            Debug.Log($"{nameof(ScoreUpdatedEvent)}! Score={e.CurrentScore} Change={e.ChangedAmount} Type={e.Type}");

            ScoreText.text = e.CurrentScore.ToString();
        }

    }

}