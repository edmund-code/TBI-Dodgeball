using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [Tooltip("Reference to the enemy prefab (a simple gray body).")]
    public GameObject enemyPrefab;

    [Tooltip("The maximum number of enemies that can exist at once.")]
    public int maxEnemies = 10;

    [Header("Spawn Area Settings")]
    [Tooltip("The size of the rectangular area in which enemies can spawn.")]
    public Vector3 areaSize = new Vector3(20f, 0f, 20f);

    [Header("Spawning Settings")]
    [Tooltip("Time interval (in seconds) between spawn checks.")]
    public float spawnInterval = 2f;

    // Internal timer to track spawning
    private float spawnTimer = 0f;

    void Update()
    {
        // Accumulate time
        spawnTimer += Time.deltaTime;

        // Check if it's time to attempt spawning
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f; // reset the timer
            AttemptSpawnEnemy();
        }
    }

    void AttemptSpawnEnemy()
    {
        // Count how many enemies with the tag "Enemy" are in the scene
        // Make sure your enemy prefab has the tag "Enemy" set in the Inspector.
        int currentEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        // Only spawn if we haven't reached the limit
        if (currentEnemyCount < maxEnemies)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        // Calculate random position within the defined area
        Vector3 randomPosition = new Vector3(
            transform.position.x + Random.Range(-areaSize.x / 2f, areaSize.x / 2f),
            transform.position.y + Random.Range(-areaSize.y / 2f, areaSize.y / 2f),
            transform.position.z + Random.Range(-areaSize.z / 2f, areaSize.z / 2f)
        );

        // Instantiate the enemy
        GameObject newEnemy = Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
        Debug.Log("Enemy Spawned");
        
        // Make sure the enemy has the "Enemy" tag (optional but recommended for easy tracking)
        newEnemy.tag = "Enemy";
    }

    // Optional: Draw a wireframe box in the editor to visualize the spawn area
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, areaSize);
    }
}
