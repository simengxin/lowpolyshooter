using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
	public class EnemyThrowAttack : MonoBehaviour
	{
		public enum ThrowAction { WaitPlayerInRange, ThrowAuto }
		public ThrowAction throwAction;
		[Header("Grenade")]
		public float angleThrow = 60;       //the angle to throw the bomb
		public float throwForce = 300;      //how strong?
		public float addTorque = 100;
		public float throwRate = 0.5f;
		public Transform throwPosition;     //throw the bomb at this position
		public Grenade _Grenade;        //the bomb prefab object
		public int makeDamage = 100;
		public float radius = 3;
		public AudioClip soundAttack;
		float lastShoot = -999;
		[Header("===CHECK TARGET ZONE===")]
		public LayerMask targetPlayer;
		public float radiusDetectPlayer = 5;
		public bool isAttacking { get; set; }

		public bool AllowAction()
		{
			return Time.time - lastShoot > throwRate;
		}

		public void Throw(bool isFacingRight, Vector2 throwDirection)
		{
			Vector3 throwPos = throwPosition.position;
			var obj = (Grenade)Instantiate(_Grenade, throwPos, Quaternion.identity);
			obj.Init(makeDamage, radius, false, false, GameManager.Instance.Player.transform.position.y + 2);

			if (throwDirection == Vector2.zero)
			{
				float angle;
				angle = isFacingRight ? angleThrow : 135;
				obj.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
			}
			else
			{
				obj.transform.forward = throwDirection;
			}

			obj.GetComponent<Rigidbody>().AddRelativeForce(obj.transform.right * throwForce);
			obj.GetComponent<Rigidbody>().AddTorque(obj.transform.up * addTorque);
		}

		// Update is called once per frame
		public bool CheckPlayer()
		{
			if (throwAction == ThrowAction.ThrowAuto)
				return true;

			//RaycastHit2D hit = Physics2D.CircleCast(checkPoint.position, radiusDetectPlayer, Vector2.zero, 0, targetPlayer);

			if (Physics.OverlapSphere(transform.position, radiusDetectPlayer, targetPlayer).Length > 0)
				return true;
			else
				return false;
		}

		public void Action()
		{
			if (_Grenade == null)
				return;
			lastShoot = Time.time;
		}

		private void OnDrawGizmos()
		{
			if (throwAction == ThrowAction.WaitPlayerInRange)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(transform.position, radiusDetectPlayer);
			}
		}
	}
}