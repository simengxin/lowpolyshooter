using UnityEngine;
using System.Collections.Generic;
namespace PhoenixaStudio
{
	public class MovingObjectHelper : MonoBehaviour
	{
		//only move when the player contact
		public bool moveWhenPlayer = false;
		//auto move on start
		public bool autoMove = true;
		//allow the moving loop
		public bool isLoop = true;
		[Header("Look At Target")]
		//allow look at the next target
		public bool lookAtNextPoint = false;
		//look offset angle
		public float lookAtOffset = 0;
		public float speedLook = 1;
		bool isReverseMoving = false;
		[Space]
		[Tooltip("Ignore All Value and just follow this object")]
		//ignore all value if follow target
		public Transform followTarget;
		//delay defore moving
		public float delayStart = 0;
		bool isStanding;

		[Tooltip("If has 2 or more, use them instead localWaypoints")]
		public GameObject Paths;
		public List<Transform> objectLocalWaypoints;
		public List<Vector3> localWaypoints;
		Vector3[] globalWaypoints;
		//moving speed
		public float speed = 3;
		//it move cyclic or not
		public bool cyclic;
		//wait time before move to the next point
		public float waitTime = 1;
		[Range(0, 2)]
		//the smooth moving
		public float easeAmount;
		public AudioClip soundEndPoint;
		public AudioSource ASource;

		[Header("Draw line")]
		public bool drawLine = false;
		public float lineWidth = 0.2f;
		public Material lineMat;
		public float offsetLineZ = -1;
		LineRenderer lineRen;

		int fromWaypointIndex;
		float percentBetweenWaypoints;
		float nextMoveTime;
		public bool isFinishedWays { get; set; }

		public bool allowMoving { get; set; }

		[ReadOnly] public bool isStop = true;

		//Control moving by the PlatformControllerSwitcher script
		[ReadOnly] public bool isManualControl = false;

		public void Start()
		{
			allowMoving = autoMove;

			if (Paths)
			{
				//find all the child object in the Path object
				int childs = Paths.transform.childCount;
				//reset the waypoints
				objectLocalWaypoints.Clear();
				if (childs > 0)
				{
					for (int i = 0; i < childs; i++)
					{
						//Add all the child object in the list
						if (Paths.transform.GetChild(i).gameObject.activeInHierarchy)
							objectLocalWaypoints.Add(Paths.transform.GetChild(i));
					}
				}
			}
			//only moving when the point number > 2
			if (objectLocalWaypoints.Count >= 2)
			{
				localWaypoints.Clear();
				for (int i = 0; i < objectLocalWaypoints.Count; i++)
				{
					//add all the child object to the list
					localWaypoints.Add(objectLocalWaypoints[i].localPosition);
				}
			}
			//init the global list
			globalWaypoints = new Vector3[localWaypoints.Count];
			for (int i = 0; i < localWaypoints.Count; i++)
			{
				//change the local to world postion
				globalWaypoints[i] = localWaypoints[i] + transform.position;
			}
			//begin moving
			Invoke("Play", delayStart);
		}

		void Play()
		{
			if (isManualControl)
				return;
			//alow moving
			isStop = false;
			//save the next moving time
			nextMoveTime = Time.time + waitTime;
		}

		float oldPercent;

		void LateUpdate()
		{
			if (!allowMoving)
				return;
			//save the current percent value
			oldPercent = percentBetweenWaypoints;
			//caculating the new velocity
			Vector3 velocity = CalculatePlatformMovement();

			if ((!moveWhenPlayer || isStanding) && !isStop)
			{
				//move the object
				transform.Translate(velocity, Space.World);
			}
			else if ((moveWhenPlayer && !isStanding) || isStop)
			{
				//save the curent percent of moving
				percentBetweenWaypoints = oldPercent;
			}
			//look at the next target point
			if (lookAtNextPoint)
				Look();
		}
		int toWaypointIndex;
		public void Look()
		{
			if (lookTempStop)
				return;
			//if there is no target to look, exit
			if (toWaypointIndex >= globalWaypoints.Length)
				return;
			//if already reaaced to the final target, no look
			if (fromWaypointIndex >= globalWaypoints.Length)
				return;

			if (globalWaypoints[fromWaypointIndex] == globalWaypoints[toWaypointIndex])
				return;
			//get the offset from the current target to the next target
			Vector3 diff = globalWaypoints[toWaypointIndex] - globalWaypoints[fromWaypointIndex];
			//normal the vector to 1
			diff.Normalize();
			float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
			float finalZ = Mathf.Lerp(transform.rotation.eulerAngles.z, rot_z, speedLook);
			//if reverse look
			float offsetReverse = isReverseMoving ? 180 : 0;
			//rotate the object to look at the target
			transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, finalZ + offsetReverse + lookAtOffset);
		}

