using Assets.Scripts.Components.Events;
using UnityEngine;
using UnityEngine.UI;
using Vella.Events;

namespace Assets.Scripts.UI
{
    public class UIScoreUpdater : MonoBehaviour, IEventObserver<ScoreUpdatedEvent>
    {
        public EventRouter EventSource;
        public Text ScoreText;

        public void OnEvent(ScoreUpdatedEvent e)
        {
            Debug.Log($"{nameof(ScoreUpdatedEvent)}! Score={e.CurrentScore} Change={e.ChangedAmount} Type={e.Type}");

            ScoreText.text = e.CurrentScore.ToString();
        }

        private void Start() => EventSource.Subscribe<ScoreUpdatedEvent>(this);

        private void OnDestroy() => EventSource.Unsubscribe<ScoreUpdatedEvent>(this);
    }
}
