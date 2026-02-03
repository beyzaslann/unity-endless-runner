using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LanePatternSpawner : MonoBehaviour
{
    public Transform player;

    [Header("Lane")] 
    public float laneDistance = 2f; 
    public float spawnZAhead = 45f; 
    
    [Header("Spacing (Difficulty)")] 
    public float startSegmentStepZ = 12f; 
    public float minSegmentStepZ = 7f; 
    public float stepDecreasePerSecond = 0.15f; // zamanla sýklaþsýn

    [Header("Chances (Difficulty)")]
    [Range(0, 1)] public float startObstacleChance = 0.75f; 
    [Range(0, 1)] public float maxObstacleChance = 0.95f; 
    public float obstacleChanceIncreasePerSecond = 0.01f; 

    [Range(0, 1)] public float coinChance = 0.85f;

    [Header("Heights")]
    public float hitY = 0.5f;
    public float jumpY = 0.35f;
    public float slideY = 2.0f;   // SlideUnder genelde yukarýda durur (bar gibi)
    public float coinY = 1.0f;

    [Header("Coin Patterns")] 
    public float coinGapZ = 2.0f; // coinler arasý mesafe
    public int tripleCount = 3; // 3'lü sýra
    public int laneStripeCount = 5; // 3 lane þerit uzunluðu
    public int zigzagCount = 6; // zigzag uzunluðu

    [Header("Despawn (Pooling)")] 
    public float despawnBehindDistance = 40f; 

    readonly List<SpawnedItem> alive = new List<SpawnedItem>(256); 
    int cleanupIndex = 0; // her frame baþtan taramamak için

    float nextSpawnZ; 
    float segmentStepZ; 
    float obstacleChance; 
    
    // Pattern tekrarýný engellemek için
    int lastPatternIndex = -1;

    bool forceEasyNext = false; 
    
    void Start() 
    { 
        segmentStepZ = startSegmentStepZ; 
        obstacleChance = startObstacleChance; 
        
        if (player != null) 
            nextSpawnZ = player.position.z + spawnZAhead; 
    }

    void Update()
    {
        if (GameManagerRunner.Instance == null) return; 
        if (!GameManagerRunner.Instance.isRunning || GameManagerRunner.Instance.isGameOver) return; 
        if (player == null) return; 
        
        // Zorluk: zamanla engel art, aralýk azalsýn
        segmentStepZ = Mathf.Max(minSegmentStepZ, segmentStepZ - stepDecreasePerSecond * Time.deltaTime); 
        obstacleChance = Mathf.Min(maxObstacleChance, obstacleChance + obstacleChanceIncreasePerSecond * Time.deltaTime); 
        
        // Spawn
        if (player.position.z + spawnZAhead > nextSpawnZ) 
        { 
            SpawnSet(nextSpawnZ); nextSpawnZ += segmentStepZ; 
        } 
        CleanupBehindPlayer(); 
    }

    void CleanupBehindPlayer()
    {
        if (player == null) return; 
        float cutoffZ = player.position.z - despawnBehindDistance; 
        
        // Her frame tüm listeyi taramak yerine parça parça tarýyoruz
        int checksThisFrame = 30; // performans ayarý
        int checkedCount = 0; 
        
        while (alive.Count > 0 && checkedCount < checksThisFrame && cleanupIndex < alive.Count) 
        { 
            var item = alive[cleanupIndex]; 
            
            if (item.go == null) 
            { 
                alive.RemoveAt(cleanupIndex); continue; 
            } 
            
            if (item.go.transform.position.z < cutoffZ) 
            { 
                SimplePool.Instance.Despawn(item.key, item.go); 
                alive.RemoveAt(cleanupIndex); 
                continue; 
            } 
            cleanupIndex++; 
            checkedCount++; 
        } 
        // index sona geldiyse baþa sar
        if (cleanupIndex >= alive.Count) cleanupIndex = 0; }

    void SpawnSet(float z)
    { 
        // Geçilebilir obstacle patternler (en az 1 boþ lane þart) 
        // 1=engel, 0=boþ
        int[][] patterns = 
        { 
            new[]{1,0,0}, 
            new[]{0,1,0}, 
            new[]{0,0,1}, 
            new[]{1,0,1}, 
            new[]{1,1,0}, 
            new[]{0,1,1}, 
            new[]{0,0,0}, // nefes
        };


        // Ayný pattern üst üste gelmesin diye seç
        int patternIndex;
        int[] chosen; 
        
        // Eðer önceki set 2 engelse, bu seti kolay seç
        if (forceEasyNext) 
        { 
            // Easy patternler: tek engel veya hiç engel // index'ler: 0,1,2 (tek engel), 6 (0,0,0)
            int[] easyIndices = { 0, 1, 2, 6 }; 
            
            patternIndex = easyIndices[Random.Range(0, easyIndices.Length)];

            // Üst üste ayný easy gelmesin diye basit kontrol
            if (patternIndex == lastPatternIndex)
                patternIndex = easyIndices[(System.Array.IndexOf(easyIndices, patternIndex) + 1) % easyIndices.Length];

            lastPatternIndex = patternIndex; 
            chosen = patterns[patternIndex];

            forceEasyNext = false; // zorunlu kolaylýðý tükettik
        } 
        else 
        { 
            patternIndex = PickPatternIndex(patterns.Length); 
            chosen = patterns[patternIndex]; 
        }

        // "Ýmkânsýz set" güvenliði: 
        // Eðer bir önceki set ile birleþince oyuncuya tepki süresi kalmayacaksa, 
        // (ör: aralýk çok küçükken 2 lane engel + 2 lane engel) => nefes pattern'e zorla. 
        // Basit kural: segmentStepZ çok küçükse ve chosen iki engelse, ara ara boþ ekle.
        int obstacleCount = chosen[0] + chosen[1] + chosen[2]; 
        // Eðer bu set 2 engelse, bir sonraki seti zorunlu kolay yap
        if (obstacleCount >= 2) forceEasyNext = true; 
        if (segmentStepZ <= (minSegmentStepZ + 0.5f) && obstacleCount >= 2 && Random.value < 0.5f) 
        { 
            chosen = patterns[6]; // 0,0,0
        }

        // Eðer chosen nefes setine döndüyse, forceEasyNext'i iptal et
        if (chosen[0] + chosen[1] + chosen[2] < 2)
            forceEasyNext = false;

        // Engel spawn
        if (Random.value < obstacleChance) 
        { 
            for (int lane = 0; lane < 3; lane++) 
            { 
                if (chosen[lane] == 1) 
                {
                    string key = PickObstacleKey();
                    float y = GetObstacleY(key);

                    Vector3 pos = new Vector3(LaneToX(lane), y, z);
                    var go = SimplePool.Instance.Spawn(key, pos, Quaternion.identity);
                    if (go != null) alive.Add(new SpawnedItem(key, go));
                }
            } 
        }

        float GetObstacleY(string key)
        {
            // Pool key'lerine göre Y seçiyoruz
            switch (key)
            {
                case "ObstacleHit": return hitY;
                case "ObstacleJump": return jumpY;
                case "ObstacleSlide": return slideY;
                default: return hitY;
            }
        }

        // Coin spawn: çeþitlendirme
        if (Random.value < coinChance) 
        { 
            // Coin’ler engellerin olmadýðý “güvenli” lane’lere göre karar verir
            int safeLane = PickSafeLane(chosen); 
            
            // 0: triple, 1: lane stripe, 2: zigzag
            int coinPattern = Random.Range(0, 3); switch (coinPattern) 
            { 
                case 0: 
                    SpawnTripleCoins(safeLane, z); 
                    break; 
                case 1: 
                    SpawnLaneStripeCoins(chosen, z); 
                    break; 
                case 2: 
                    SpawnZigZagCoins(z); 
                    break; 
            } 
        } 
    }

    string PickObstacleKey()
    {
        float r = Random.value; 
        
        // örnek daðýlým: %50 Hit, %30 Jump, %20 Slide
        if (r < 0.5f) return "ObstacleHit"; 
        if (r < 0.8f) return "ObstacleJump"; 
        return "ObstacleSlide"; } 
    
    int PickPatternIndex(int count) 
    { 
        
        // Ayný pattern art arda gelmesin
        int idx = Random.Range(0, count); 
        if (idx == lastPatternIndex) idx = (idx + Random.Range(1, count)) % count; 
        
        lastPatternIndex = idx; 
        return idx; 
    }

    void SpawnTripleCoins(int lane, float z) 
    { 
        for (int i = 0; i < tripleCount; i++) 
        { 
            Vector3 pos = new Vector3(LaneToX(lane), coinY, z + (i * coinGapZ)); 
            var go = SimplePool.Instance.Spawn("Coin", pos, Quaternion.identity); 
            alive.Add(new SpawnedItem("Coin", go)); 
        } 
    }

    void SpawnLaneStripeCoins(int[] obstaclePattern, float z)
    { 
        // 3 lane boyunca kýsa bir "þerit" yap: 
        // Engelin olduðu lane'e coin koyma, diðer lane'lere koy.
        for (int i = 0; i < laneStripeCount; i++) 
        { 
            float zz = z + i * coinGapZ; 
            for (int lane = 0; lane < 3; lane++) 
            { 
                if (obstaclePattern[lane] == 0) 
                { 
                    Vector3 pos = new Vector3(LaneToX(lane), coinY, zz); 
                    var go = SimplePool.Instance.Spawn("Coin", pos, Quaternion.identity); 
                    alive.Add(new SpawnedItem("Coin", go)); 
                } 
            } 
        } 
    }

    void SpawnZigZagCoins(float z)
    { 
        // Lane'ler arasýnda zikzak (0-1-2-1-0...)
        int[] zig = { 0, 1, 2, 1 }; 

        for (int i = 0; i < zigzagCount; i++) 
        { 
            int lane = zig[i % zig.Length]; 
            Vector3 pos = new Vector3(LaneToX(lane), coinY, z + i * coinGapZ); 
            var go = SimplePool.Instance.Spawn("Coin", pos, Quaternion.identity); 
            alive.Add(new SpawnedItem("Coin", go)); 
        } 
    }
    int PickSafeLane(int[] obstaclePattern)
    { 
        // Önce boþ lane ara
        for (int lane = 0; lane < 3; lane++) 
            if (obstaclePattern[lane] == 0) return lane; 

        return 1; 
    } 
    
    float LaneToX(int laneIndex) 
    { 
        return (laneIndex - 1) * laneDistance; 
    }
}