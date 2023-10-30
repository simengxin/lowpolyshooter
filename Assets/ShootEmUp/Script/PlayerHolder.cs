using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class PlayerHolder : MonoBehaviour
    {
        public static PlayerHolder Instance;
        public GameObject[] players;

        private void Start()
        {
            if (PlayerHolder.Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public GameObject GetPlayer()
        {
            return players[GlobalValue.pickedCharacterID - 1];
        }
    }
}