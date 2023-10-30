using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    [RequireComponent(typeof(EnemyThrowAttack))]
    [RequireComponent(typeof(BlinkingEffect))]
    public class ThrowGrenadeEnemy : MonoBehaviour, ICanTakeDamage
    {
        [Header("--- SET UP ---")]
        public int maxHealth = 100;
        int currentHealth;
        [Header("=== RAGDOLL EFFECT ===")]
        public GameObject ragdollObj;

        EnemyThrowAttack throwAttack;
        public float delayThrowToSyncAnim = 0.5f;

        public Animator anim;       //place the animator
        public float gravity = -9.8f;       //set the gravity
        public int horizontalInput = -1;        //set the moving direction
        public LayerMask layerAsGround;     //set the Ground, Wall layer
        public AudioClip soundHurt, soundDie;
        [ReadOnly] public bool isGrounded = false;
        CharacterController characterController;
        [ReadOnly] public Vector2 velocity;
        bool isDead = false;

        [ReadOnly] public bool isDetectPlayer;
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
            throwAttack = GetComponent<EnemyThrowAttack>();
            blinkingEffect = GetComponent<BlinkingEffect>();
        }

        private void Update()
        {
            if (isDead)
            {
                HandleAnimation();
                return;
            }
            //Move the character
            transform.forward = new Vector3(horizontalInput, 0, 0);
            velocity.x = 0;
            //Check the ground to able run or stop
            CheckGround();

            if (isGrounded && velocity.y < 0)
                velocity.y = 0;
            else
                velocity.y += gravity * Time.deltaTime;     //add gravity

            if (isDead)
                velocity = Vector2.zero;

            Vector2 finalVelocity = velocity;       //get the final speed

            //Move the character with the final speed
            characterController.Move(finalVelocity * Time.deltaTime);
            HandleAnimation();

            CheckAttack();
        }

        void CheckAttack()
        {
            if (throwAttack.AllowAction())
            {
                if (throwAttack.CheckPlayer())
                {
                    throwAttack.Action();
                    anim.SetTrigger("throw");
                    Invoke("ThrowCo", delayThrowToSyncAnim);
                }
            }
        }

        void ThrowCo()
        {
            throwAttack.Throw(isFacingRight, Vector2.zero);
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
                blinkingEffect.DoBlinking();
            }
        }
    }
}