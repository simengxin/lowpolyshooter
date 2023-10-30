using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
namespace PhoenixaStudio
{
    public class CinemaCameraController : MonoBehaviour
    {
        CinemachineVirtualCamera cameraVirtual;    // Start is called before the first frame update
        IEnumerator Start()
        {
            while(GameManager.Instance.Player == null) { yield return null; }

            cameraVirtual = GetComponent<CinemachineVirtualCamera>();
            cameraVirtual.Follow = GameManager.Instance.Player.transform;

            GameManager.playerRebornEvent += GameManager_playerRebornEvent;
        }

        private void GameManager_playerRebornEvent()
        {
            cameraVirtual.Follow = GameManager.Instance.Player.transform;
        }
    }
}
