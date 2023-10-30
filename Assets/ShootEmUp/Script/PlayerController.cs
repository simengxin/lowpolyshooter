using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
namespace PhoenixaStudio
{
    public enum PlayerState { Ground, Jetpack, Windy }
    [System.Serializable]
    public class PlayerParameter
    {
        //set the move speed of the player
        public float moveSpeed = 4;
        //the max and min jump height 
        public float maxJumpHeight = 3;
        public float minJumpHeight = 1;
        //set the gravity for the player
        public float gravity = -35f;
    }

    public class PlayerController : MonoBehaviour, ICanTakeDamage
    {
        [Header("=== SETUP ===")]
        public Animator anim;
        public int maxHealth = 100;
        [HideInInspector] public int currentHealth;
        public ParticleSystem smokeFX;
        public GameObject smokeLandingFX;

        [Header("Setup parameter on ground")]
        public PlayerParameter GroundParameter;     //Ground parameters

        [Header("=== RIG SETUP BONE ===")]
        public Rig rigAim;
        public MultiAimConstraint rightHandRig;
        public TwoBoneIKConstraint leftHandRig;
        public float rigAimSmooth = 0.1f;

        [Header("=== RAGDOLL EFFECT ===")]
        public GameObject ragdollObj;
        [ReadOnly] public List<GunItemObject> gunTypeIDList;
        public Transform animTarget;
        [ReadOnly] public bool isFiring = false;

        [HideInInspector] public float gravity = -35f;
        [HideInInspector] public PlayerState PlayerState = PlayerState.Ground;        //set what state the player in

        [Header("---SETUP LAYERMASK---")]
        public LayerMask layerAsGround;         //set the ground, wall layers for player

        [Header("---AIR JUMP OPTION---")]
        [Range(0, 2)]
        public int numberOfAirJump = 0;         //set the max jump on air for the player
        [HideInInspector] public int numberAirJumpCounter = 0;

        public CharacterController characterController { get; set; }
        [HideInInspector] public Vector2 velocity;     //temp value
        [HideInInspector] public Vector2 input;
        [HideInInspector] public Vector2 inputLastTime = Vector2.right;
        [HideInInspector] public bool isGrounded = false;
        bool isPlaying = true;
        [HideInInspector] public bool isDead = false;

        float velocityXSmoothing;
        public float accelerationTimeAirborne = .2f;
        public float accelerationTimeGroundedRun = .3f;
        public float accelerationTimeGroundedSliding = 1f;

        [Header("---AUDIO---")]
        public AudioClip soundFootStep;
        [Range(0f, 1f)]
        public float soundFootStepVolume = 0.5f;
        public AudioClip soundJump;
     
        [Range(0f, 1f)]
        public float soundJumpVolume = 0.2f;
        public AudioClip soundHit, soundDie, soundLanding;

        public bool isInJumpZone { get; set; }
        public float accGrounedOverride { get; set; }

        private float moveSpeed;        //the moving speed, changed evey time the player on ground or in water
        private float maxJumpHeight;
        private float minJumpHeight;

        public bool isFacingRight { get { return inputLastTime.x > 0; } }
        float lastGroundPos;

        protected PlayerParameter overrideZoneParameter = null; //null mean no override
        protected bool useOverrideParameter = false;
        PlayerOverrideParametersChecker playerOverrideParametersChecker;
        public void SetOverrideParameter(PlayerParameter _override, bool isUsed, PlayerState _zone = PlayerState.Ground)
        {
            //override the player parameter on Ground, Wind zone or Water,
            overrideZoneParameter = _override;
            useOverrideParameter = isUsed;
            PlayerState = _zone;
        }

        bool isUsingPatachute = false;

        public void SetParachute(bool useParachute)
        {
            isUsingPatachute = useParachute;
        }

        public void ExitZoneEvent()
        {
            //when exit the other zone, set the state to the ground again
            PlayerState = PlayerState.Ground;
            if (isUsingPatachute)
                SetParachute(false);
        }

        public bool forcePlayerStanding { get; set; }
        public void ForcePlayerStanding(bool stop)
        {
            //force player stading immediately
            forcePlayerStanding = stop;
            if (stop)
                input = Vector2.zero;
        }

