
/*
 * Control the menu in the gameplay scene
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
namespace PhoenixaStudio
{
    public class MenuManager : MonoBehaviour
    {
        public static MenuManager Instance;
        public GameObject uI, finish, pauseUI, LoadingUI;       //set the UI object to control hide/show

        [Header("===Health bar===")]
        public Slider healthBarSlider;

        public Text bulletRemainTxt, levelTxt;

        public GameObject handDirection;

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            uI.SetActive(true);         //set the ui panel to false at begin
            pauseUI.SetActive(false);
            LoadingUI.SetActive(false);
            handDirection.SetActive(false);
            levelTxt.text = SceneManager.GetActiveScene().name;
            if (Time.timeScale == 0)
                Time.timeScale = 1;     //prevent pause issue
        }

        private void Update()
        {
            //display health bar
            var currentHealthPercent = (float)GameManager.Instance.Player.currentHealth / (float)GameManager.Instance.Player.maxHealth;
            healthBarSlider.value = Mathf.Lerp(healthBarSlider.value, currentHealthPercent, 5 * Time.deltaTime);

            if(GameManager.Instance.Player && GameManager.Instance.Player.gunTypeID)
            bulletRemainTxt.text = GameManager.Instance.Player.gunTypeID.unlimitedBullet? "Bullet: -/-" : ( "Bullet: " + GameManager.Instance.Player.gunTypeID.bullet);
        }

        public void Finish()
        {
            //Disable the UI and call the Finish function after 2 seconds
            uI.SetActive(false);
            Invoke("FinishCo", 2);
        }

        void FinishCo()
        {
            finish.SetActive(true);
        }

        public void ShowUI(bool open)
        {
            uI.SetActive(open);
        }

        public void Restart()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void Pause(bool pause)
        {
            pauseUI.SetActive(pause);
            Time.timeScale = pause ? 0 : 1;

            SoundManager.Instance.PauseMusic(pause);
        }

        public void NextLevel()
        {
            if (GlobalValue.levelPlaying == -1)
                Home();
            else
            {
                GlobalValue.levelPlaying++;
                LoadingUI.SetActive(true);
                SceneManager.LoadSceneAsync("Level " + GlobalValue.levelPlaying);
            }
        }

        public void Home()
        {
            GlobalValue.levelPlaying = -1;
            LoadingUI.SetActive(true);
            SceneManager.LoadSceneAsync(0);
        }

        public void ShowHandDirection(float delay = 2)
        {
            SoundManager.PlaySfx(SoundManager.Instance.moveOn);
            handDirection.SetActive(true);
            Invoke("HideHandDirection", delay);
        }

        void HideHandDirection()
        {
            handDirection.SetActive(false);
        }
    }
}