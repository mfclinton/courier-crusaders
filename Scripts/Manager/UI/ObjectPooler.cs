using System.Collections.Generic;
using UnityEngine;

namespace Manager
{
    public class ObjectPooler : MonoBehaviour
    {
        public static ObjectPooler Instance;
        public int defaultPoolSize = 10;

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            Instance = this;
            poolDictionary = new Dictionary<string, Queue<GameObject>>();
        }

        private void PopulateQueue(GameObject prefab, int size, string tag)
        {
            for (int i = 0; i < size; i++)
            {
                Queue<GameObject> pool = poolDictionary[tag]; 
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                pool.Enqueue(obj);
            }
        }

        public GameObject SpawnFromPool(GameObject prefab, Vector3 position = default, Quaternion rotation = default, Transform parent = null, int childIndex = -1)
        {
            string tag = prefab.tag;

            // Create queue if it doesn't exist
            if (!poolDictionary.ContainsKey(tag))
            {
                // If pool doesn't exist, create it
                Queue<GameObject> newPool = new Queue<GameObject>();
                poolDictionary.Add(tag, newPool);

                // Populate queue
                PopulateQueue(prefab, defaultPoolSize, tag);
            }

            GameObject objectToSpawn;
            poolDictionary[tag].TryDequeue(out objectToSpawn);

            if (objectToSpawn != null && !objectToSpawn.activeInHierarchy)
            {
                objectToSpawn.SetActive(true);
                objectToSpawn.transform.position = position;
                objectToSpawn.transform.rotation = rotation;
                
                if (parent != null)
                {
                    // Assumes object is part of UI
                    objectToSpawn.transform.SetParent(parent, false);
                }

                // poolDictionary[tag].Enqueue(objectToSpawn);
                if (childIndex != -1)
                    objectToSpawn.transform.SetSiblingIndex(childIndex);

                return objectToSpawn;
            }
            else
            {
                GameObject newObject = Instantiate(prefab, position, rotation, parent);
                
                if (childIndex != -1)
                    newObject.transform.SetSiblingIndex(childIndex);
                
                poolDictionary[tag].Enqueue(newObject);

                return newObject;
            }
        }

        public void ReturnToPool(GameObject objectToReturn)
        {
            string tag = objectToReturn.tag;

            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning("Pool with tag " + tag + " doesn't exist.");
                Debug.LogWarning($"Destroying object instead {objectToReturn.name}.");
                Destroy(objectToReturn); // Destroying object
                return;
            }

            objectToReturn.SetActive(false);
            poolDictionary[tag].Enqueue(objectToReturn);
        }

    }
}
