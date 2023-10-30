using UnityEngine;
namespace PhoenixaStudio
{
	public interface ICanTakeDamage		//use this script to allow the damage object can detect and deal the damage
	{
		void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint);
	}
}