        bool forceIdle = false;
        public void ForceIdle(float time)
        {
            //force player stop immediately with the certain time
            StartCoroutine(ForceIdleCo(time));
        }

        IEnumerator ForceIdleCo(float time)
        {
            //set the force state on and off after the time
            forceIdle = true;
            yield return new WaitForSeconds(time);
            forceIdle = false;
        }

        BlinkingEffect blinkingEffect;
        //No take damage when player begin or respawn
        float noTakeDamageOnBeginTime = 2;

        private void Awake()
        {
             animTarget.parent = null;
        }

        [ReadOnly] public GunTypeID gunTypeID;

        void Start()
        {
            currentHealth = maxHealth;
            //Init the value
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);
            characterController = GetComponent<CharacterController>();
            if (anim == null)
                anim = GetComponent<Animator>();
            transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);
            originalCharHeight = characterController.height;
            originalCharCenterY = characterController.center.y;

            if (jetpackObj)
            {
                jetpackObj.SetActive(false);

                //init the jetpack sound for player
                jetpackAScr = jetpackObj.AddComponent<AudioSource>();
                jetpackAScr.clip = jetpackSound;
                jetpackAScr.volume = 0;
                jetpackAScr.loop = true;
            }
            //get the player components
            rangeAttack = GetComponent<RangeAttack>();
            playerOverrideParametersChecker = GetComponent<PlayerOverrideParametersChecker>();
            //setup the curent zone
            SetupParameter();
            //find all gun object in the player
            gunTypeIDList = new List<GunItemObject>(gameObject.GetComponentsInChildren<GunItemObject>(true));

