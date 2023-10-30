using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class BodyPartExplosion : MonoBehaviour
    {
        public float force = 1000;
        public float torque = 300;
        public float timeToLive = 3;

        void Start()
        {
            GetComponent<Rigidbody>().AddForce(new Vector3(Random.Range(-0.5f, 0.5f), 1, 0) * force);
            GetComponent<Rigidbody>().AddTorque(new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f)) * torque);

            Destroy(gameObject, timeToLive);
        }
    }
}