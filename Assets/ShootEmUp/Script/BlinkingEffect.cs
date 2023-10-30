using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
    public class BlinkingEffect : MonoBehaviour
    {
        public enum BlinkMode { Auto, Manual}
        public BlinkMode blinkMode = BlinkMode.Manual;
        public Color startColor = Color.white;
        public Color endColor = Color.red;
        [Range(0, 10)]
        public float speed = 8;

        public float time = .6f;

        //public Renderer[] rend;
        [ReadOnly] public List<Renderer> rend;

        private void Awake()
        {
            rend = new List<Renderer>(transform.GetComponentsInChildren<SkinnedMeshRenderer>());
        }

        // Start is called before the first frame update
        void Start()
        {
            if (blinkMode == BlinkMode.Auto)
                StartCoroutine(BlinkingCo());
        }

        public void DoBlinking(float _tempTime = -1)
        {
            StopAllCoroutines();
            StartCoroutine(BlinkingCo(_tempTime));
        }

        // Update is called once per frame
        IEnumerator BlinkingCo(float _tempTime = -1)
        {
            if (rend.Count == 0)
                yield break;

            var beginTime = Time.time;
            var _time = _tempTime > 0 ? _tempTime : time;

            while ((Time.time - _time) < beginTime)
            {
                foreach (var r in rend)
                {
                    r.material.color = Color.Lerp(startColor, endColor, Mathf.PingPong(speed * Time.time, 1));
                    yield return null;
                }
            }

            foreach (var r in rend)
            {
                r.material.color = Color.white;
            }
        }
    }
}