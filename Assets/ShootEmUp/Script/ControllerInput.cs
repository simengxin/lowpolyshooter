using UnityEngine;
namespace PhoenixaStudio
{

    [RequireComponent(typeof(CanvasGroup))]
    public class ControllerInput : MonoBehaviour
    {
        public static ControllerInput Instance;
        public Transform aimIcon;

        public GameObject PCGroup, MobileGroup;
        CanvasGroup canvasGroup;

        GameplayControl controls; 

        private void OnEnable()
        {
            if (GameManager.Instance != null)
                StopMove(0);

            controls.PlayerControl.Enable();        //enable to active the new input controller
        }

        private void OnDisable()
        {
            controls.PlayerControl.Disable();       //disable it when restart or move to another scene

        }

        private void Awake()
        {
            Instance = this;

            controls = new GameplayControl();
            controls.PlayerControl.B.started += ctx => Jump();          //get the event of the button
            controls.PlayerControl.B.canceled += ctx => JumpOff();      

            controls.PlayerControl.SwitchGun.started += ctx => SwitchGun();

            controls.PlayerControl.X.started += ctx => RangeAttack(true);
            controls.PlayerControl.X.canceled += ctx => RangeAttack(false);

            controls.PlayerControl.DLeft.started += ctx => MoveLeft();
            controls.PlayerControl.DLeft.canceled += ctx => StopMove(-1);

            controls.PlayerControl.DRight.started += ctx => MoveRight();
            controls.PlayerControl.DRight.canceled += ctx => StopMove(1);

            controls.PlayerControl.DUp.started += ctx => MoveDUp();
            controls.PlayerControl.DUp.canceled += ctx => StopMove(0);

            controls.PlayerControl.DDown.started += ctx => MoveDown();
            controls.PlayerControl.DDown.canceled += ctx => StopMove(0);

            controls.PlayerControl.Jetpack.started += ctx => UseJetpack(true);
            controls.PlayerControl.Jetpack.canceled += ctx => UseJetpack(false);
        }

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();      //set the variable

            PCGroup.SetActive(GameManager.Instance.controllerType == ControllerType.PC);
            MobileGroup.SetActive(GameManager.Instance.controllerType == ControllerType.Mobile);
        }

        public void UpdateShootingJoystickMobile(Vector2 delta)
        {
            if (delta.magnitude > 0.1f)
            {
                aimIcon.position = Camera.main.WorldToScreenPoint(GameManager.Instance.Player.transform.position + (Vector3)delta * 5);
            }

            //only firing when reach the distance
            if (delta.magnitude > 0.3f)
            {
                RangeAttack(true);
            }
        }

        public Vector3 GetAimPosition()
        {
            return aimIcon.transform.position;
        }

        private void Update()
        {
            if (GameManager.Instance.controllerType == ControllerType.PC)
                aimIcon.position = Input.mousePosition;
        }

        public void ShowController(bool show)       //hide and show the controller
        {
            if (show)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
                canvasGroup.alpha = 1;
            }
            else
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.alpha = 0;
            }
        }

        bool allowJump = true;

        public void Jump()      //player action
        {
            if (allowJump)
                GameManager.Instance.Player.Jump();
        }

        public void JumpOff()
        {
            if (allowJump)
                GameManager.Instance.Player.JumpOff();
        }

        public void MoveLeft()
        {
            if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                GameManager.Instance.Player.MoveLeft();
            }
        }

        public void MoveRight()
        {
            if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                GameManager.Instance.Player.MoveRight();
            }
        }

        public void MoveDUp()
        {
            if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                GameManager.Instance.Player.MoveUp();
            }
        }

        public void MoveDown()
        {
            if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                GameManager.Instance.Player.MoveDown();
            }
        }

        public void StopMove(int fromDirection)
        {
            if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                GameManager.Instance.Player.StopMove(fromDirection);
            }
        }

        public void RangeAttack(bool holding)
        {
            if (GameManager.Instance.gameState == GameManager.GameState.Playing)
            {
                GameManager.Instance.Player.RangeAttackState(holding);
            }
        }

        public void SwitchGun()
        {
            GunManager.Instance.NextGun();
        }

        public void UseJetpack(bool use)
        {
            if (use && !GameManager.Instance.Player.isJetpackActived)
                return;

            GameManager.Instance.Player.UseJetpack(use);
        }
    }
}