using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class Checkpoint : MonoBehaviour
    {
        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = Color.blue;
            Gizmos.DrawCube((Vector3)transform.position + GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
        }
    }
}