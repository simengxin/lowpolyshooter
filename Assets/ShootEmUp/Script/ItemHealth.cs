using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class ItemHealth : TriggerEvent
    {
        public int healthAmount = 50;
        public GameObject effect;
        public AudioClip soundCollection;

        public override void OnContactPlayer()
        {
            GameManager.Instance.Player.AddHealth(healthAmount);
            if (effect)
                Instantiate(effect, transform.position, effect.transform.rotation);
            SoundManager.PlaySfx(soundCollection);
            Destroy(gameObject);
        }
    }
}