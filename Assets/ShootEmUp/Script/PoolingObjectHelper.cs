using System.Collections.Generic;
using UnityEngine;
namespace PhoenixaStudio
{
	public class PoolingObjectHelper : MonoBehaviour
	{
		static private PoolingObjectHelper Instance;

		void Awake()
		{
			Instance = this;
		}

		void Start()
		{
			for (int i = 0; i < poolingObjects.Length; i++)
			{
				LoadingObjects(poolingObjects[i], amounts[i]);
			}
		}

		static public GameObject GetTheObject(GameObject objectPrefab, Vector3 position, bool activateObject = true)
		{
			int specialID = objectPrefab.GetInstanceID();

			if (!Instance.pointerOfPool.ContainsKey(specialID))
			{
				Debug.LogError("Object no in the list, place the prefab!");
				return null;
			}

			int cursor = Instance.pointerOfPool[specialID];
			Instance.pointerOfPool[specialID]++;
			if (Instance.pointerOfPool[specialID] >= Instance.CreatedObjects[specialID].Count)
			{
				Instance.pointerOfPool[specialID] = 0;
			}

			GameObject returnObj = Instance.CreatedObjects[specialID][cursor];
			returnObj.transform.position = position;
			if (activateObject)
				if (returnObj)
					returnObj.SetActive(true);

			return returnObj;
		}

		static public void LoadingObjects(GameObject sourceObj, int amount = 1)
		{
			Instance.PlusNewObjecs(sourceObj, amount);
		}

		private void PlusNewObjecs(GameObject sourceObject, int number)
		{
			int specialID = sourceObject.GetInstanceID();

			if (!CreatedObjects.ContainsKey(specialID))
			{
				CreatedObjects.Add(specialID, new List<GameObject>());
				pointerOfPool.Add(specialID, 0);
			}

			GameObject addObject;
			for (int i = 0; i < number; i++)
			{
				addObject = (GameObject)Instantiate(sourceObject, new Vector2(0, 100), sourceObject.transform.rotation);
				addObject.SetActive(false);
				CreatedObjects[specialID].Add(addObject);
			}
		}

		public GameObject[] poolingObjects = new GameObject[0];
		public int[] amounts = new int[0];
		private Dictionary<int, List<GameObject>> CreatedObjects = new Dictionary<int, List<GameObject>>();
		private Dictionary<int, int> pointerOfPool = new Dictionary<int, int>();
	}
}