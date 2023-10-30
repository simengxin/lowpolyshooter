using UnityEngine;
namespace PhoenixaStudio
{
    public class GlobalValue : MonoBehaviour
    {
        public static int levelPlaying = -1;        //the playing level
        public static bool isSound = true;
        public static bool isMusic = true;

        public static int LevelHighest
        {
            get { return PlayerPrefs.GetInt("LevelHighest", 1); }
            set { PlayerPrefs.SetInt("LevelHighest", value); }
        }

        public static int pickedCharacterID
        {
            get { return PlayerPrefs.GetInt("pickedCharacterID", 1); }
            set { PlayerPrefs.SetInt("pickedCharacterID", value); }
        }
    }
}