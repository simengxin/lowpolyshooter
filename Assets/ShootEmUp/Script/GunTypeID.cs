using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class GunTypeID : MonoBehaviour
    {
        public string gunID = "gun ID";
        //Allow add the gun at begin game
        public bool useItOnStart = false;
        //the ranking of the gun, allow auto switch to the highest after the current gun run out of ammo
        public int ranking = 1;

        [Header("===ANIMATION===")]
        public AnimatorOverrideController animatorOverride;
        public bool canReflected = false;
        public GameObject bulletObj;

        [Header("===RELOADING===")]
        public bool reloadPerShoot = false;
        public float reloadTime = 0.5f;
        public AudioClip reloadSound;

        public int damage = 10;
        public float bulletSpeed = 1000;
        public float bulletForce = 300;
        public bool useExplosion = false;

        [Header("===WEAPONS===")]
        public bool unlimitedBullet = false;
        public int maxBullet = 99;
        public float rate = 0.2f;
        public AudioClip soundFire;
        [Range(0, 1)]
        public float soundFireVolume = 0.5f;
        public int maxBulletPerShoot = 1;
        public bool isSpreadBullet = false;
        public GameObject muzzleFX;


        [Header("---Gun Recoil Effect---")]
        public AnimationCurve recoildCurve;
        public float recoilDuration = 0.15f;
        public float recoilMaxRotation = 10;
       
        public void ResetBullet()
        {
            bullet = maxBullet;
        }

        public int bullet
        {
            get { return PlayerPrefs.GetInt("gunID" + gunID, unlimitedBullet ? int.MaxValue: maxBullet); }
            set { PlayerPrefs.SetInt("gunID" + gunID, Mathf.Min(value, maxBullet)); }
        }
    }
}