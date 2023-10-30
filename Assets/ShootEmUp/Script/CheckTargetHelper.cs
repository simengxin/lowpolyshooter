using UnityEngine;
namespace PhoenixaStudio
{
    public class CheckTargetHelper : MonoBehaviour
    {
        //set the target layer
        public LayerMask targetLayer;
        //place the check point, like the Eye position
        public Transform checkPoint;     
        public Vector2 size = new Vector2(5, 2);
        public Vector2 boxOffset;

        int dir = 1;

        public GameObject CheckTarget(int direction = 1)        //check the target, call by the owner
        {
            if (checkPoint == null)
                checkPoint = transform;

            dir = direction;

            GameObject hasTarget = null;
            boxOffset.x = dir * Mathf.Abs(boxOffset.x);
            var hits = Physics.OverlapBox(checkPoint.position + (Vector3)boxOffset, size * 0.5f, Quaternion.identity, targetLayer);
            if (hits.Length > 0)
                hasTarget = hits[0].gameObject;

            return hasTarget;
        }

        void OnDrawGizmos()
        {
            if (targetLayer == 0)
                return;

            if (!Application.isPlaying)
                dir = transform.rotation.y == 0 ? 1 : -1;

            Gizmos.color = Color.white;
            if (checkPoint == null)
                checkPoint = transform;

            boxOffset.x = dir * Mathf.Abs(boxOffset.x);
            Gizmos.DrawWireCube(checkPoint.position + (Vector3)boxOffset, size);
        }
    }
}