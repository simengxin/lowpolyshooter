using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class BulletProjectile : MonoBehaviour
    {
        public GameObject impactParticle;
        public LayerMask layerToHit;
        public float colliderRadius = 1f;
        [Range(0f, 1f)]
        public float collideOffset = 0.15f;
        //Rigidbody rig;
        [ReadOnly] public int damage = 10;
        [ReadOnly] public float speed = 10;
        public float bulletDefaultForce = 200;
        public float timeToLive = 2;

        [Header("---EXTRA---")]
        [ReadOnly] public bool canReflect = false;
        [ReadOnly] public int numberReflect = 3;
        int currentReflect = 0;

        [Header("---Explosion---")]
        public bool useExplosion = false;
        public float radius = 3;
        public LayerMask targetLayer;
        public AudioClip explosionSound;
        public AudioClip soundImpactOther;
        public GameObject explosionBlowFX;

        private void OnEnable()
        {
            Invoke("DisableBullet", timeToLive);
            //Reset the reflect number
            currentReflect = numberReflect;
        }

        public void DisableBullet()
        {
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        public void InitBullet(int dmg, float _speed, bool reflected, bool isExplosion = false, float _bulletForce = -1)
        {
            speed = _speed;
            canReflect = reflected;
            damage = dmg;
            useExplosion = isExplosion;
            if (_bulletForce != -1)
                bulletDefaultForce = _bulletForce;
        }

        private void Update()
        {
            RaycastHit hit;
            var checkPosition = transform.position;
            //Force z position to zero to fit with 2.5D mode
            checkPosition.z = 0;
            //Check target
            if (Physics.Raycast(checkPosition, transform.forward, out hit, speed * Time.deltaTime, layerToHit))
            {
                transform.position = hit.point;

                if (useExplosion)
                    DoExplosion();
                else
                {
                    //spawn the hit effet
                    if (impactParticle)
                        PoolingObjectHelper.GetTheObject(impactParticle, hit.point, true);

                    //Check deal damage to the target
                    var takeDamage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
                    if (takeDamage != null)
                    {
                        takeDamage.TakeDamage(damage, bulletDefaultForce, gameObject, hit.point);
                        gameObject.SetActive(false);
                    }
                    else if (canReflect && currentReflect > 0) //if no hit the target then check do reflect the bullet
                    {
                        currentReflect--;

                        Vector3 reflected = Vector3.Reflect(transform.forward, hit.normal);
                        Vector3 direction = transform.forward;
                        Vector3 vop = Vector3.ProjectOnPlane(reflected, Vector3.forward);

                        transform.forward = vop;
                        transform.rotation = Quaternion.LookRotation(vop, Vector3.forward);

                        SoundManager.PlaySfx(soundImpactOther);
                    }
                    else
                    {
                        gameObject.SetActive(false);        //disable the bullet if no use reflection
                        SoundManager.PlaySfx(soundImpactOther);
                    }
                }
            }
            else
            {
                transform.Translate(Vector3.forward * speed * Time.deltaTime);


                transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            }
        }

        void DoExplosion()
        {
            var hits = Physics.OverlapSphere(transform.position, radius, targetLayer);
            foreach (var hit in hits)
            {
                hit.gameObject.GetComponent<ICanTakeDamage>().TakeDamage(damage, bulletDefaultForce, gameObject, hit.transform.position);
            }

            SoundManager.PlaySfx(explosionSound);
            if(explosionBlowFX)
                PoolingObjectHelper.GetTheObject(explosionBlowFX, transform.position, true);
            gameObject.SetActive(false);
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, colliderRadius);
        }
    }
}