using UnityEngine;
namespace PhoenixaStudio
{
	public class EnemyMeleeAttack : MonoBehaviour
	{
		public float delayCheckTarget = 0.2f; //sync with the attack animation
		public LayerMask targetPlayer;      //set the layer of target
		public Transform checkPoint;        //place the checkpoint pos
		public Transform meleePoint;        //place the melee point checkpoint area to damage the target	
		public float detectDistance = 1;    //delect the target within this distance
		public float meleeRate = 1;     //the time delay between 2 attacks
		float lastShoot = 0;
		public bool isAttacking { get; set; }

		public float meleeAttackZone = .7f;     //the area affect the target
		public float meleeAttackCheckPlayer = 0.1f;
		public int meleeDamage = 20;  //give damage to player
		public AudioClip[] soundAttacks;

		public bool AllowAction()
		{
			return Time.time - lastShoot > meleeRate;
		}


		public bool CheckPlayer(bool isFacingRight)     //check the target ahead and allow attack
		{
			RaycastHit hit;
			if (Physics.CapsuleCast(transform.position + Vector3.up * 1 * 0.5f, transform.position + Vector3.up * (1 - 0.25f),
			  0.25f, isFacingRight ? Vector3.right : Vector3.left, out hit, detectDistance, targetPlayer))
			{

				if (hit.collider)
					return true;
				else
					return false;
			}

			return false;
		}

		public void Action()
		{
			//store the last attack time
			lastShoot = Time.time;      
									
			CancelInvoke();
			//Check the target after the delay time to sync with the attack animation
			Invoke("CheckHitTarget", delayCheckTarget);
		}

		public void CheckHitTarget()
		{
			/*
			 * Check the target
			 */
			var hits = Physics.OverlapSphere(meleePoint.position, meleeAttackZone, targetPlayer);
			foreach (var hit in hits)		//if there are targets, check and deal damage to the target
			{
				if (hit)
				{
					var damage = (ICanTakeDamage)hit.gameObject.GetComponent(typeof(ICanTakeDamage));
					if (damage != null)
					{
						damage.TakeDamage(meleeDamage, 200, gameObject, transform.position);
					}
				}
			}

			if (soundAttacks.Length > 0)
				SoundManager.PlaySfx(soundAttacks[Random.Range(0, soundAttacks.Length)]);

			Invoke("EndAttack", 1);
		}

		void EndAttack()
		{
			isAttacking = false;
		}

		void OnDrawGizmos()
		{
			Gizmos.color = Color.red;
			Gizmos.DrawLine(checkPoint.position, checkPoint.position + Vector3.right * detectDistance);
			Gizmos.DrawSphere(checkPoint.position + Vector3.right * detectDistance, 0.1f);

			if (meleePoint != null)
			{
				Gizmos.color = Color.yellow;
				Gizmos.DrawWireSphere(meleePoint.position, meleeAttackZone);
			}
		}
	}
}