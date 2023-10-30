using UnityEngine;
using System.Collections;
namespace PhoenixaStudio
{
	public class AutoRotate : MonoBehaviour
	{
		public float speed = 100;
		public Vector3 axis = new Vector3(0, 0, 1);
		void Update()
		{
			//rotate the object around the z axis
			//transform.RotateAround(transform.position, axis, speed * Time.deltaTime);

			transform.Rotate(axis * speed * Time.deltaTime, Space.Self);
		}
	}
}