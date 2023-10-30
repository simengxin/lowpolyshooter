using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class GameSetup : MonoBehaviour
    {
        public static GameSetup Instance;
        public ControllerType controllerType = ControllerType.PC;
        public int targetFramRate = 60;

        private void Start()
        {
            if (GameSetup.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            Application.targetFrameRate = targetFramRate;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            DontDestroyOnLoad(gameObject);
        }
    }
}