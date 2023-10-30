using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class ExplosionBarrel : MonoBehaviour, ICanTakeDamage
    {
        public int health = 100;
        int currentHealth;
        public Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);

        [Header("Explosion Damage")]
        public AudioClip soundDestroy;
        public GameObject[] DestroyFX;
        public LayerMask collisionLayer;
        public int makeDamage = 100;
        public float force = 500;
        public float radius = 3;

        HealthBar healthBar;

        void Start()
        {
            currentHealth = health;
            var healthBarObj = (HealthBar)Resources.Load("HealthBar", typeof(HealthBar));
            healthBar = (HealthBar)Instantiate(healthBarObj, healthBarOffset, Quaternion.identity);
        }

        public void TakeDamage(int damage, float force, GameObject instigator, Vector3 hitPoint)
        {
            if (currentHealth <= 0)
                return;

            currentHealth -= damage;
            healthBar.UpdateValue((float)currentHealth / (float)health);

            if (currentHealth <= 0)
                DoExplosion();
        }

        public void DoExplosion()
        {
            var hits = Physics.OverlapSphere(transform.position, radius, collisionLayer);
            if (hits == null)
                return;

            foreach (var hit in hits)
            {
                var damage = (ICanTakeDamage)hit.GetComponent<Collider>().gameObject.GetComponent(typeof(ICanTakeDamage));
                if (damage == null)
                    continue;

                damage.TakeDamage(makeDamage, force, gameObject, transform.position);
            }

            foreach (var fx in DestroyFX)
            {
                if (fx)
                {
                    Instantiate(fx, transform.position + Vector3.up, fx.transform.rotation);
                }
            }

            SoundManager.PlaySfx(soundDestroy);
            Destroy(gameObject);
        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }
}