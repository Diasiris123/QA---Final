using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// This script contains the list of objects, which should be pooled. When receiving the command, it returns the object. 
/// If the object is not on the list, it creates the new object.
/// </summary>
[System.Serializable]
public class PoolingObjects
{
    public GameObject pooledPrefab;
    public int count;
}

public class PoolingController : MonoBehaviour
{
    [Tooltip("Your 'pooling' objects. Add new element and add the prefab to create the object prefab")]
    public PoolingObjects[] poolingObjectsClass;

    // The list where 'pooling' objects will be stored
    private List<GameObject> pooledObjectsList = new List<GameObject>();

    public static PoolingController instance; // Unique class instance for easy access

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        CreateNewList(); // Create the new list of 'pooling' objects
    }

    void CreateNewList()
    {
        for (int i = 0; i < poolingObjectsClass.Length; i++) // For each prefab, create the needed amount of objects and deactivate them
        {
            for (int k = 0; k < poolingObjectsClass[i].count; k++)
            {
                GameObject newObj = Instantiate(poolingObjectsClass[i].pooledPrefab, transform);
                newObj.name = poolingObjectsClass[i].pooledPrefab.name + "(Clone)";
                pooledObjectsList.Add(newObj);
                newObj.SetActive(false);
            }
        }
    }

    public GameObject GetPoolingObject(GameObject prefab) // Looking for the needed object by prefab name and return it
    {
        string cloneName = GetCloneName(prefab);
        for (int i = 0; i < pooledObjectsList.Count; i++)
        {
            if (!pooledObjectsList[i].activeSelf && pooledObjectsList[i].name == cloneName)
            {
                return pooledObjectsList[i];
            }
        }
        return AddNewObject(prefab); // If there is no object available, create a new one
    }

    GameObject AddNewObject(GameObject prefab) // Create a new object and add it to the list
    {
        GameObject newObj = Instantiate(prefab, transform);
        newObj.name = prefab.name + "(Clone)";
        pooledObjectsList.Add(newObj);
        newObj.SetActive(false);
        return newObj;
    }

    string GetCloneName(GameObject prefab)
    {
        return prefab.name + "(Clone)";
    }

    // ✅ This method helps testing — counts all pooled objects by name
    public int CountPooledObjectsByName(string name)
    {
        return pooledObjectsList.Count(obj => obj.name == name);
    }
}
