/*
 * Control the Menu, show Gameover, Finish, Pause
 * 
 */

using UnityEngine;
using UnityEngine.SceneManagement;
namespace PhoenixaStudio
{
    public class HomeMenu : MonoBehaviour
    {
        public static HomeMenu Instance;
        public GameObject UI;           //place the game ui object to control
        public GameObject LevelUI;
        public GameObject LoadingUI;
        public GameObject characterSelectorUI;

        public void Awake()         //init the object
        {
            Instance = this;
            UI.SetActive(true);         //disable the UI object at begin the game
            LevelUI.SetActive(false);   //disable the UI object at begin the game
            LoadingUI.SetActive(false); //disable the UI object at begin the game
            characterSelectorUI.SetActive(false);

            Time.timeScale = 1;
        }

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;      //no allow the screen auto turn off
            SoundManager.PlayGameMusic();       //start play the game music
        }

        public void ShowLevelUI(bool open)
        {
            SoundManager.Click();
            LevelUI.SetActive(open);        //open the level panel
            UI.SetActive(!open);
        }

        public void LoadLevel()
        {
            LoadingUI.SetActive(true);
            if (GlobalValue.levelPlaying == -1)
                SceneManager.LoadSceneAsync("Demo");
            else
                SceneManager.LoadSceneAsync("Level " + GlobalValue.levelPlaying);       //load the level scene with the level playing
        }

        public void LoadTestFeatureScene()
        {
            GlobalValue.levelPlaying = -1;
            LevelUI.SetActive(false);
            UI.SetActive(false);
            ShowCharacterSelector(true);
        }

        public void ShowCharacterSelector(bool show)
        {
            SoundManager.Click();
            characterSelectorUI.SetActive(show);
            UI.SetActive(!show);
            LevelUI.SetActive(false);

            if (show)
                CharacterSelector.Instance.ShowCharacter();
            else
                CharacterSelector.Instance.HideAllCharacter();
        }
    }
}