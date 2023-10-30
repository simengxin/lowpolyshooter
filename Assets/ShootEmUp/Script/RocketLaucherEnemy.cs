using System.Collections;
using UnityEngine;
namespace PhoenixaStudio
{
    [RequireComponent(typeof(CheckTargetHelper))]
    public class RocketLaucherEnemy : MonoBehaviour, ICanTakeDamage
    {
        public Animator anim;

        public int health = 200;
        [ReadOnly] public bool allowMoving = false;
        public float speed = 2;
        public float moveToLocalPointX = -8f;
        Vector2 moveToTarget;
        public GameObject explosionFX;
        public AudioClip soundHit, soundDestroy;
        public GameObject destroyedPuppet;

        [Header("---DAMAGE SETUP---")]
        public GameObject rocketObj;
        public Transform normalPoint;
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

            anim.SetBool("moving", allowMoving);
        }

        IEnumerator FireCo()
        {
            while (true)
            {
                for (int i = 0; i < normalNumberBulletsRound; i++)
                {
                    anim.SetTrigger("shoot");

                    //var projectile = PoolingObjectHelper.GetTheObject(normalBullet.gameObject, normalPoint.position, false).GetComponent<BulletProjectile>();
                    var projectile = Instantiate(rocketObj, normalPoint.position, Quaternion.identity);
                    projectile.transform.forward = normalPoint.transform.forward;

                    //projectile.InitBullet(normalDamage, noralBulletSpeed, false, true);
                    //projectile.gameObject.SetActive(true);
                    SoundManager.PlaySfx(normalSound);
                    yield return new WaitForSeconds(normalBulletRate2Bullets);
                }

                yield return new WaitForSeconds(normalGunRate);
            }
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
                Instantiate(explosionFX, transform.position + Vector3.up, Quaternion.identity);
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
