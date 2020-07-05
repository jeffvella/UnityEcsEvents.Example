using UnityEngine;
using UnityEngine.Timeline;

public class ButtonClickedSource : MonoBehaviour
{
    public delegate void ButtonClickedDelegate(TimelineAsset timeline);

    public TimelineAsset Timeline;
    public static event ButtonClickedDelegate ButtonClickedEvent;

    public void OnClick()
    {
        ButtonClickedEvent?.Invoke(Timeline);
    }
}