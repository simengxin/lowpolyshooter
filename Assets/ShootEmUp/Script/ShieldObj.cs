using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class ShieldObj : MonoBehaviour, ICanTakeDamage
    {
        public int maxHealth = 200;
        [ReadOnly] public int currentHealth;

        public AudioClip soundHit;

        private void Awake()
        {
            currentHealth = maxHealth;
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            SoundManager.PlaySfx(soundHit);

            currentHealth -= damage;
            if (currentHealth <= 0)
                gameObject.SetActive(false);
        }
    }
}