            gunTypeID = GunManager.Instance.getGunID();
            SetGun(gunTypeID);
            GunManager.Instance.ResetGunBullet();
            blinkingEffect = GetComponent<BlinkingEffect>();
            if (blinkingEffect && GameManager.Instance.playerRespawned)
                blinkingEffect.DoBlinking(noTakeDamageOnBeginTime);
        }

        public void SetupParameter()
        {
            //setup the player value with the current zone
            PlayerParameter _tempParameter;

            switch (PlayerState)
            {
                case PlayerState.Ground:
                    _tempParameter = GroundParameter;
                    break;
                default:
                    _tempParameter = GroundParameter;
                    break;
            }

            if (useOverrideParameter)
                _tempParameter = overrideZoneParameter;

            moveSpeed = _tempParameter.moveSpeed;
            maxJumpHeight = _tempParameter.maxJumpHeight;
            minJumpHeight = _tempParameter.minJumpHeight;
            gravity = _tempParameter.gravity;
        }

        public void animSetSpeed(float value)       //set the animation speed with the value 
        {
            if (anim)
                anim.speed = value;
        }

        void SetCheckPoint(Vector3 pos)
        {
            //store the checkpoint for respawn
            RaycastHit hit;
            if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 100, layerAsGround))
            {
                GameManager.Instance.SetCheckPoint(hit.point);
            }
        }

        bool playLandingEffect = false;

        void Update()
        {
            if (isLoading)
            {
                rightHandRig.weight = 0;
                leftHandRig.weight = 0;
            }
            else
            {
                rightHandRig.weight = Mathf.Lerp(rightHandRig.weight, 1, rigAimSmooth * Time.deltaTime * 5);
                leftHandRig.weight = Mathf.Lerp(leftHandRig.weight, 1, rigAimSmooth * Time.deltaTime * 5);
            }

            //counting no damage at begin/respawn
            if (noTakeDamageOnBeginTime > 0)
                noTakeDamageOnBeginTime -= Time.deltaTime;

            //Aim target check
            if (!isDead)
            {
                rigAim.weight = Mathf.Lerp(rigAim.weight, 1, rigAimSmooth * Time.deltaTime * 5);
                Vector3 position = ControllerInput.Instance.GetAimPosition();

                position.z = -Camera.main.transform.position.z;
                //Debug.LogError(position);
                animTarget.position = Camera.main.ScreenToWorldPoint(position);
            }

            //Check to shoot
            if (isFiring)
            {
                //keep shooting
                RangeAttack();
            }


            //stop the playing if the game is not in the playing
            if (GameManager.Instance.gameState != GameManager.GameState.Playing)
            {
                //set velocity to zero to stop move
                velocity.x = 0;
                velocity.y += gravity * Time.deltaTime;

                input.x = 0;
                //move the character, meaning just move the Y axis
                characterController.Move(velocity * Time.deltaTime);
                CheckGround();
                if (isGrounded && velocity.y < 0)
                    velocity.y = 0;

                //still handle the animation
                HandleAnimation();
                return;
            }

            //move player forward
            transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);

            float targetX = 0;
            if (IgnoreControllerInput())
                targetX = 0;
            else
                targetX = moveSpeed;     //if no any action, just check and set the speed if player running or waking

            //get the target velocity X
            float targetVelocityX = input.x * targetX;

            if (isSliding || forcePlayerStanding)
                targetVelocityX = 0;

            if (forceStandingRemain > 0)
            {
                targetVelocityX = 0;
                forceStandingRemain -= Time.deltaTime;
            }

            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, isGrounded ?
                ((isSliding ? accelerationTimeGroundedSliding : accelerationTimeGroundedRun)) : accelerationTimeAirborne);

            //check the ground
            CheckGround();

            //if player contact to the deadzone, kill him
            if (isGrounded && groundHit.collider.gameObject.tag == "Deadzone")
                HitAndDie();

            //if player using jetpack, calucating the jetpack time remain
            if (!isDead && isUsingJetpack)
            {
                jetpackRemainTime -= Time.deltaTime;
                jetpackRemainTime = Mathf.Max(0, jetpackRemainTime);
                if (jetpackRemainTime > 0)
                    velocity.y += jetForce * Time.deltaTime;
                else
                {
                    ActiveJetpack(false);
                }
            }

            if (isGrounded)
                lastGroundPos = transform.position.y;

            //If player standing on ground and the velocity Y < 0, try to reset some value
            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0;
                isIgnorePlayerMovenmentInput = false;

                if (playLandingEffect)
                {
                    playLandingEffect = false;

                    if (smokeLandingFX)
                        Instantiate(smokeLandingFX, transform.position, smokeLandingFX.transform.rotation);
                }

                if (isFallingFromHeight)
                {
                    isFallingFromHeight = false;
                    SoundManager.PlaySfx(soundLanding, 0.5f);
                    ForceIdle(hardFallingIdleTime);
                }
            }
            else
            {
                velocity.y += gravity * Time.deltaTime;     //add gravity if player is in the normal state
            }

            //if player ion the windy zone, set the velocity Y base on the Parachute or freedom
            if (PlayerState == PlayerState.Windy)
            {
                if (isUsingPatachute)
                {
                    if (velocity.y < playerOverrideParametersChecker.currentZone.forceVeritcalWithParachute)
                    {
                        velocity.y = playerOverrideParametersChecker.currentZone.forceVeritcalWithParachute;
                    }
                }
                else
                {
                    if (velocity.y < playerOverrideParametersChecker.currentZone.forceVertical)
                    {
                        velocity.y = playerOverrideParametersChecker.currentZone.forceVertical;
                    }
                }
            }

            //Stop move if player is dead or falling
            if (!isPlaying || isDead)
                velocity.x = 0;

            //get the final velocity
            Vector2 finalVelocity = velocity + new Vector2(extraForceSpeed, 0);
            if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
                GetSlopeVelocity(ref finalVelocity);

            //Make the player move with the final velocity
            characterController.Move(finalVelocity * Time.deltaTime);

            if (isJetpackActived)
                UpdateJetPackStatus();


            HandleAnimation();

            if (isGrounded)
            {
                isInJumpZone = false;
                CheckStandOnEvent();
            }

            if (isGrounded || input.x == 0)
                UpdatePositionOnMovableObject(groundHit.transform);

            //Check and allow smoke effect work
            var emission = smokeFX.emission;
            emission.enabled = isGrounded && (Mathf.Abs(velocity.x) > moveSpeed / 2) ? true : false;
            //smokeFX.emission = emission;
        }

        private void LateUpdate()
        {
            if (isDead)
                return;

            var finalPos = new Vector3(transform.position.x, transform.position.y, 0);
            transform.position = finalPos;    //keep the player stay 0 on Z axis

            if ((isFacingRight && (transform.position.x > animTarget.position.x)) || (!isFacingRight && (transform.position.x < animTarget.position.x)))
                Flip();
        }

        float extraForceSpeed = 0;
        public void AddHorizontalForce(float _speed)
        {
            extraForceSpeed = _speed;
        }

        //Save the value for push and drag the object
        [HideInInspector] public Vector3 m_LastGroundPos = Vector3.zero;
        private float m_LastAngle = 0;
        [HideInInspector] public Transform m_CurrentTarget;
        public Vector3 DeltaPos { get; private set; }
        public float DeltaYAngle { get; private set; }

        public void UpdatePositionOnMovableObject(Transform target)
        {
            //If no target, stop here
            if (target == null)
            {
                m_CurrentTarget = null;
                return;
            }

            if (m_CurrentTarget != target)
            {
                m_CurrentTarget = target;       //set the current target
                DeltaPos = Vector3.zero;        //store the defaul delta position
                DeltaYAngle = 0;
            }
            else
            {
                DeltaPos = target.transform.position - m_LastGroundPos;     //cacuating the delta position to move the object
                DeltaYAngle = target.transform.rotation.eulerAngles.y - m_LastAngle;

                Vector3 direction = transform.position - target.transform.position;     //get the direction
                direction.y = 0;

                //get the final angle
                float FinalAngle = Vector3.SignedAngle(Vector3.forward, direction.normalized, Vector3.up) + DeltaYAngle;

                float xMult = Vector3.Dot(Vector3.forward, direction.normalized) > 0 ? 1 : -1;
                float zMult = Vector3.Dot(Vector3.right, direction.normalized) > 0 ? -1 : 1;

                float cosine = Mathf.Abs(Mathf.Cos(FinalAngle * Mathf.Deg2Rad));
                Vector3 deltaRotPos = new Vector3(cosine * xMult, 0,
                     Mathf.Abs(Mathf.Sin(FinalAngle * Mathf.Deg2Rad)) * zMult) * Mathf.Abs(direction.magnitude);

                DeltaPos += deltaRotPos * (DeltaYAngle * Mathf.Deg2Rad);
            }

            if (DeltaPos.magnitude > 3f)
                DeltaPos = Vector3.zero;

            //stop the character controller when moving the object
            characterController.enabled = false;
            transform.Rotate(0, DeltaYAngle, 0);
            characterController.enabled = true;


            m_LastGroundPos = target.transform.position;
            m_LastAngle = target.transform.rotation.eulerAngles.y;
        }

        // Add the position for the player
        public void AddPosition(Vector2 pos)
        {
            characterController.enabled = false;
            transform.position += (Vector3)pos;
            characterController.enabled = true;
        }

        //Set the player position
        public void SetPosition(Vector2 pos)
        {
            characterController.enabled = false;
            transform.position = (Vector3)pos;
            characterController.enabled = true;
        }

        //The teleport function
        public void TeleportTo(Vector3 pos)
        {
            if (!isPlaying)
                return;

            StartCoroutine(TeleportCo(pos));
        }

        IEnumerator TeleportCo(Vector3 pos)
        {
            isPlaying = false;

            //Hide the Controller when doing the teleport, disable the character controller too to avoid the issue
            ControllerInput.Instance.ShowController(false);
            yield return new WaitForSeconds(0.5f);
            characterController.enabled = false;
            transform.position = pos;
            characterController.enabled = true;
            isPlaying = true;
            ControllerInput.Instance.ShowController(true);
        }

        private void CheckStandOnEvent()
        {
            //Check if player stand on the object that contain the event
            var hasEvent = (IPlayerStandOn)groundHit.collider.gameObject.GetComponent(typeof(IPlayerStandOn));
            if (hasEvent != null)
                hasEvent.OnPlayerStandOn();
        }

        void Flip()
        {
            //Flip the player
            if (isSliding || isIgnorePlayerMovenmentInput || forceIdle)
                return;

            inputLastTime *= -1;
        }

        void GetSlopeVelocity(ref Vector2 vel)
        {
            //Check and get the slope
            var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
            vel = vel.x * crossSlope;

            Debug.DrawRay(transform.position, crossSlope * 10);
        }

        public void Victory()
        {
            //Victoy event, call finish game too
            isPlaying = false;
            GameManager.Instance.FinishGame();
            if (isUsingJetpack)
            {
                UseJetpack(false);
                UpdateJetPackStatus();
            }
        }

        void HitAndDie()
        {
            if (isDead)
                return;

            SoundManager.PlaySfx(soundHit);
            Die(transform.position, 50);
        }

        public void Die(Vector3 hitPoint, float force)
        {
            //If player is die, set some value to the default value and call the Gameover function
            if (isDead)
                return;

            anim.SetTrigger("die");
            SoundManager.PlaySfx(soundDie);
            isDead = true;
            velocity.x = 0;
            velocity.y = 0;
            rigAim.weight = 0;
            forceStandingRemain = 0;
            anim.applyRootMotion = true;
            isFiring = false;
            if (isJetpackActived)
                ActiveJetpack(false);

            GameManager.Instance.GameOver();

            //spawn the ragdoll
            if (ragdollObj)
            {
                gameObject.SetActive(false);
                var _hitPoint = hitPoint;
                _hitPoint.z = Random.Range(-0.5f, 0.5f);
                Vector3 _forceDirection = (transform.position + characterController.center) - _hitPoint;

                _forceDirection.Normalize();
                Instantiate(ragdollObj, transform.position, transform.rotation).gameObject.GetComponent<RagdollCharacter>().Init(_forceDirection, force);
            }
        }

        [HideInInspector] public bool isFallingFromHeight = false;
        [Tooltip("If player velocity Y lower this value, active the hard falling")]
        [HideInInspector] public float hardFallingDistance = 10;
        [HideInInspector] public float hardFallingIdleTime = 1;

        //Update the animation parameters
        void HandleAnimation()
        {
            anim.SetInteger("inputX", (int)input.x);
            anim.SetBool("isFacingRight", isFacingRight);
            anim.SetBool("moveBackward", (isFacingRight && velocity.x < 0) || (!isFacingRight && velocity.x > 0));
            anim.SetFloat("speed", Mathf.Abs(velocity.x));
            anim.SetBool("isGrounded", isGrounded);
            anim.SetFloat("height speed", velocity.y);

            //Check Hard Falling
            if (!isFallingFromHeight && (transform.position.y < (lastGroundPos - 0.5f)) && !isGrounded)
            {
                RaycastHit hit;
                if (!Physics.Raycast(transform.position, Vector3.down, out hit, hardFallingDistance))      //don't hit anything in this distance => Active hard falling
                    isFallingFromHeight = true;
            }
        }

        #region ATTACK

        public void RangeAttackState(bool holding)
        {
            isFiring = holding;
        }

        [HideInInspector] public RangeAttack rangeAttack;

        public void RangeAttack()
        {
            if (!isPlaying)
                return;
            if (isDead)
                return;

            //if is ignore state, no attack too
            if (IgnoreControllerInput())
            {
                return;
            }

            if (PlayerState == PlayerState.Windy)
            {
                return;
            }

            if (rangeAttack != null)
            {
                if (rangeAttack.Fire())        //check if can fire the bullet
                {
                    anim.SetTrigger("shoot");

                    if (rangeAttack.gunTypeID.reloadPerShoot)
                    {
                        Invoke("ReloadGun", 0.2f);
                    }
                }
            }
        }

        bool isLoading = false;

        void ReloadGun()
        {
            isLoading = true;
            anim.SetTrigger("reload");
            SoundManager.PlaySfx(rangeAttack.gunTypeID.reloadSound);
            Invoke("FinishLoading", rangeAttack.gunTypeID.reloadTime);
        }

        void FinishLoading()
        {
            isLoading = false;
        }

        float forceStandingRemain = 0;

        #endregion

        [HideInInspector] public RaycastHit groundHit;
        void CheckGround()
        {
            isGrounded = false;
            if (velocity.y > 0.1f)
                return;

            if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.99f, Vector3.down, out groundHit, 2, layerAsGround))
            {
                float distance = transform.position.y - groundHit.point.y;

                //if standing on the moving platform (deltaPos.y != 0), increase the delect ground to avoid problem
                if (distance <= (characterController.skinWidth + 0.01f + (DeltaPos.y != 0 ? 0.1 : 0)))
                {
                    isGrounded = true;
                    numberAirJumpCounter = 0;
                    //check if standing on small ledge then force play move
                    if (!Physics.Raycast(transform.position, Vector3.down, 1, layerAsGround))
                    {
                        if (input.x == 0 && groundHit.point.y > (transform.position.y - 0.1f))
                        {
                            var forceMoveOnLedge = Vector3.zero;
                            if (groundHit.point.x < transform.position.x)
                                forceMoveOnLedge = (transform.position - groundHit.point) * Time.deltaTime * 10 * (isFacingRight ? 1 : -1);
                            else
                                forceMoveOnLedge = (transform.position - groundHit.point) * Time.deltaTime * 10 * (isFacingRight ? -1 : 1);
                            //move the character with the force move value
                            characterController.Move(forceMoveOnLedge);
                        }
                    }
                }
            }
        }

        [HideInInspector] public Vector3 moveDirection;

        void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (isDead)
                return;

            moveDirection = hit.moveDirection;
            var isTrigger = hit.gameObject.GetComponent<TriggerEvent>();
            if (isTrigger)
            {
                isTrigger.OnContactPlayer();
            }
            //check hit object from below
            if (velocity.y > 1 && hit.moveDirection.y == 1)
            {
                velocity.y = 0;

            }
            Rigidbody body = hit.collider.attachedRigidbody;

            // no rigidbody
            if (body == null || body.isKinematic)
            {
                return;
            }

            // We dont want to push objects below us
            if (hit.moveDirection.y < -0.3)
            {
                return;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            if (characterController == null)
                characterController = GetComponent<CharacterController>();
        }

        private void OnDrawGizmosSelected()
        {
            if (isGrounded)
            {
                Gizmos.DrawWireSphere(groundHit.point, characterController.radius * 0.9f);
            }
        }

        public void Jump(float newForce = -1)
        {
            if (!isPlaying)
                return;

            if (isDead)
                return;

            playLandingEffect = true;
            

                if (IgnoreControllerInput())
                    return;

                if (GameManager.Instance.gameState != GameManager.GameState.Playing)
                    return;

                if (isUsingJetpack)
                    return;

                //ignore the moving object
                UpdatePositionOnMovableObject(null);
                
              if (isGrounded && (PlayerState == PlayerState.Ground || PlayerState == PlayerState.Windy))
                {
                    if (newForce == -1)
                        SoundManager.PlaySfx(soundJump, soundJumpVolume);

                    if (isSliding)
                        SlideOff();

                    isGrounded = false;
                    var _height = newForce != -1 ? newForce : maxJumpHeight;
                    velocity.y += Mathf.Sqrt(_height * -2 * gravity);
                    //velocity.x = characterController.velocity.x;
                    //Debug.LogError("After" + velocity.x);
                    characterController.Move(velocity * Time.deltaTime);

                }
                else if (isInJumpZone)
                {
                    if (newForce == -1)
                        SoundManager.PlaySfx(soundJump, soundJumpVolume);

                    var _height = maxJumpHeight;
                    velocity.y = Mathf.Sqrt(_height * -2 * gravity);

                    characterController.Move(velocity * Time.deltaTime);
                    isInJumpZone = false;
                    Time.timeScale = 1;
                }
                //If is in the air and still has the nummber air jump remain, keep jumping
                else if (!isGrounded && (numberAirJumpCounter < numberOfAirJump))
                {
                    numberAirJumpCounter++;
                    if (numberAirJumpCounter == 1)
                        anim.SetTrigger("doubleJump");
                    else if (numberAirJumpCounter == 2)
                        anim.SetTrigger("tripleJump");

                    if (newForce == -1)
                        SoundManager.PlaySfx(soundJump, soundJumpVolume);
                    var _height = newForce != -1 ? newForce : maxJumpHeight;
                    velocity.y = Mathf.Sqrt(_height * -2 * gravity);
                    velocity.x = characterController.velocity.x;

                    characterController.Move(velocity * Time.deltaTime);
                }
            
        }

        public void JumpOff()
        {
            if (!isPlaying)
                return;
            //set the jump force to minimum value when release the jump button
            var _minJump = Mathf.Sqrt(minJumpHeight * -2 * gravity);
            if (velocity.y > _minJump)
            {
                velocity.y = _minJump;
            }
        }

        //jump and do not allow change the direction until hit the Ground or Grab something
        [HideInInspector] public bool isIgnorePlayerMovenmentInput = false;
        void JumpAndIgnoreInput()
        {
            isIgnorePlayerMovenmentInput = true;
        }

        //JumpZoneObj lastJumpZoneObj;
        private void OnTriggerEnter(Collider other)
        {
            if (!isPlaying)
                return;

            //dont want to do anything in the dead mode
            if (isDead)
                return;

            //try to trigger the trigger event
            var isTrigger = other.GetComponent<TriggerEvent>();
            if (isTrigger)
            {
                isTrigger.OnContactPlayer();
            }

            if (other.gameObject.tag == "Finish")
                Victory();
            else if (other.gameObject.tag == "Deadzone")
                Die(transform.position, 50);

            if (other.gameObject.tag == "Checkpoint")
                SetCheckPoint(other.transform.position);
        }

        private void OnTriggerExit(Collider other)
        {

        }

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.tag == "Deadzone")
            {
                HitAndDie();
            }
        }

        #region MOVE

        //Ignore the input if one of those condition is true
        bool IgnoreControllerInput()
        {
            return forceIdle || isIgnorePlayerMovenmentInput
                 || forceIdle || forcePlayerStanding;
        }

        public void MoveLeft()
        {
            if (isPlaying)
            {
                input.x = -1;
            }
        }

        //This action is called by the Input/ControllerInput
        public void MoveRight()
        {
            if (isPlaying)
            {
                input.x = 1;
            }
        }

        //This action is called by the Input/ControllerInput
        public void MoveUp()
        {
            input.y = 1;
        }


        //This action is called by the Input/ControllerInput
        public void MoveDown()
        {
            input.y = -1;
        }

        public void StopMove(int fromDirection = 0)
        {
            if (fromDirection == 0)     //mean release Up/Down button
            {
                input.y = 0;
            }
            else
            {
                if (input.x != 0 && input.x != fromDirection)
                    return;

                input.x = 0;
            }
        }

        #endregion

        private void OnAnimatorMove()
        {
            // Vars that control root motion
            if (!anim || !anim.applyRootMotion)
                return;

            bool useRootMotion = true;
            bool verticalMotion = true;
            bool rotationMotion = true;

            Vector3 multiplier = Vector3.one;
            // Conditions to avoid animation root motion
            if (Mathf.Approximately(Time.deltaTime, 0f) || !useRootMotion) { return; }

            Vector3 delta = anim.deltaPosition;

            delta.z = 0;
            delta = transform.InverseTransformVector(delta);
            delta = Vector3.Scale(delta, multiplier);
            delta = transform.TransformVector(delta);
            // Get animator movement
            Vector3 vel = (delta) / Time.deltaTime;

            // Preserve vertical velocity
            if (!verticalMotion)
                vel.y = characterController.velocity.y;

            //Move the character controller
            characterController.Move(vel * Time.deltaTime);

            Vector3 deltaRot = anim.deltaRotation.eulerAngles;

            //Rotate the character
            if (rotationMotion)
                transform.rotation *= Quaternion.Euler(deltaRot);
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            //wait time before can take damage
            if (noTakeDamageOnBeginTime > 0)
                return;

            currentHealth -= damage;
            if (currentHealth <= 0)
            {
                //Die when get hit from enemy
                Die(hitPoint, force);
            }
            else
            {
                Hurt();
            }
        }

        public void AddHealth(int amount)
        {
            //add health value
            currentHealth += amount;
            //limit the max value
            currentHealth = Mathf.Min(currentHealth, maxHealth);
        }

        void Hurt()
        {
            if (blinkingEffect)
                blinkingEffect.DoBlinking();
        }

        #region JET PACK
        //[Header("---JET PACK---")]
        //the jetpack force to raise the player up
       [HideInInspector]  public float jetForce = 5;
        //jetpac drain time
        [HideInInspector] public float jetpackDrainTimeOut = 5f;      
        public float jetpackRemainTime { get; set; }
        //place the jetpack object
        [HideInInspector] public GameObject jetpackObj;
        //jet pack sound when using the jetpack
        [HideInInspector] public AudioClip jetpackSound;      
        [Range(0f, 1f)]
        [HideInInspector] public float jetpackSoundVolume = 0.5f;
        AudioSource jetpackAScr;
        [HideInInspector] public ParticleSystem[] jetpackEmission;
        [HideInInspector] public GameObject jetpackReleaseObj;
         public bool isJetpackActived { get; set; }
         public bool isUsingJetpack { get; set; }

        public void ActiveJetpack(bool active)
        {
            //Set active the jetpack,
            if (active)
            {
                //If player is sliding, set it off
                if (isSliding)
                    SlideOff();

                //Init the default value
                jetpackRemainTime = jetpackDrainTimeOut;
                isJetpackActived = true;
                jetpackObj.SetActive(true);
            }
            else if (isJetpackActived)
            {
                //Disable the jetpack state
                isJetpackActived = false;
                isUsingJetpack = false;
                jetpackObj.SetActive(false);

                var obj = Instantiate(jetpackReleaseObj, jetpackObj.transform.position, jetpackReleaseObj.transform.rotation);
                obj.GetComponent<Rigidbody>().velocity = new Vector2(isFacingRight ? -1 : 1, 2);
                //Destroy the jetpack object after 2 seconds
                Destroy(obj, 2);
                lastGroundPos = transform.position.y;
            }
        }

        public void AddJetpackFuel()
        {
            //Add the jetpack bar
            jetpackRemainTime = jetpackDrainTimeOut;
        }

        public void UseJetpack(bool use)
        {
            //if already use the jetpack, ignore it
            if (!isJetpackActived)
                return;

            if (jetpackRemainTime <= 0)
                return;

            //If is sliding, make it off
            if (isSliding)
                SlideOff();

            isUsingJetpack = use;
        }

        void UpdateJetPackStatus()
        {
            //Update the jetpack emission status and the sound
            for (int i = 0; i < jetpackEmission.Length; i++)
            {
                var emission = jetpackEmission[i].emission;
                emission.enabled = isUsingJetpack;
                jetpackAScr.volume = (isUsingJetpack & GlobalValue.isSound) ? jetpackSoundVolume : 0;
            }
        }

        #endregion

        #region SLIDING
        [HideInInspector] public float slidingTime = 1;       //set the slidig time
        [HideInInspector] public float slidingCapsultHeight = 0.8f;       //set the collision height when sliding to avoid
        float originalCharHeight, originalCharCenterY;
        [HideInInspector] public bool isSliding = false;
        [HideInInspector] public AudioClip soundSlide;        //the sound when sliding

        public void SlideOn()
        {
            if (GameManager.Instance.gameState != GameManager.GameState.Playing)
                return;

            if (!isGrounded)
                return;

            if (isSliding)
                return;

            if (isUsingJetpack)
                return;

            SoundManager.PlaySfx(soundSlide);
            isSliding = true;

            //Set the character collison height
            SetCharacterControllerSlidingSize();
            //Diable sliding after the sliding time
            Invoke("SlideOff", slidingTime);
        }

        void SlideOff()
        {
            if (!isSliding)
                return;

            if (isUsingJetpack)
                return;
            //Set the collision height back to normal
            SetCharacterControllerOriginalSize();

            isSliding = false;
        }
        #endregion

        void SetCharacterControllerSlidingSize()
        {
            //Set the collison height when sliding
            characterController.height = slidingCapsultHeight;
            var _center = characterController.center;
            //Change the center Y size
            _center.y = slidingCapsultHeight * 0.5f;
            characterController.center = _center;
        }

        void SetCharacterControllerOriginalSize()
        {
            //Set the collison height to normal
            characterController.height = originalCharHeight;
            var _center = characterController.center;
            //Change the center Y size
            _center.y = originalCharCenterY;
            characterController.center = _center;
        }

        public void SetGun(GunTypeID gunID)
        {

            anim.runtimeAnimatorController = gunID.animatorOverride;
            gunTypeID = gunID;

            //check to enable the gun as the type
            foreach (var gun in gunTypeIDList)
            {
                if (gun.gunTypeID.gunID == gunTypeID.gunID)
                {
                    gun.gameObject.SetActive(true);

                    //update gun firepoint
                    rangeAttack.firePoint = gun.firepoint;
                }else
                    gun.gameObject.SetActive(false);
            }

            //update gun id
            rangeAttack.UpdateGun(gunTypeID);
            //SoundManager.PlaySfx(SoundManager.Instance.swapGun);
        }
    }
}