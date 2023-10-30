using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class CharacterSelector : MonoBehaviour
    {
        public static CharacterSelector Instance;

        public GameObject[] Players;
        [ReadOnly] public int currentPos;

        private void Awake()
        {
            Instance = this;
            HideAllCharacter();
        }

        private void OnEnable()
        {
            currentPos = GlobalValue.pickedCharacterID - 1;
        }

        public void NextCharacter()
        {
            currentPos++;
            if (currentPos == Players.Length)
                currentPos = 0;

            ShowCharacter();
        }

        public void PreviousCharacter()
        {
            currentPos--;
            if (currentPos < 0)
                currentPos = Players.Length - 1;

            ShowCharacter();
        }

        public void ShowCharacter()
        {
            HideAllCharacter();
            Players[currentPos].SetActive(true);
        }

        public void HideAllCharacter()
        {
            foreach (var obj in Players)
            {
                obj.SetActive(false);
            }
        }

        public void SaveCharacterID()
        {
            GlobalValue.pickedCharacterID = Players[currentPos].GetComponent<CharacterID>().ID;
        }
    }
}