		float Ease(float x)
		{
			float a = easeAmount + 1;
			return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
		}

		Vector3 CalculatePlatformMovement()
		{
			if (followTarget)
			{
				//return the offset vector
				return followTarget.position - transform.position;
			}
			//wait for the moving next point
			if (Time.time < nextMoveTime)
			{
				return Vector3.zero;
			}
			//Caculating the moving point and speed
			fromWaypointIndex %= globalWaypoints.Length;
			toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
			float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
			percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
			percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
			float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);
			//Get the new position
			Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

			//if reached to the target, move to the next point
			if (percentBetweenWaypoints >= 1)
			{
				percentBetweenWaypoints = 0;
				fromWaypointIndex++;
				//if no loop, stop moving and disable the script
				if (!isLoop && (fromWaypointIndex >= globalWaypoints.Length - 1))
				{
					isFinishedWays = true;
					enabled = false;
				}
				else if (!cyclic)
				{
					//set the new target
					if (fromWaypointIndex >= globalWaypoints.Length - 1)
					{
						//play the sound when reach to the final point
						if (ASource)
						{
							ASource.clip = soundEndPoint;
							ASource.volume = 1;
							ASource.Play();
						}
						fromWaypointIndex = 0;
						System.Array.Reverse(globalWaypoints);
						//reserse the moving of the object
						isReverseMoving = !isReverseMoving;
						//stop looking
						lookTempStop = true;
						//call loop action
						Invoke("AllowLookAgain", 0.1f);
					}
				}
				//Wait for next move
				nextMoveTime = Time.time + waitTime;
			}

			return newPos - transform.position;
		}

		bool lookTempStop = false;
		void AllowLookAgain()
		{
			//allow the object look at the next target
			lookTempStop = false;
		}

		//This just drawn the information on the Scene
		void OnDrawGizmos()
		{
			if (followTarget)
			{
				if (!Application.isPlaying)
				{
					transform.position = followTarget.position;
					if (lineRen)
						DestroyImmediate(lineRen);
				}

				return;
			}

			if (Paths)
			{
				int childs = Paths.transform.childCount;
				objectLocalWaypoints.Clear();
				if (childs > 0)
				{
					for (int i = 0; i < childs; i++)
					{
						if (Paths.transform.GetChild(i).gameObject.activeInHierarchy)
							objectLocalWaypoints.Add(Paths.transform.GetChild(i));
					}
				}
			}

			if (objectLocalWaypoints.Count >= 2)
			{
				localWaypoints.Clear();
				for (int i = 0; i < objectLocalWaypoints.Count; i++)
				{
					if (objectLocalWaypoints[i] == null)
						objectLocalWaypoints.RemoveAt(i);
					else
						localWaypoints.Add(objectLocalWaypoints[i].localPosition);
				}
			}

			if (!Application.isPlaying)
			{
				lineRen = GetComponent<LineRenderer>();

				if (drawLine)
				{
					if (!lineRen)
						lineRen = gameObject.AddComponent<LineRenderer>();

					lineRen.positionCount = localWaypoints.Count;
					lineRen.useWorldSpace = true;
					lineRen.startWidth = lineWidth;
					lineRen.material = lineMat;
					lineRen.textureMode = LineTextureMode.Tile;

					for (int i = 0; i < localWaypoints.Count; i++)
						lineRen.SetPosition(i, localWaypoints[i] + transform.position + Vector3.forward * offsetLineZ);

				}
				else if (lineRen)
					DestroyImmediate(lineRen);
			}

			if (localWaypoints != null && this.enabled)
			{
				for (int i = 0; i < localWaypoints.Count; i++)
				{
					Gizmos.color = Color.red;
					float size = .3f;
					Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
					if ((i + 1 >= localWaypoints.Count) && !isLoop && !cyclic)
						Gizmos.DrawWireCube(globalWaypointPos, Vector3.one * 0.5f);
					else
						Gizmos.DrawWireSphere(globalWaypointPos, size);

					if (i + 1 >= localWaypoints.Count)
					{
						if (cyclic && isLoop)
						{
							Gizmos.color = Color.yellow;
							if (Application.isPlaying)
								Gizmos.DrawLine(globalWaypoints[i], globalWaypoints[0]);
							else
								Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[0] + transform.position);
						}
						return;
					}

					Gizmos.color = Color.green;
					if (Application.isPlaying)
						Gizmos.DrawLine(globalWaypoints[i], globalWaypoints[i + 1]);
					else
						Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[i + 1] + transform.position);
				}
			}
		}
	}
}