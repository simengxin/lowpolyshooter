using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class Grenade : MonoBehaviour
    {
        public bool onlyBlowWhenContactCollision;

        public float delayBlowUp = 0.7f;
        [Header("Explosion Damage")]
        public AudioClip soundDestroy, soundHitGround;
        public GameObject[] DestroyFX;

        public LayerMask collisionLayer, groundLayer;
        public int makeDamage = 100;
        public float force = 500;
        public float radius = 3;
        // Use this for initialization
        bool isBlowingUp = false;

        Collider _collider;
        [ReadOnly] public float collideWithTheGroundUnderPosY = 1000;

        public void Init(int _damage, float _radius, bool blowImmediately = false, bool blowOnContactCollision = false, float _collideWithTheGroundUnderPosY = -1)
        {
            makeDamage = _damage;
            radius = _radius;
            onlyBlowWhenContactCollision = blowOnContactCollision;

            GetComponent<Collider>().enabled = false;
            if (_collideWithTheGroundUnderPosY != -1)
                collideWithTheGroundUnderPosY = _collideWithTheGroundUnderPosY;
            else
                collideWithTheGroundUnderPosY = 1000;

            if (blowImmediately)
            {
                DoExplosion();
            }
        }

        void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        private void Update()
        {
            if (!_collider.enabled)
            {
                if (transform.position.y < collideWithTheGroundUnderPosY)
                    _collider.enabled = true;
            }
        }

        IEnumerator OnCollisionEnter(Collision collision)
        {
            if (isBlowingUp)
                yield break;

            var hits = Physics.OverlapSphere(transform.position, 0.2f, groundLayer);
            if (hits == null)
                yield break;

            isBlowingUp = true;
            float delayCounter = 0;

            if (!onlyBlowWhenContactCollision)
            {
                while (delayCounter < delayBlowUp)
                {
                    delayCounter += Time.deltaTime;
                    yield return null;
                }
            }

            SoundManager.PlaySfx(soundHitGround);
          
            DoExplosion();
        }

        public void DoExplosion()
        {
            var hits = Physics.OverlapSphere(transform.position, radius, collisionLayer);
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
                    Instantiate(fx, transform.position, fx.transform.rotation);
                }
            }


            SoundManager.PlaySfx(soundDestroy);
            Destroy(gameObject);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}