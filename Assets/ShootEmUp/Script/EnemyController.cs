using System.Collections;
using UnityEngine;
namespace PhoenixaStudio
{
    [RequireComponent(typeof(BlinkingEffect))]
    public class EnemyController: MonoBehaviour, ICanTakeDamage
    {
        [Header("--- SET UP ---")]
        public int maxHealth = 100;
        int currentHealth;
        public ParticleSystem smokeFX;
        [Header("=== RAGDOLL EFFECT ===")]
        public GameObject ragdollObj;

        public Animator anim;       //place the animator
        public float moveSpeed = 2;     //set the moving speed
        public float runSpeed = 4;
        public float gravity = -9.8f;       //set the gravity
        public int horizontalInput = -1;        //set the moving direction
        public LayerMask layerAsGround;     //set the Ground, Wall layer
        public LayerMask layerAsWall;       //the layer as wall
        public AudioClip soundHurt, soundDie;
        [ReadOnly] public bool isGrounded = false;
        CharacterController characterController;
        [ReadOnly] public Vector2 velocity;
        bool isDead = false;

        public bool allowCheckGroundAhead = false;

        [Header("*** PATROL ***")]
        public bool usePatrol = false;      //allow patrol or not
        public float patrolWaitForNextPoint = 2f;       //wait this time value before change the direction
        [Range(-10, -1)]
        public float limitLocalLeft = -2;       //set the left/ right position for patrol
        [Range(1, 10)]
        public float limitLocalRight = 2;       
        [ReadOnly] public float limitLeft, limitRight;
        [ReadOnly] public float countingStanding = 0;
        float dismissPlayerWhenStandSecond = 5;      //of this enemy stand over 5s, it also stop move

        [Header("+++ FIRE BULLET +++")]
        public bool allowFireBullet = false;        //allow fire the bullet or not
        public GameObject bulletObj;
        public int bulletDamage = 30;
        public float bulletSpeed = 30;
        public bool bulletReflected = false;
        public float fireRate = 3;      //the delay time between 2 shoots
        public float checkDistance = 8;     //the distance allow shoot the target
        public Transform firePosition;      //set the fire position
        public LayerMask layerAsTarget;     //set the target layer
        public AudioClip soundShoot;
        float lastShoot = -999;
        [ReadOnly] public bool isAttacking = false;

        public GameObject muzzleFX;

        [Header("=== Gun Recoil Effect===")]
         public AnimationCurve recoildCurve;
         public float recoilDuration = 0.15f;
         public float recoilMaxRotation = 10;
        public Transform[] recoilAffectedBones;
        private float recoilTimer;

        [ReadOnly] public bool isDetectPlayer;
        [ReadOnly] public bool chasingPlayer = false;
        bool isWaiting = false;

        bool isFacingRight { get { return transform.forward.x > 0.5f; } }
        BlinkingEffect blinkingEffect;

        private void Start()
        {
            currentHealth = maxHealth;
            //if the animator is not placed manually, try to find it
            if (anim == null)
                anim = GetComponent<Animator>();
            //Get the Component
            characterController = GetComponent<CharacterController>();
            blinkingEffect = GetComponent<BlinkingEffect>();
            if (allowFireBullet)
                StartCoroutine(CheckAndShootTarget());
            //Caculating the world positons
            limitLeft = transform.position.x + limitLocalLeft;
            limitRight = transform.position.x + limitLocalRight;
            if(muzzleFX)
            muzzleFX.SetActive(false);
            isWaiting = false;
            //If no patrol set, the move speed will be zero
            if (!usePatrol)
                moveSpeed = 0;

            StartCoroutine(CheckTargetCo());
        }

        IEnumerator CheckAndShootTarget()       //auto check and shoot the target
        {
            while (true)
            {
                while (GameManager.Instance.gameState != GameManager.GameState.Playing) { yield return null; }

                //Check the target in range to make the shoot
                RaycastHit hit;
   
                if (Physics.CapsuleCast(transform.position + Vector3.up * characterController.height * 0.5f, transform.position + Vector3.up * (characterController.height - characterController.radius),
               characterController.radius, horizontalInput > 0 ? Vector3.right : Vector3.left, out hit, checkDistance, layerAsTarget))
                {
                    isDetectPlayer = true;
                    chasingPlayer = true;
                    //If the time allow to shoot
                    if (Time.time > (lastShoot + fireRate))
                    {
                        recoilTimer = Time.time;

                        var projectile = PoolingObjectHelper.GetTheObject(bulletObj, firePosition.position, false);
                        projectile.transform.forward = firePosition.forward;

                        projectile.gameObject.SetActive(true);
                        projectile.GetComponent<BulletProjectile>().InitBullet(
                            bulletDamage, bulletSpeed, bulletReflected);

                        muzzleFX.SetActive(true);
                        SoundManager.PlaySfx(soundShoot);
                        anim.SetTrigger("shoot");
                        lastShoot = Time.time;

                        ////check if nothing block the bullet line then shoot the target
                        //RaycastHit hitObject;
                        //Physics.Linecast(new Vector3(transform.position.x, firePosition.position.y, 0), hit.point, out hitObject);
                        //if (hitObject.collider == null || (hitObject.collider.gameObject.CompareTag("Checkpoint")))
                        //{
                        //    SoundManager.PlaySfx(soundShoot);
                        //    anim.SetTrigger("shoot");
                        //    muzzleFX.SetActive(true);

                        //    //Player get shoot and die
                        //    GameManager.Instance.Player.GetShoot();
                        //    yield return new WaitForSeconds(0.1f);

                        //    lastShoot = Time.time;
                        //}
                        //else
                        //    isDetectPlayer = false;
                    }
                }
                else
                    isDetectPlayer = false;

                yield return null;
            }
        }

