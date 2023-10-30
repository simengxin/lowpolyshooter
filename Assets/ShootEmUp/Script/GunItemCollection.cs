using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio {
    public class GunItemCollection : TriggerEvent
    {
        public GunTypeID gunTypeID;
        public AudioClip soundCollection;

        public override void OnContactPlayer()
        {
            GunManager.Instance.SetNewGunDuringGameplay(gunTypeID);
            SoundManager.PlaySfx(soundCollection);
            Destroy(gameObject);
        }
    }
}