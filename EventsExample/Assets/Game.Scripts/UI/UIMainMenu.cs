using Assets.Game.Scripts;
using Assets.Game.Scripts.Components.Events;
using Assets.Scripts.Components;
using Assets.Scripts.Components.Events;
using Assets.Scripts.Systems;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

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