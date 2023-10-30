using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class WaveTrigger : TriggerEvent
    {
        [Header("=== FINISH FLAG ===")]
        public bool finishGameAfterKillAll = false;

        [Header("=== Place the VCamera for this zone")]
        public bool useTheNewVCamera = false;
        public GameObject vCamera;

        [Header("=== Place the spawn position and the Enemy prefab in this object ===")]
        public bool beginOnStart = false;
        public EnemyWave enemyWave;

        bool isWorked = false;

        private void Awake()
        {
            //active the camera when the option is choosen
            if (vCamera)
                vCamera.SetActive(false);
        }

        public override void OnContactPlayer()
        {
            if (isWorked)
                return;

            isWorked = true;
            if (vCamera)
                vCamera.SetActive(useTheNewVCamera);
            SpawnEnemy();
        }

        void SpawnEnemy()
        {
            LevelEnemyManager.Instance.BeginWave(this, finishGameAfterKillAll);
        }

        public void FinishWave()
        {
            if (vCamera)
                vCamera.SetActive(false);
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawCube((Vector3)transform.position + GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
        }
    }
}