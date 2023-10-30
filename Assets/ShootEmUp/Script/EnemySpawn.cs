using UnityEngine;
namespace PhoenixaStudio
{
    [System.Serializable]
    public class EnemySpawn
    {
        public Transform[] spawnPos;
        public float wait = 3;      //delay for first enemy
        public GameObject[] enemy;    //enemy spawned
        public int numberEnemy = 5;     //the number of enemy need spawned
        public float rateMin = 1;  //time delay spawn next enemy
        public float rateMax = 2;
    }
}