using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class GunManager : MonoBehaviour
    {
        public static GunManager Instance;
        public List<GunTypeID> listGun;
        [ReadOnly] public List<GunTypeID> listGunPicked;

        int currentPos = 0;

        private void Awake()
        {
            if (GunManager.Instance != null)
                Destroy(gameObject);
            else
            {
                Instance = this;
                ResetPlayerCarryGun();

                //DontDestroyOnLoad(gameObject);
            }
        }

        public void ResetPlayerCarryGun()
        {
            listGunPicked.Clear();
            for (int i = 0; i < listGun.Count; i++)
            {
                if (listGun[i].useItOnStart)
                    AddGun(listGun[i]);
            }

            currentPos = 0;
        }

        public void AddBullet(int amount)
        {
            foreach (var gun in listGunPicked)
            {
                gun.bullet += amount;
            }
        }

        public void ResetGunBullet()
        {
            foreach (var gun in listGunPicked)
            {
                gun.ResetBullet();
            }
        }

        public void AddGun(GunTypeID gunID, bool pickImmediately = false)
        {
            listGunPicked.Add(gunID);
        }

        public void SetNewGunDuringGameplay(GunTypeID gunID)
        {
            GunTypeID pickGun = null;
            foreach (var gun in listGun)
            {
                if (gun.gunID == gunID.gunID)
                {
                    if (!listGunPicked.Contains(gun))
                        AddGun(gun);
                    else
                    {
                        foreach (var _gun in listGunPicked)
                        {
                            if (_gun.gunID == gun.gunID)
                                _gun.ResetBullet();
                        }
                    }

                    pickGun = gun;
                }
            }

            if (pickGun != null)
            {
                NextGun(pickGun);
                pickGun.ResetBullet();
            }
        }

        public void RemoveGun(GunTypeID gunID)
        {
            listGunPicked.Remove(gunID);
        }

        public void NextGun()
        {
            currentPos++;
            if (currentPos >= listGunPicked.Count)
            {
                currentPos = 0;
            }

            if (listGunPicked[currentPos].bullet <= 0)
                NextGun();
            else
            {
                GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
                SoundManager.PlaySfx(SoundManager.Instance.swapGun);
            }
        }

        public void GetNextHighestGun()
        {
            int gunLevel = 0;
            int gunPos = 0;

            for (int i = 0; i < listGunPicked.Count; i++)
            {
                if (listGunPicked[i].bullet >=0 && ( listGunPicked[i].ranking > gunLevel))
                {
                    gunLevel = listGunPicked[i].ranking;
                    gunPos = i;
                }

                //if (listGunPicked[i].gunID == gunID.gunID)
                //{
                //    currentPos = i;
                //    GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
                //    SoundManager.PlaySfx(SoundManager.Instance.swapGun);
                //}
            }
            currentPos = gunPos;
            GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
            SoundManager.PlaySfx(SoundManager.Instance.swapGun);
        }

        public void NextGun(GunTypeID gunID)
        {
            if (listGunPicked[currentPos].gunID == gunID.gunID)
                return;     //don't swap gun when the player holding the same gun

            for (int i = 0; i < listGunPicked.Count; i++)
            {
                if (listGunPicked[i].gunID == gunID.gunID)
                {
                    currentPos = i;
                    GameManager.Instance.Player.SetGun(listGunPicked[currentPos]);
                    SoundManager.PlaySfx(SoundManager.Instance.swapGun);
                }
            }
        }

        public GunTypeID getGunID()
        {
            return listGunPicked[currentPos];
        }
    }
}