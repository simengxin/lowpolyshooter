using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class RagdollCharacter : MonoBehaviour
    {
        public Rigidbody appleForceBone;

        //public Vector3 defaultForce = new Vector3(200, 300, 0);

        public void Init(Vector3 direction, float force)
        {
            SetForce(direction * force);
        }

        public void SetForce(Vector3 force)
        {
            appleForceBone.AddForce(force, ForceMode.Impulse);
        }
    }
}