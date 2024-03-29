﻿
/*
 * 
 * 
 * Override the platformer
 * 
 * 
 */
using UnityEngine;
namespace PhoenixaStudio
{
	public class PlayerOverrideParametersChecker : MonoBehaviour
	{
		public LayerMask layerZoneCheck;		//set the layer of the zone need to check
		[ReadOnly]
		public bool useOverrideAcc = false;	
		[ReadOnly]
		public float accGrounedOverride = 0;
		[ReadOnly]
		public float velocityDevide = 1;
		[ReadOnly]
		public OverrideParameterZone currentZone = null;

		private void Update()
		{
			//stop if the character controller no deteced
			if (GameManager.Instance.Player.characterController == null)
				return;

			//check the zone
			var hitZones = Physics.OverlapSphere(transform.position + Vector3.up, GameManager.Instance.Player.characterController.radius, layerZoneCheck);

			//if hit the zone, try to get the OverrideParameterZone and set to the player
			if (hitZones.Length > 0)
			{
				OverrideParameterZone zone = hitZones[0].GetComponent<OverrideParameterZone>();
				if (zone && zone != currentZone)
				{
					currentZone = zone;
					GameManager.Instance.Player.PlayerState = zone.zone;

					if (zone.isOverridParameter)
					{
						GameManager.Instance.Player.SetOverrideParameter(zone.overrideParameter, true, zone.zone);
						GameManager.Instance.Player.SetupParameter();
					}

					if (zone.isOverrideAcceleration && zone.overrideAcc > 0 && zone.overrideAcc < 1)
					{
						accGrounedOverride = 1f / zone.overrideAcc;
						useOverrideAcc = true;
					}

					if (zone.canWalkOnThis && GameManager.Instance.Player.groundHit.collider.gameObject != zone.gameObject)
						useOverrideAcc = false;

					velocityDevide = 1;
					if (zone.isOverrideAcceleration && zone.overrideAcc > 1)
					{
						velocityDevide = 1f / zone.overrideAcc;
					}

					if (zone.isUseAddForce)
					{
						GameManager.Instance.Player.AddHorizontalForce(zone.forceMoveSpeed);
					}
				}
			}
			else if (currentZone != null)
			{
				//if current zone available, try to get the zone and set the the player
				OverrideParameterZone zone = currentZone.GetComponent<OverrideParameterZone>();
				if (zone)
				{
					GameManager.Instance.Player.ExitZoneEvent();

					if (zone.isOverridParameter)
					{
						GameManager.Instance.Player.SetOverrideParameter(zone.overrideParameter, false);
						GameManager.Instance.Player.SetupParameter();
					}
					if (zone.isOverrideAcceleration)
					{
						useOverrideAcc = false;
						velocityDevide = 1;
					}

					if (zone.isUseAddForce)
					{
						GameManager.Instance.Player.AddHorizontalForce(0);
					}

					currentZone = null;
				}
			}
		}
	}
}