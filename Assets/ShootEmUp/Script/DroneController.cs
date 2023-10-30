using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class DroneController : MonoBehaviour, ICanTakeDamage
    {
        public Animator anim;
        public int health = 200;
        public GameObject destroyedPuppet;
        public GameObject explosionFX;
        public AudioClip soundHit, soundDestroy;

        [Header("=== CHASING PLAYER ===")]
        public float keepOffsetPlayerX = 6;
        public float movingSpeed = 5;
        public float movingLerp = 1;

        [Header("=== LOOK AT TARGET ===")]
        public Transform turretObj;
        public Vector3 lookAtOffset = new Vector3(0, 0.6f, 0);
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

        BlinkingEffect blinkingEffect;
        CheckTargetHelper checkTargetHelper;
        bool isLookingAtPlayer = false;

        // Start is called before the first frame update
        void Start()
        {
            blinkingEffect = GetComponent<BlinkingEffect>();
            checkTargetHelper = GetComponent<CheckTargetHelper>();

            if (anim == null)
                anim = GetComponent<Animator>();

            StartCoroutine(FireCo());
        }

        // Update is called once per frame
        void Update()
        {
            if (checkTargetHelper.CheckTarget(transform.position.x > GameManager.Instance.Player.transform.position.x ? -1 : 1))
            {
                //make the turret look at the player target
                //turretObj.transform.forward = targetDirection(Vector3.up * 0.5f);
                var targetRotation = Quaternion.LookRotation(GameManager.Instance.Player.transform.position + lookAtOffset - turretObj.position);

                // Smoothly rotate towards the target point.
                turretObj.rotation = Quaternion.Slerp(turretObj.rotation, targetRotation, lookAtSpeed * Time.deltaTime);

                isLookingAtPlayer = true;
            }
            else
                isLookingAtPlayer = false;

            if(Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) > keepOffsetPlayerX)
            {
                transform.position = Vector3.MoveTowards(transform.position, GameManager.Instance.Player.transform.position + Vector3.up * 5, movingSpeed * Time.deltaTime);
            }
        }

        IEnumerator FireCo()
        {
            while (true)
            {
                yield return new WaitForSeconds(normalGunRate);

                //wait until detect the player
                while (!isLookingAtPlayer) { yield return null; }

                for (int i = 0; i < normalNumberBulletsRound; i++)
                {
                    var projectile = PoolingObjectHelper.GetTheObject(normalBullet.gameObject, normalPoint.position, false).GetComponent<BulletProjectile>();
                    projectile.transform.forward = normalPoint.transform.forward;

                    projectile.InitBullet(normalDamage, noralBulletSpeed, false, false);
                    projectile.gameObject.SetActive(true);
                    SoundManager.PlaySfx(normalSound);
                    yield return new WaitForSeconds(normalBulletRate2Bullets);
                }
            }
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
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
                Destroy(gameObject);
            }
            else
                SoundManager.PlaySfx(soundHit);
        }
    }
}