        private void Update()
        {
            if (isDead)
            {
                //handle the animation even this character is dead
                HandleAnimation();
                return;
            }
            //Move the character
            transform.forward = new Vector3(horizontalInput, 0, 0);
            //If detect the player, chasing him
            if (isDetectPlayer || GameManager.Instance.gameState != GameManager.GameState.Playing)
                velocity.x = 0;
            else
                velocity.x = (chasingPlayer ? runSpeed : moveSpeed) * horizontalInput;
            //Check the ground to able run or stop
            CheckGround();

            if (isGrounded && velocity.y < 0)
                velocity.y = 0;
            else
                velocity.y += gravity * Time.deltaTime;     //add gravity

            if (isWaiting)
                velocity.x = 0;

            if (chasingPlayer && (Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) < 0.5f))
                velocity.x = 0;

            if (isDead)
                velocity = Vector2.zero;

            Vector2 finalVelocity = velocity;       //get the final speed
            if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
                GetSlopeVelocity(ref finalVelocity);

            //Move the character with the final speed
            characterController.Move(finalVelocity * Time.deltaTime);

            if (chasingPlayer && velocity.x == 0)
            {
                countingStanding += Time.deltaTime;
                if (isDetectPlayer && countingStanding >= dismissPlayerWhenStandSecond)
                    DismissDetectPlayer();
            }
            else
                countingStanding = 0;

            HandleAnimation();

            if (chasingPlayer)
            {
                //Check and flip the character to chasing the player
                if ((isFacingRight && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight && transform.position.x < GameManager.Instance.Player.transform.position.x))
                {
                    Flip();
                    isWaiting = false;
                }

                if (isWallAHead() || (allowCheckGroundAhead && !isGroundedAhead()))     //if detect the wall ahead, stop moving
                {
                    isWaiting = true;
                }
                else
                    isWaiting = false;
            }

            //if is not dead, do the patrol and detect the player
            else
            {
                if (!isDead && !isWaiting)
                {
                    if (isWallAHead())
                        StartCoroutine(ChangeDirectionCo());
                    else if (usePatrol)
                    {
                        if ((velocity.x < 0 && transform.position.x < limitLeft)
                            || (velocity.x > 0 && transform.position.x > limitRight))
                            StartCoroutine(ChangeDirectionCo());
                    }

                    if (allowCheckGroundAhead && !isGroundedAhead())
                        StartCoroutine(ChangeDirectionCo());
                }
            }

            //Check and allow smoke effect work
            if (smokeFX)
            {
                var emission = smokeFX.emission;
                emission.enabled = isGrounded && (Mathf.Abs(velocity.x) > moveSpeed / 2) ? true : false;
            }
        }

        private void LateUpdate()
        {
            if (recoilTimer < 0)
                return;

            float curveTime = (Time.time - recoilTimer) / recoilDuration;
            if (curveTime > 1f)
            {
                recoilTimer = -1;
            }
            else
            {
                foreach (var bone in recoilAffectedBones)
                {
                    bone.Rotate(Vector3.right, recoildCurve.Evaluate(curveTime) * recoilMaxRotation, Space.Self);
                }
            }
        }

        IEnumerator CheckTargetCo()
        {
            //Check and detect the target
            //
            while (true)
            {
                while (chasingPlayer || (GameManager.Instance.gameState != GameManager.GameState.Playing)) { yield return null; }

                RaycastHit hit;
                if (Physics.CapsuleCast(transform.position + Vector3.up * characterController.height * 0.5f, transform.position + Vector3.up * (characterController.height - characterController.radius),
               characterController.radius, horizontalInput > 0 ? Vector3.right : Vector3.left, out hit, checkDistance, layerAsTarget))
                {
                    DetectPlayer(0.5f);
                }

                yield return null;
            }
        }

        //Called by the LevelEnemyManager script
        public void DetectPlayer()
        {
            chasingPlayer = true;
        }


        //can call by Alarm action of other Enemy
        public virtual void DetectPlayer(float delayChase = 0)
        {
            if (chasingPlayer)
                return;

            chasingPlayer = true;
            StartCoroutine(DelayBeforeChasePlayer(delayChase));     //call detect player function
        }

