using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class TriggerObjects : TriggerEvent
    {
        public bool disableTargetOnStart = true;
        public string sendMessage = "DetectPlayer";
        public GameObject[] targets;
        List<MonoBehaviour> listMono;

        bool isWorked = false;
        void Awake()
        {
            Init();
        }

        void Init()
        {

            listMono = new List<MonoBehaviour>();
            foreach (var obj in targets)
            {
                if (obj)
                {
                    MonoBehaviour[] monos = obj.GetComponents<MonoBehaviour>();
                    foreach (var mono in monos)
                    {
                        listMono.Add(mono);
                        mono.enabled = false;
                    }

                    obj.SetActive(!disableTargetOnStart);
                }
            }

            isWorked = false;
        }

        public override void OnContactPlayer()
        {
            Action();
        }

        public void Action()
        {
            base.OnContactPlayer();
            if (isWorked)
                return;
            foreach (var obj in targets)
            {
                if (obj)
                {
                    obj.SetActive(true);
                    foreach (var mono in listMono)
                    {
                        mono.enabled = true;
                    }
                    obj.SendMessage(sendMessage, SendMessageOptions.DontRequireReceiver);
                }
            }
            isWorked = true;
        }

        void OnDrawGizmos()
        {
            if (Application.isPlaying)
                return;

            Gizmos.color = Color.white;
            if (targets != null && targets.Length > 0)
            {
                foreach (var obj in targets)
                {
                    if (obj)
                        Gizmos.DrawLine(new Vector2(transform.position.x, transform.position.y + 1), obj.transform.position);

                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawCube((Vector3)transform.position + GetComponent<BoxCollider>().center, GetComponent<BoxCollider>().size);
        }
    }
}