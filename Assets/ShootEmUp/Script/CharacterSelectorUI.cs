using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class CharacterSelectorUI : MonoBehaviour
    {
        public void Next()
        {
            SoundManager.Click();
            CharacterSelector.Instance.NextCharacter();
        }

        public void Previous()
        {
            SoundManager.Click();
            CharacterSelector.Instance.PreviousCharacter();
        }

        public void Close()
        {
            HomeMenu.Instance.ShowCharacterSelector(false);
        }

        public void Play()
        {
            SoundManager.Click();
            //save character ID
            CharacterSelector.Instance.SaveCharacterID();
            HomeMenu.Instance.LoadLevel();		//load the play scene
        }
    }
}