        protected IEnumerator DelayBeforeChasePlayer(float delay)       //wait the delay time before chasing to the player
        {
            isWaiting = true;
            yield return new WaitForSeconds(delay);

            isWaiting = false;
        }

        public virtual void DismissDetectPlayer()       //stop chasing the player
        {
            if (!chasingPlayer)
                return;

            isWaiting = false;
            chasingPlayer = false;
        }

        IEnumerator ChangeDirectionCo()
        {
            //Only can change the direction if not in the waiting state
            if (isWaiting)
                yield break;

            isWaiting = true;
            yield return new WaitForSeconds(patrolWaitForNextPoint);

            while (GameManager.Instance.gameState != GameManager.GameState.Playing) { yield return null; }
            //Change the moving direction
            Flip();
            isWaiting = false;
        }

        RaycastHit groundHit;
        void CheckGround()
        {
            //Check if the ground under feet to move or stop
            isGrounded = false;
            if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.9f, Vector3.down, out groundHit, 1f, layerAsGround))
            {
                float distance = transform.position.y - groundHit.point.y;
                if (distance <= (characterController.skinWidth + 0.01f))
                    isGrounded = true;
            }
        }

        bool isGroundedAhead()
        {
            //Check the ground ahead
            var _isGroundAHead = Physics.Raycast(transform.position + Vector3.up * 0.5f + (isFacingRight ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down, 1);
            Debug.DrawRay(transform.position + Vector3.up * 0.5f + (isFacingRight ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down * 1);
            return _isGroundAHead;
        }

        void GetSlopeVelocity(ref Vector2 vel)
        {
            //Get the slope angle to adjust the velocity X
            var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
            vel = vel.x * crossSlope;

            Debug.DrawRay(transform.position, crossSlope * 10);
        }

        void Flip()
        {
            //Change the direction of moving
            horizontalInput *= -1;
        }

        bool isWallAHead()
        {
            //Check if the wall ahead to stop moving
            if (Physics.CapsuleCast(transform.position + Vector3.up * characterController.height * 0.5f, transform.position + Vector3.up * (characterController.height - characterController.radius),
                characterController.radius, horizontalInput > 0 ? Vector3.right : Vector3.left, 1f, layerAsWall))
            {
                return true;
            }
            else
                return false;
        }

        void HandleAnimation()
        {
            anim.SetFloat("speed", Mathf.Abs(velocity.x));
            anim.SetBool("isDead", isDead);
        }

        public void Kill()
        {
            if (isDead)
                return;

            //Stop all functions
            StopAllCoroutines();
            isDead = true;
            SoundManager.PlaySfx(soundDie);
            gameObject.layer = LayerMask.NameToLayer("TriggerPlayer");
            gameObject.AddComponent<Rigidbody>();
            //Destroy the character controller to avoid the issue
            Destroy(characterController);
            gameObject.AddComponent<Rigidbody>();
            GetComponent<Rigidbody>().isKinematic = true;
            //Destroy the character after 5 seconds
            Destroy(gameObject, 5);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Deadzone")
            {
                Kill();
            }
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            if (isDead)
                return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                if (ragdollObj)
                {
                    isDead = true;
                    gameObject.SetActive(false);
                    var _hitPoint = hitPoint;
                    _hitPoint.z = Random.Range(-0.5f, 0.5f);
                    Vector3 _forceDirection = (transform.position + characterController.center) - _hitPoint;

                    _forceDirection.Normalize();
                    Instantiate(ragdollObj, transform.position, transform.rotation).gameObject.GetComponent<RagdollCharacter>().Init(_forceDirection, force);
                }
                else
                    Kill();
            }
            else
            {
                anim.SetTrigger("hurt");
                SoundManager.PlaySfx(soundHurt);
                if(blinkingEffect)
                blinkingEffect.DoBlinking();
            }
        }

        private void OnDrawGizmos()
        {
            if (usePatrol)
            {
                if (Application.isPlaying)
                {
                    var lPos = transform.position;
                    lPos.x = limitLeft;
                    var rPos = transform.position;
                    rPos.x = limitRight;
                    Gizmos.DrawWireCube(lPos, Vector3.one * 0.2f);
                    Gizmos.DrawWireCube(rPos, Vector3.one * 0.2f);
                    Gizmos.DrawLine(lPos, rPos);
                }
                else
                {
                    Gizmos.DrawWireCube(transform.position + Vector3.right * limitLocalLeft, Vector3.one * 0.2f);
                    Gizmos.DrawWireCube(transform.position + Vector3.right * limitLocalRight, Vector3.one * 0.2f);
                    Gizmos.DrawLine(transform.position + Vector3.right * limitLocalLeft, transform.position + Vector3.right * limitLocalRight);
                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position + new Vector3(-checkDistance / 2f, 1, 0), new Vector3(checkDistance, 1, 0));
        }
    }
}