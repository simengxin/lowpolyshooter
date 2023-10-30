using UnityEngine;
using System.Collections;
namespace PhoenixaStudio
{
    public class RangeAttack : MonoBehaviour
    {
        [Header("WEAPONS")]
        [HideInInspector] public GunTypeID gunTypeID;
        public Transform firePoint;
        float lastTimeShooting = -999;
        bool allowShooting = true;
        public bool isFacingRight { get { return transform.rotation.eulerAngles.y == 0; } }
        public GameObject muzzleFX;

        [Header("---Gun Recoil Effect---")]
        [ReadOnly] public AnimationCurve recoildCurve;
        [ReadOnly] public float recoilDuration = 0.15f;
        [ReadOnly] public float recoilMaxRotation = 10;
        public Transform[] recoilAffectedBones;
        private float recoilTimer;

        private void Awake()
        {
        }

        public void UpdateGun(GunTypeID _gunTypeID)
        {
            SetGun(_gunTypeID);

            UpdateAllUpgrades();
        }

        public void UpdateAllUpgrades()
        {
            recoildCurve = gunTypeID.recoildCurve;
            recoilDuration = gunTypeID.recoilDuration;
            recoilMaxRotation = gunTypeID.recoilMaxRotation;
        }

        private void LateUpdate()
        {
            if (recoilTimer < 0)
                return;

            float curveTime = (Time.time - recoilTimer) / recoilDuration;
            if (curveTime > 1f)
            {
                recoilTimer = -1;
            }
            else
            {
                foreach(var bone in recoilAffectedBones)
                {
                    bone.Rotate(Vector3.right, recoildCurve.Evaluate(curveTime) * recoilMaxRotation, Space.Self);
                }
            }
        }

        public bool Fire()
        {
            if (!allowShooting)
                return false;

            if (Time.time < (lastTimeShooting + gunTypeID.rate))
                return false;

            lastTimeShooting = Time.time;
            recoilTimer = Time.time;

            //for spread bullet
            int _right = 0;
            int _left = 0;

            //Spawn the muzzle effect
            if (muzzleFX)
            {
                var muzzleFXObj = PoolingObjectHelper.GetTheObject(muzzleFX, firePoint.position, true);
                muzzleFXObj.transform.up = firePoint.forward;
            }

            for (int i = 0; i < (gunTypeID.maxBulletPerShoot); i++)
            {
                StartCoroutine(FireCo());

                var projectile = PoolingObjectHelper.GetTheObject(gunTypeID.bulletObj, firePoint.position, false);
                projectile.transform.forward = firePoint.forward;

                if (gunTypeID.isSpreadBullet)
                {
                    if (i != 0)
                    {
                        if (i % 2 == 1)
                        {
                            _right++;
                            projectile.transform.Rotate(Vector3.forward, 10 * _right, Space.World);
                        }
                        else
                        {
                            _left++;
                            projectile.transform.Rotate(Vector3.forward, -10 * _left, Space.World);
                        }
                    }
                }
                else
                {
                    if (i != 0)
                    {
                        if (i % 2 == 1)
                        {
                            _right++;
                            projectile.transform.position += transform.right * 0.2f * _right;
                        }
                        else
                        {
                            _left++;
                            projectile.transform.position -= transform.right * 0.2f * _left;
                        }
                    }
                }
                projectile.gameObject.SetActive(true);
                projectile.GetComponent<BulletProjectile>().InitBullet(
                    gunTypeID.damage,
                    gunTypeID.bulletSpeed, gunTypeID.canReflected, gunTypeID.useExplosion, gunTypeID.bulletForce);
            }

            SoundManager.PlaySfx(gunTypeID.soundFire, gunTypeID.soundFireVolume);

            gunTypeID.bullet--;

            CancelInvoke("CheckBulletRemain");
            Invoke("CheckBulletRemain", gunTypeID.rate);

            return true;
        }

        void CheckBulletRemain()
        {
            if (gunTypeID.bullet <= 0)
            {
                GunManager.Instance.GetNextHighestGun();
            }
        }

        public IEnumerator FireCo()
        {
            yield return null;

            if (gunTypeID.muzzleFX)
            {
                var _muzzle = PoolingObjectHelper.GetTheObject(gunTypeID.muzzleFX, firePoint.position, true);
                _muzzle.transform.forward = firePoint.up;
            }
        }

        public void SetGun(GunTypeID gunID)
        {
            gunTypeID = gunID;
            allowShooting = false;
            Invoke("AllowShooting", 0.3f);
        }

        void AllowShooting()
        {
            allowShooting = true;
        }
    }
}