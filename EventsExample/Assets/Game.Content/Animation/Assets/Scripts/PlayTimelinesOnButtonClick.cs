using System.Linq;
using UnityEngine;
using UnityEngine.Timeline;

public class PlayTimelinesOnButtonClick : MonoBehaviour
{
    public TimelordMixer TimelordMixer;

    private void OnAwake()
    {
        if (TimelordMixer == null)
            TimelordMixer = GetComponent<TimelordMixer>();
    }

    private void OnEnable()
    {
        ButtonClickedSource.ButtonClickedEvent += ButtonClicked_ButtonClickedEvent;
    }

    private void OnDisable()
    {
        ButtonClickedSource.ButtonClickedEvent -= ButtonClicked_ButtonClickedEvent;
    }

    private void ButtonClicked_ButtonClickedEvent(TimelineAsset timeline)
    {
        //Debug.Log($"{timeline?.name} button was clicked");

        if (TimelordMixer == null)
            return;

        if (timeline != null)
        {
            TimelordMixer.Play(timeline);
        }
        else
        {
            var allTimelineAssets = TimelordMixer.Directors.Where(d => d != null).Select(t => t.PlayableDirector.playableAsset as TimelineAsset).ToList();
            TimelordMixer.Play(allTimelineAssets);
        }

        //ExecuteEvents.Execute<IButtonClickedHandler>(gameObject, null, (handler, data) => handler.OnButtonClicked());
    }
}

//public interface IButtonClickedHandler : IEventSystemHandler
//{
//    void OnButtonClicked();
//}