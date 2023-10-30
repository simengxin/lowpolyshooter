using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
	public class HealthBar : MonoBehaviour
	{
		public GameObject container;
		public Transform healthBar;
		public float showTime = 1f;

        private void Start()
        {
			Hide();
		}

        public void UpdateValue(float value)
		{
			StopAllCoroutines();
			CancelInvoke();

			container.SetActive(true);

			value = Mathf.Max(0, value);
			healthBar.localScale = new Vector2(value, healthBar.localScale.y);
			if (value > 0)
				Invoke("Hide", showTime);
			else
				Destroy(gameObject);
		}

		public void Hide()
		{
			container.SetActive(false);
		}
	}
}