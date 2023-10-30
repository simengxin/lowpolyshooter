using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class EnemyHeadDamagePart : MonoBehaviour, ICanTakeDamage
    {
        public GameObject owner;
        public int multipleDamage = 3;

        private void Start()
        {
            //try to get the parent if not place
            if (owner == null)
                owner = transform.parent.gameObject;
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            if (owner == null)
            {
                Debug.LogError("No found the parent to deal the damage");
                return;
            }
            //get the owner damage
            var takeDamage = (ICanTakeDamage)owner.GetComponent(typeof(ICanTakeDamage));
            if (takeDamage == null)
                return;

            var newDamage = damage * multipleDamage;
            takeDamage.TakeDamage(newDamage, force, instigator, hitPoint);
        }
    }
}