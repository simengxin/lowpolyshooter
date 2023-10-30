using UnityEngine;
using System.Collections;
using UnityEngine.UI;
namespace PhoenixaStudio
{
    public class WorldControllerUI : MonoBehaviour
    {
        public RectTransform BlockLevel;        //place the block level object
        public int howManyBlocks = 3;       //how many maps
        public float step = 720f;       //set the distance to the previous map
        public int levelPerBlock = 10;      //how many level per block
        private float newPosX = 0;

        int currentPos = 0;     //current level block

        public Button btnNext, btnPre;      //place the button next and previous

        void OnEnable()
        {
            SetCurrentWorld(Mathf.Clamp((GlobalValue.LevelHighest / levelPerBlock) + 1, 0, howManyBlocks));     //show the current level by the block
        }

        void Start()
        {
            UpdateButtons();
        }

        void UpdateButtons()
        {
            btnNext.interactable = currentPos < howManyBlocks - 1;      //only allow the next button work when there are available block ahead
            btnPre.interactable = currentPos > 0;        //only allow the previous button work when there are available block in front
        }

        void OnDisable()
        {
            if (SoundManager.Instance)
                SoundManager.PlayMusic(SoundManager.Instance.musicsGame);       //play the game music again
        }

        public void SetCurrentWorld(int world)     
        {
            currentPos = (world - 1);
            newPosX = 0;
            newPosX -= step * (world - 1);
            newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

            SetMapPosition();
            UpdateButtons();
        }

        public void SetMapPosition()         //set the current block position
        {
            BlockLevel.anchoredPosition = new Vector2(newPosX, BlockLevel.anchoredPosition.y);
        }

        bool allowPressButton = true;
        public void Next()
        {
            if (allowPressButton)
            {
                StartCoroutine(NextCo());       //call the next function
            }
        }

        IEnumerator NextCo()
        {
            allowPressButton = false;       //prevent the player press the button again

            SoundManager.Click();

            if (newPosX != (-step * (howManyBlocks - 1)))
            {
                currentPos++;

                newPosX -= step;
                newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);     //limit the position X

            }
            else
            {
                allowPressButton = true;
                yield break;
            }
            yield return new WaitForSeconds(0.1f);
            SetMapPosition();
            UpdateButtons();
            allowPressButton = true;        //allow press button again
        }

        public void Pre()
        {
            if (allowPressButton)
            {
                StartCoroutine(PreCo());
            }
        }

        IEnumerator PreCo()
        {
            allowPressButton = false;       //prevent the player press the button again
            SoundManager.Click();
            if (newPosX != 0)
            {
                currentPos--;

                newPosX += step;
                newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);         //limit the position X


            }
            else
            {
                allowPressButton = true;
                yield break;
            }


            yield return new WaitForSeconds(0.1f);
            SetMapPosition();

            UpdateButtons();

            allowPressButton = true;             //allow press button again

        }

        public void UnlockAllLevels()       //unlock all level command
        {
            GlobalValue.LevelHighest = (GlobalValue.LevelHighest + 1000);
            UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            SoundManager.Click();
        }
    }
}