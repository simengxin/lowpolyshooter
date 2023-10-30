using System.Collections;
using UnityEngine;
namespace PhoenixaStudio
{
    [RequireComponent(typeof(BlinkingEffect))]
    public class MeleeEnemy : MonoBehaviour, ICanTakeDamage
    {
        [Header("--- SET UP ---")]
        public int maxHealth = 100;
        int currentHealth;
        public ParticleSystem smokeFX;

        [Header("=== RAGDOLL EFFECT ===")]
        public GameObject ragdollObj;

        [Header("=== PARACHUTE SETUP ===")]
        public bool useParachute = false;
        public GameObject parachuteObj;
        public float parachuteSpeed = 1;
        public float parachuteGravity = -2;

        public Animator anim;       //place the animator for the enemy
        public float moveSpeed = 2;     //set the move speed
        public float runSpeed = 4;      //set the run speed
        public float gravity = -9.8f;       //set the gravity for the character
        public enum MovingDirection { Right, Left}
        public MovingDirection movingDirection = MovingDirection.Left;

        int horizontalInput = -1;        //set the moving direction
        public LayerMask layerAsGround;     //set the ground layers
        public LayerMask layerAsWall;       //set the wall layers
        public AudioClip soundDie, soundDetectPlayer, soundHurt;
        [ReadOnly] public bool isGrounded = false;
        CharacterController characterController;
        [ReadOnly] public Vector2 velocity;
        bool isDead = false;

        public bool allowCheckGroundAhead = false;

        [Header("=== SHIELD OPTIONAL ===")]
        public ShieldObj shieldObj;
        public AnimatorOverrideController animatorWithShield;
        public AnimatorOverrideController animatorNoShield;

        [Header("*** PATROL ***")]
        public bool usePatrol = false;      //use patrol or not
        public float patrolWaitForNextPoint = 2f;       //reach to the A point, wait this time value before move to the next point
        [Range(-10, -1)]
        public float limitLocalLeft = -2;       //set the local pos X to the left
        [Range(1, 10)]
        public float limitLocalRight = 2;       //set the local pos X to the right
        [ReadOnly] public float limitLeft, limitRight;
        public float dismissPlayerDistance = 10;        //if player move over this distance, stop chasing the player
        public float dismissPlayerWhenStandSecond = 5;      //of this enemy stand over 5s, it also stop move

        public float checkDistance = 8;     //check target in this distance
        public LayerMask layerAsTarget;     //set the layer of the target
        [ReadOnly] public bool isAttacking = false;

        [ReadOnly] public bool isDetectPlayer;
        bool isWaiting = false;

        bool isFacingRight { get { return transform.forward.x > 0.5f; } }
        [ReadOnly] public float countingStanding = 0;
        protected EnemyMeleeAttack meleeAttack;
        BlinkingEffect blinkingEffect;

        private void Start()
        {
            if (anim == null)
                anim = GetComponent<Animator>();        //get the animatior if you don't place the animator manually

            if (shieldObj != null)
            {
                anim.runtimeAnimatorController = animatorWithShield;
            }

            horizontalInput = movingDirection == MovingDirection.Right ? 1 : -1;

            characterController = GetComponent<CharacterController>();      //get the component
            meleeAttack = GetComponent<EnemyMeleeAttack>();
            blinkingEffect = GetComponent<BlinkingEffect>();

            StartCoroutine(CheckTargetCo());

            limitLeft = transform.position.x + limitLocalLeft;      //get the world position of the patrol point
            limitRight = transform.position.x + limitLocalRight;
            isWaiting = false;

            if (!usePatrol)
                moveSpeed = 0;      //if no use patrol, set movespeed = 0

            currentHealth = maxHealth;
            if (useParachute)
                DetectPlayer();
        }

