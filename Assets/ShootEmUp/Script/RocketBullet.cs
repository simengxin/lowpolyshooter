using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class RocketBullet : MonoBehaviour, ICanTakeDamage
    {
        public float moveSpeed = 2;
        public float lookAtTargetSpeed = 1;
        public LayerMask targetLayer;
        public float offsetTargetY = 1.2f;

        bool lockedTarget = false;
        Transform target;

        [Header("Explosion Damage")]
        public AudioClip soundDestroy;
        public GameObject[] DestroyFX;
        public LayerMask collisionLayer;
        public int makeDamage = 100;
        public float force = 500;
        public float radius = 3;

        bool isWorked = false;

        private IEnumerator Start()
        {
            yield return new WaitForSeconds(0.3f);
            lockedTarget = true;
        }

        private void Update()
        {
            if(target == null)
                target = GameManager.Instance.Player.transform;

            if (lockedTarget)
            {
                Quaternion lookOnLook =
                Quaternion.LookRotation(target.transform.position - transform.position + Vector3.up * offsetTargetY);

                transform.rotation =
                Quaternion.Slerp(transform.rotation, lookOnLook, lookAtTargetSpeed * Time.deltaTime);



                //stop look at player if the distance lower than 2;
                if (Vector2.Distance(transform.position, target.position + Vector3.up * offsetTargetY) < 3)
                {
                    lockedTarget = false;
                    Invoke("AutoDestroy", 3);
                }
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, 1, collisionLayer))
            {
                transform.position = hit.point;
                    DoExplosion(hit.point);
            }

            transform.Translate(0, 0, moveSpeed * Time.deltaTime, Space.Self);
        }

        //if stop chasing the taregt, auto destroy it after time
        void AutoDestroy()
        {
            DoExplosion(transform.position);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isWorked)
                return;

            if(other.gameObject == GameManager.Instance.Player.gameObject)
            {
                DoExplosion(GameManager.Instance.Player.transform.position);
            }
        }

        public void DoExplosion(Vector3 _pos)
        {
            isWorked = true;
            var hits = Physics.OverlapSphere(_pos, radius, collisionLayer);
            if (hits == null)
                return;

            foreach (var hit in hits)
            {
                var damage = (ICanTakeDamage)hit.GetComponent<Collider>().gameObject.GetComponent(typeof(ICanTakeDamage));
                if (damage == null)
                    continue;

                damage.TakeDamage(makeDamage, force, gameObject, transform.position);
            }

            foreach (var fx in DestroyFX)
            {
                if (fx)
                {
                    Instantiate(fx, transform.position + Vector3.up, fx.transform.rotation);
                }
            }

            SoundManager.PlaySfx(soundDestroy);
            Destroy(gameObject);
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            DoExplosion(hitPoint);
        }
    }
}