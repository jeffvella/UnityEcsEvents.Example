using Assets.Game.Scripts;
using Assets.Game.Scripts.Components.Events;
using UnityEngine;
using Vella.Events;

namespace Assets.Scripts.UI
{
    public class UIMainMenu : MonoBehaviour
    {
        public EventRouter EventSource;

        public void StartGame()
        {
            EventSource.FireEvent(new RequestSceneUnloadEvent
            {
                Id = SceneId.MainMenu,
                ThenLoad = SceneId.Scene1
            });
        }

        public void GoToMainMenu()
        {
            EventSource.FireEvent(new RequestSceneUnloadEvent
            {
                Id = SceneId.Scene1,
                ThenLoad = SceneId.MainMenu
            });
        }
    }
}