        IEnumerator CheckTargetCo()
        {
            //Check and detect the target
            //
            while (true)
            {
                while (isDetectPlayer || (GameManager.Instance.gameState != GameManager.GameState.Playing)) { yield return null; }

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
            isDetectPlayer = true;
        }


        //can call by Alarm action of other Enemy
        public virtual void DetectPlayer(float delayChase = 0)
        {
            if (isDetectPlayer)
                return;

            isDetectPlayer = true;
            SoundManager.PlaySfx(soundDetectPlayer);
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
            if (!isDetectPlayer)
                return;

            isWaiting = false;
            isDetectPlayer = false;
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
            Gizmos.DrawWireCube(transform.position + new Vector3(-checkDistance/2f, 1 ,0), new Vector3(checkDistance, 1, 0));
        }

        private void Update()
        {
            if (isDead)     //if enemy is dead, just update the animation status
            {
                HandleAnimation();
                return;
            }

            transform.forward = new Vector3(horizontalInput, 0, 0);     //move the character
            if (GameManager.Instance.gameState != GameManager.GameState.Playing || isAttacking)
                velocity.x = 0;
            else
                velocity.x = (isDetectPlayer ? runSpeed : moveSpeed) * horizontalInput;

            CheckGround();      //check the ground under the feet

            if (isGrounded && velocity.y < 0)
                velocity.y = 0;
            else
                velocity.y += gravity * Time.deltaTime;     //add gravity

            if (isWaiting)
                velocity.x = 0;

            if (isDetectPlayer && (Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) < 0.5f))
                velocity.x = 0;

            if (isDead)
                velocity = Vector2.zero;

            Vector2 finalVelocity = velocity;
            if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
                GetSlopeVelocity(ref finalVelocity);

            if (useParachute)
                finalVelocity = new Vector3((isFacingRight ? 1 : -1) * parachuteSpeed, parachuteGravity, 0);

            characterController.Move(finalVelocity * Time.deltaTime);       //move the character

            if (isDetectPlayer && velocity.x == 0)
            {
                countingStanding += Time.deltaTime;
                if (isDetectPlayer && countingStanding >= dismissPlayerWhenStandSecond)
                    DismissDetectPlayer();
            }
            else
                countingStanding = 0;

            HandleAnimation();      //handle the animation of the character

            if (isDetectPlayer)
            {
                //Check and flip the character to chasing the player
                if ((isFacingRight && transform.position.x > GameManager.Instance.Player.transform.position.x) || (!isFacingRight && transform.position.x < GameManager.Instance.Player.transform.position.x))
                {
                    //only flip if no use parachute or using parachute and the distance > 1
                    if (!useParachute || ( useParachute && Mathf.Abs(transform.position.x - GameManager.Instance.Player.transform.position.x) > 1))
                    {
                        Flip();
                        isWaiting = false;
                    }
                }

                if (isWallAHead() || (allowCheckGroundAhead && !isGroundedAhead()))     //if detect the wall ahead, stop moving
                {
                    isWaiting = true;
                }
                else
                    isWaiting = false;
            }
            else
            {
                if (!isDead && !isWaiting)
                {
                    if (isWallAHead())
                        StartCoroutine(ChangeDirectionCo());        //change the direction if the wall ahead
                    else if (usePatrol)
                    {
                        if ((velocity.x < 0 && transform.position.x < limitLeft)
                            || (velocity.x > 0 && transform.position.x > limitRight))
                            StartCoroutine(ChangeDirectionCo());        //if character reach the patrol position, do the change direction function
                    }

                    if (allowCheckGroundAhead && !isGroundedAhead())
                        StartCoroutine(ChangeDirectionCo());
                }
            }

            if (isDetectPlayer)
            {
                if (Vector2.Distance(transform.position, GameManager.Instance.Player.transform.position) > dismissPlayerDistance)
                    DismissDetectPlayer();
            }

            if (isDetectPlayer && GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                CheckAttack();      //check the attack if detected the player
            }

            //Check Shield
            if (shieldObj != null)
            {
                if (shieldObj.currentHealth <= 0)
                    anim.runtimeAnimatorController = animatorNoShield;
            }

            //Check and allow smoke effect work
            if (smokeFX)
            {
                var emission = smokeFX.emission;
                emission.enabled = isGrounded && (Mathf.Abs(velocity.x) > moveSpeed / 2) ? true : false;
            }
        }

        Vector3 hitNormal;
        public float dot;
        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            hitNormal = hit.normal;
            dot = Vector3.Dot(transform.forward, hitNormal);
        }

        IEnumerator ChangeDirectionCo()
        {
            if (isWaiting)      //if is in waiting status, break
                yield break;

            isWaiting = true;
            yield return new WaitForSeconds(patrolWaitForNextPoint);

            while (GameManager.Instance.gameState != GameManager.GameState.Playing) { yield return null; }

            Flip();     //flip the direction
            isWaiting = false;
        }

        RaycastHit groundHit;
        void CheckGround()          //check the ground
        {
            isGrounded = false;
            if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.9f, Vector3.down, out groundHit, 1f, layerAsGround))
            {
                float distance = transform.position.y - groundHit.point.y;
                if (distance <= (characterController.skinWidth + 0.01f))
                {
                    isGrounded = true;      //set the ground = true
                    if (useParachute)
                    {
                        parachuteObj.SetActive(false);
                        useParachute = false;
                    }
                }
            }
        }

        bool isGroundedAhead()      //check if there are ground ahead or not
        {
            var _isGroundAHead = Physics.Raycast(transform.position + Vector3.up * 0.5f + (isFacingRight ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down, 0.75f);
            Debug.DrawRay(transform.position + Vector3.up * 0.5f + (isFacingRight ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down * 1);
            return _isGroundAHead;
        }

        void GetSlopeVelocity(ref Vector2 vel)      //get the slope to allow the player move up or not
        {
            var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
            vel = vel.x * crossSlope;

            Debug.DrawRay(transform.position, crossSlope * 10);
        }

        void Flip()
        {
            if (isAttacking)
                return;

            horizontalInput *= -1;      //change the input direction, change the moving direction
        }

        bool isWallAHead()      //check if character hit the wall or not
        {
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
            anim.SetBool("isRunning", isDetectPlayer);
            anim.SetBool("parachute", useParachute);
        }

        public void Kill()      //when the character is killed
        {
            if (isDead)
                return;

            StopAllCoroutines();
            isDead = true;
            SoundManager.PlaySfx(soundDie);
            gameObject.layer = LayerMask.NameToLayer("TriggerPlayer");
            gameObject.AddComponent<Rigidbody>();

            Destroy(characterController);
            gameObject.AddComponent<Rigidbody>();
            GetComponent<Rigidbody>().isKinematic = true;
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
                    Vector3 _forceDirection = (transform.position + characterController.center ) - _hitPoint;

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
                DetectPlayer();     //if the character no dead, chasing the player
                blinkingEffect.DoBlinking();
            }
        }

        void CheckAttack()
        {
            if (meleeAttack.AllowAction())      //check if can attack player or not
            {
                if (meleeAttack.CheckPlayer(isFacingRight))     //check if detect player ahead
                {
                    meleeAttack.Action();
                    anim.SetTrigger("melee");       //trigger the attack animation
                    isAttacking = true;
                    CancelInvoke("NoAttacking");
                    Invoke("NoAttacking", 2);
                }
            }
        }

        void NoAttacking()
        {
            isAttacking = false;
        }
    }
}