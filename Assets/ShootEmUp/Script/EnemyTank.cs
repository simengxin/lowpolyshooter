using System.Collections;
using UnityEngine;
namespace PhoenixaStudio
{
    [RequireComponent(typeof(CheckTargetHelper))]
    public class EnemyTank : MonoBehaviour, ICanTakeDamage
    {
        public Animator anim;

        public int health = 200;
        [ReadOnly] public bool allowMoving = false;
        public float speed = 2;
        public float moveToLocalPointX = -8f;
        Vector2 moveToTarget;
        public GameObject explosionFX;
        public AudioClip soundHit, soundDestroy;

        [Header("=== LOOK AT TARGET ===")]
        public Transform turretObj;
        public GameObject destroyedPuppet;
        public float lookAtSpeed = 5;

        [Header("---DAMAGE SETUP---")]
        public int normalDamage = 50;
        public Transform normalPoint;
        public BulletProjectile normalBullet;
        public int noralBulletSpeed = 6;
        public float normalGunRate = 2;
        [Range(1, 10)]
        public int normalNumberBulletsRound = 3;
        public float normalBulletRate2Bullets = 0.3f;
        public AudioClip normalSound;

        CheckTargetHelper checkTargetHelper;
        bool finishMoving = false;

        bool isWorking = false;
        BlinkingEffect blinkingEffect;

        private void Start()
        {
            checkTargetHelper = GetComponent<CheckTargetHelper>();
            blinkingEffect = GetComponent<BlinkingEffect>();
            moveToTarget = transform.position + Vector3.right * moveToLocalPointX;
            if (anim == null)
                anim = GetComponent<Animator>();
        }

        private void Update()
        {
            if (!finishMoving)
            {
                if (!allowMoving)
                {
                    if (checkTargetHelper.CheckTarget(transform.position.x > GameManager.Instance.Player.transform.position.x ? -1 : 1))
                    {
                        isWorking = true;
                        allowMoving = true;
                     
                        //GameManager.Instance.PauseCamera(true);
                    }
                }

                if (allowMoving)
                {
                    moveToTarget.y = transform.position.y;
                    transform.position = Vector2.MoveTowards(transform.position, moveToTarget, speed * Time.deltaTime);

                    if (Mathf.Abs(transform.position.x - moveToTarget.x) < 0.1f)
                    {
                        finishMoving = true;
                        allowMoving = false;
                        StartCoroutine(FireCo());
                    }
                }
            }
            //make the turret look at the player target
            var targetRotation = Quaternion.LookRotation(GameManager.Instance.Player.transform.position - turretObj.position);

            // Smoothly rotate towards the target point.
            turretObj.rotation = Quaternion.Slerp(turretObj.rotation, targetRotation, lookAtSpeed * Time.deltaTime);

            anim.SetBool("moving", allowMoving);
        }

        IEnumerator FireCo()
        {
            while (true)
            {
                for (int i = 0; i < normalNumberBulletsRound; i++)
                {
                    anim.SetTrigger("shoot");

                    var projectile = PoolingObjectHelper.GetTheObject(normalBullet.gameObject, normalPoint.position, false).GetComponent<BulletProjectile>();
                    projectile.transform.forward = normalPoint.transform.forward;

                    projectile.InitBullet(normalDamage, noralBulletSpeed, false, true);
                    projectile.gameObject.SetActive(true);
                    SoundManager.PlaySfx(normalSound);
                    yield return new WaitForSeconds(normalBulletRate2Bullets);
                }

                yield return new WaitForSeconds(normalGunRate);
            }
        }

        Vector3 targetDirection(Vector3 offset)
        {
            var lookAtPlayerDirection = (GameManager.Instance.Player.transform.position + offset) - turretObj.position;

            lookAtPlayerDirection.Normalize();
            return lookAtPlayerDirection;
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            if (!isWorking)
                return;

            health -= damage;
            if (blinkingEffect)
                blinkingEffect.DoBlinking();

            if (health <= 0)
            {
                Instantiate(explosionFX, transform.position, Quaternion.identity);
                //GameManager.Instance.PauseCamera(false);
                SoundManager.PlaySfx(soundDestroy);
                if (destroyedPuppet)
                    Instantiate(destroyedPuppet, transform.position, Quaternion.identity);
                StopAllCoroutines();
                Destroy(gameObject);
            }
            else
                SoundManager.PlaySfx(soundHit);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (Application.isPlaying)
            {
                Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.right * moveToLocalPointX * (transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1));
                Gizmos.DrawWireCube(transform.position + Vector3.up + Vector3.right * moveToLocalPointX * (transform.position.x > GameManager.Instance.Player.transform.position.x ? 1 : -1), Vector3.one * 0.2f);
            }
            else
            {
                Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.right * moveToLocalPointX);
                Gizmos.DrawWireCube(transform.position + Vector3.up + Vector3.right * moveToLocalPointX, Vector3.one * 0.2f);
            }
        }
    }
}