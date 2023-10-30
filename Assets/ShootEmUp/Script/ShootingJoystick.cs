using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
namespace PhoenixaStudio
{
	public class ShootingJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public bool keepThePosition = false;
		public bool hideWhenNoUse = false;
		[Range(1, 100)]
		public int moveRangePercent = 5;
		int MovementRange = 10;


		Vector3 m_StartPos;
		public Transform targetDotImage, targetRing;

		void Start()
		{
			MovementRange = Screen.width * moveRangePercent / 100;
			m_StartPos = targetDotImage.position;
			if (hideWhenNoUse)
			{
				targetDotImage.gameObject.SetActive(false);
				targetRing.gameObject.SetActive(false);
			}
		}

		void UpdateVirtualAxes(Vector3 value)
		{
			var delta = m_StartPos - value;
			delta.y = -delta.y;
			delta /= MovementRange;

			var finalDelta = delta;
			finalDelta.x *= -1;
			ControllerInput.Instance.UpdateShootingJoystickMobile(finalDelta);

			//ControllerInput.Instance.Horizontal = -delta.x;
			//ControllerInput.Instance.Vertical = delta.y;
		}

		public void OnDrag(PointerEventData data)
		{
			Vector3 newPos = Vector3.zero;
			int delta = (int)(data.position.x - m_StartPos.x);
			newPos.x = delta;

			delta = (int)(data.position.y - m_StartPos.y);
			newPos.y = delta;
			targetDotImage.transform.position = Vector3.ClampMagnitude(new Vector3(newPos.x, newPos.y, newPos.z), MovementRange) + m_StartPos;
			UpdateVirtualAxes(targetDotImage.transform.position);
		}


		public void OnPointerUp(PointerEventData data)
		{
			targetDotImage.transform.position = m_StartPos;
			//
			UpdateVirtualAxes(m_StartPos);
			if (hideWhenNoUse)
			{
				targetDotImage.gameObject.SetActive(false);
				targetRing.gameObject.SetActive(false);
			}

			ControllerInput.Instance.RangeAttack(false);
		}

		public void OnPointerDown(PointerEventData data)
		{
			if (!keepThePosition)
				m_StartPos = data.position;

			targetDotImage.transform.position = m_StartPos;
			targetRing.transform.position = m_StartPos;
			if (hideWhenNoUse)
			{
				targetDotImage.gameObject.SetActive(true);
				targetRing.gameObject.SetActive(true);
			}

			//ControllerInput.Instance.UpdateShootingJoystickMobile(new Vector3(1, 0.25f, 0));

			ControllerInput.Instance.RangeAttack(true);
		}

		void OnDisable()
		{
			UpdateVirtualAxes(m_StartPos);
		}
	}
}