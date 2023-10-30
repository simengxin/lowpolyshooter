using UnityEngine;
namespace PhoenixaStudio
{
    public class PlayerCheckSlopeAngle : MonoBehaviour      //check the slope angle to adjust the speed 
    {
        public LayerMask layerAsGround;     //set the ground layer
        public float slideSpeed = 10;       //set the slide speed
        public float accSpeed = 1;      //the speed acc
        public float angleAsSlope = 45;     //how angle consider this is the slope

        [Header("---Rootbone Rotate Controll---")]
        public Transform rootBone;      //place the root bone of the character
        public Vector3 rotateBoneOnSliding = new Vector3(0, 90, 10);        //rotate the root bone this angle when sliding

        [ReadOnly] public float currentAngle;
        [ReadOnly] public bool isStandOnTheSlope;
        public RaycastHit hitGround;

        void Update()
        {
            isStandOnTheSlope = false;
            //Check the slope of the ground
            if (Physics.Raycast(transform.position + Vector3.up * 1, Vector3.down, out hitGround, 2, layerAsGround))
            {
                Debug.DrawRay(hitGround.point, hitGround.normal * 2);
                currentAngle = Vector3.Angle(hitGround.normal, GameManager.Instance.Player.isFacingRight ? Vector3.right : Vector3.left);
                //set the standing on slope = true if the angle fit this conditions
                if (currentAngle <= angleAsSlope || currentAngle > 120)
                {
                    isStandOnTheSlope = true;
                }
            }
        }
    }
}