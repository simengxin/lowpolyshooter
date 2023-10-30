using UnityEngine;
namespace PhoenixaStudio
{
    public enum ControllerType { PC, Mobile }
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public enum GameState { Waiting, Playing, GameOver, Finish }        //the status of the game
        public ControllerType controllerType = ControllerType.PC;
        [HideInInspector] public GameState gameState;
        [ReadOnly] public PlayerController playerController;        //store the player
        //define player reborn event, it will called all registered objects
        public delegate void OnPlayerReborn();
        public static event OnPlayerReborn playerRebornEvent;    

        GameObject clonePlayer;     //store the clone player to able active it asap player die
        public bool playerRespawned { get; set; }

        public PlayerController Player      //get and set the Player
        {
            get
            {
                if (playerController != null)
                    return playerController;
                else
                {
                    playerController = FindObjectOfType<PlayerController>();        //if there are no player, try to find it on the scene
                    var startPoint = playerController.transform.position;

                    if (PlayerHolder.Instance)
                    {
                        Destroy(playerController.gameObject);
                        playerController = Instantiate(PlayerHolder.Instance.GetPlayer(), startPoint, Quaternion.identity).GetComponent<PlayerController>();
                    }
                    else
                    {
                        clonePlayer = Instantiate(playerController.gameObject);
                        clonePlayer.SetActive(false);
                    }
                    if (playerController)
                        return playerController;
                    else
                        return null;
                }
            }
        }

        public void SetGameState(GameState state)       //set the game state
        {
            gameState = state;
        }

        [ReadOnly] public Vector3 checkPoint;
        public void SetCheckPoint(Vector3 pos)
        {
            checkPoint = pos;       //save the check point to allow player respawn at this point after die
        }

        public void Awake()
        {
            Application.targetFrameRate = 60;       //set the game target frame rate

            if (GameSetup.Instance)
                controllerType = GameSetup.Instance.controllerType;

            Instance = this;
            var _startPoint = GameObject.Find("Startpoint");        //find the start point position
            if (_startPoint)
            {
                Player.gameObject.transform.position = _startPoint.transform.position;
                SetCheckPoint(_startPoint.transform.position);
            }
        }

        private void Start()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;      //no allow the screen turn off automatically
            gameState = GameState.Playing;
            SoundManager.PlayGameMusic();

            if (checkPoint == Vector3.zero)
                checkPoint = Player.transform.position;
        }

        public void GameOver()      //game over event called by Enemy and some other object
        {
            if (gameState == GameState.GameOver)
                return;
            SoundManager.Instance.PauseMusic(true);     //pause the game music
            Time.timeScale = 1;
            gameState = GameState.GameOver;
            if (MenuManager.Instance)
                MenuManager.Instance.ShowUI(false);

            Invoke("Continue", 3);      //allow continue game after 3 seconds

        }

        public void FinishGame()
        {
            if (gameState == GameState.Finish)
                return;

            gameState = GameState.Finish;

            if (GlobalValue.levelPlaying >= GlobalValue.LevelHighest)
            {
                GlobalValue.LevelHighest++;     //check and save the game level, if current level >= the highest level
            }

            MenuManager.Instance.Finish();      //call finish event to show

            SoundManager.PlaySfx(SoundManager.Instance.soundGamefinish);
        }

        public void Continue()
        {
            SoundManager.Instance.PauseMusic(false);

            Invoke("SpawnPlayer", 0.1f);
        }

        void SpawnPlayer()      //spawn the player to continue the game
        {
            playerRespawned = true;
            Destroy(playerController.gameObject);

            if (PlayerHolder.Instance)
                playerController = Instantiate(PlayerHolder.Instance.GetPlayer(), checkPoint, Quaternion.identity).GetComponent<PlayerController>();
            else
            {
                playerController = Instantiate(clonePlayer, checkPoint, Quaternion.identity).GetComponent<PlayerController>();
                playerController.gameObject.SetActive(true);
            }

            gameState = GameState.Playing;
            if (MenuManager.Instance)
            MenuManager.Instance.ShowUI(true);

            if (playerRebornEvent != null)
                playerRebornEvent();        //call the reborn event for Player
        }
    }
}