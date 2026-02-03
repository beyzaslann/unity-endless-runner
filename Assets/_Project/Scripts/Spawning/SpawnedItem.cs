using UnityEngine;

[System.Serializable]
public class SpawnedItem
{
    public string key;
    public GameObject go;

    public SpawnedItem(string key, GameObject go)
    {
        this.key = key;
        this.go = go;
    }
}
