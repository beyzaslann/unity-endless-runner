using System.Collections.Generic;
using UnityEngine;

public class TrackManager : MonoBehaviour
{
    public Transform player;
    public GameObject trackPrefab;

    public int initialPieces = 6;
    public float pieceLength = 30f;
    public float spawnAheadDistance = 90f;

    private float nextSpawnZ = 0f;
    private readonly Queue<GameObject> pieces = new Queue<GameObject>();

    void Start()
    {
        // Ýlk parçalarý döþe
        for (int i = 0; i < initialPieces; i++)
            SpawnPiece();
    }

    void Update()
    {
        if (player == null || trackPrefab == null) return;

        // Oyuncunun ilerisinde yeterli yol yoksa yeni parça ekle
        if (player.position.z + spawnAheadDistance > nextSpawnZ)
        {
            SpawnPiece();
            RemoveOldestIfNeeded();
        }
    }

    void SpawnPiece()
    {
        GameObject piece = Instantiate(trackPrefab, new Vector3(0f, -0.5f, nextSpawnZ), Quaternion.identity);
        pieces.Enqueue(piece);
        nextSpawnZ += pieceLength;
    }

    void RemoveOldestIfNeeded()
    {
        // Kuyruk çok büyümesin
        while (pieces.Count > initialPieces + 2)
        {
            var old = pieces.Dequeue();
            Destroy(old);
        }
    }
}
