using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class LevelEnemyManager : MonoBehaviour
    {
        public static LevelEnemyManager Instance;
        EnemyWave enemyWave;

        List<GameObject> listEnemySpawned = new List<GameObject>();
        WaveTrigger curretnWaveTrigger;
        bool finishGameAfterKillAll = false;

        private void Awake()
        {
            Instance = this;
        }

        public void BeginWave(WaveTrigger wave, bool _finishGameAfterKillAll)
        {
            StopAllCoroutines();
            //reset the list
            listEnemySpawned.Clear();
            //setup the wave
            curretnWaveTrigger = wave;
            enemyWave = wave.enemyWave;
            finishGameAfterKillAll = _finishGameAfterKillAll;

            StartCoroutine(SpawnEnemyCo());
        }

        IEnumerator SpawnEnemyCo()
        {
            yield return new WaitForSeconds(enemyWave.wait);

            while (GameManager.Instance.gameState != GameManager.GameState.Playing)
                yield return null;

            for (int j = 0; j < enemyWave.enemySpawns.Length; j++)
            {
                var enemySpawn = enemyWave.enemySpawns[j];
                yield return new WaitForSeconds(enemySpawn.wait);
                for (int k = 0; k < enemySpawn.numberEnemy; k++)
                {
                    while (GameManager.Instance.gameState != GameManager.GameState.Playing)
                        yield return null;

                    var spawnPos = enemySpawn.spawnPos[Random.Range(0, enemySpawn.spawnPos.Length)].position;
                    GameObject _temp = Instantiate(enemySpawn.enemy[Random.Range(0, enemySpawn.enemy.Length)], spawnPos, Quaternion.identity) as GameObject;

                    _temp.SetActive(false);

                    yield return new WaitForSeconds(0.1f);

                    _temp.SetActive(true);
                    //Try to make enemy detect the player
                    _temp.SendMessage("DetectPlayer", SendMessageOptions.DontRequireReceiver);
                    listEnemySpawned.Add(_temp);

                    yield return new WaitForSeconds(Random.Range(enemySpawn.rateMin, enemySpawn.rateMax));

                }
            }

            //check all enemy killed
            while (isEnemyAlive()) { yield return new WaitForSeconds(0.1f); }

            yield return new WaitForSeconds(0.5f);

            if (finishGameAfterKillAll)
            {
                GameManager.Instance.FinishGame();
            }
            else
            {
                curretnWaveTrigger.FinishWave();
                MenuManager.Instance.ShowHandDirection();
            }
        }

        bool isEnemyAlive()
        {
            for (int i = 0; i < listEnemySpawned.Count; i++)
            {
                if (listEnemySpawned[i].gameObject != null && listEnemySpawned[i].activeInHierarchy)
                    return true;
            }

            return false;
        }
    }

    [System.Serializable]
    public class EnemyWave
    {
        public float wait = 3;
        public EnemySpawn[] enemySpawns;
    }
}