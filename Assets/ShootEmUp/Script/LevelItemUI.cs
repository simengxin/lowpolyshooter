using UnityEngine;
using UnityEngine.UI;
namespace PhoenixaStudio
{
	public class LevelItemUI : MonoBehaviour
	{
		int levelNumber = 1;        //set the level id
		public Text TextLevel;		//place the text object
		public GameObject Locked;

		public GameObject backgroundNormal, backgroundInActive;

		void Start()
		{
			levelNumber = int.Parse(gameObject.name);       //get the level number from the object name
			TextLevel.text = levelNumber.ToString();

			backgroundNormal.SetActive(true);		
			backgroundInActive.SetActive(false);

			var levelReached = GlobalValue.LevelHighest;		//get the highest level

			if ((levelNumber <= levelReached))		//check if the level lower than the highest number, then allow the button work
			{
				
				Locked.SetActive(false);

				var openLevel = levelReached + 1 >= levelNumber;

				Locked.SetActive(!openLevel);

				bool isInActive = levelNumber == levelReached;

				backgroundNormal.SetActive(!isInActive);
				backgroundInActive.SetActive(isInActive);

				GetComponent<Button>().interactable = openLevel;
			}
			else
			{
				Locked.SetActive(true);
				GetComponent<Button>().interactable = false;
			}
		}

		public void LoadScene()
		{
			GlobalValue.levelPlaying = levelNumber;     //set the current level to the playing level
														//HomeMenu.Instance.LoadLevel();		//load the play scene
			HomeMenu.Instance.ShowCharacterSelector(true);
		}
	}
}