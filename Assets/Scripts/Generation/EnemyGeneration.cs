using System.Collections;
using Game;
using UnityEngine;
using UnityEngine.AI;

namespace Generation
{
    public class EnemyGeneration : MonoBehaviour
    {
        public static EnemyGeneration Instance;
        public EnemyManager m_EnemyManager;
        public EnemyController[] Enemies;
        public int ExtraEnemySpawnCount = 0;

        public void Awake()
        {
            Events.Events.OnLevelRoomsGenerated += GenerateEnemies;
            Events.Events.OnLevelRoomsAlreadyGenerated += GenerateEnemies;
            Events.Events.OnLevelFinish += DestroyAllEnemies;
            Events.Events.OnLevelPatrolPathsGenerated += AssignPatrolPaths;
            Events.Events.OnLevelFinish += IncreaseEmenyCount;
        }

        public void Start()
        {
            if (Instance == null) Instance = this;
            else
            {
                Debug.LogError($"A {typeof(EnemyGeneration).Name} script already exists within the scene!");
                return;
            }

            m_EnemyManager = FindObjectOfType<EnemyManager>();
            DebugUtility.HandleErrorIfNullFindObject<EnemyManager, EnemyController>(m_EnemyManager, this);
        }

        /// <summary>
        /// Generate all the enemies for the level
        /// </summary>
        public static void GenerateEnemies()
        {
            Instance.StartCoroutine(GenerateEnemies(!GameFlowManager.IsGameStarted));
        }

        /// <summary>
        /// Generate all the enemies for the level
        /// </summary>
        public static IEnumerator GenerateEnemies(bool forceGenerate)
        {
            if (forceGenerate) yield break;

            int enemySpawnCount = (RoomGeneration.CurrentRooms.Count / 2) + Instance.ExtraEnemySpawnCount;
            for (int i = 0; i < enemySpawnCount; i++)
            {
                SpawnPoint spawnPoint = GetRandomSpawnPoint();
                if (spawnPoint == null) break;

                Vector3 position = spawnPoint.SpawnPointTransform.position;
                if (NavMesh.SamplePosition(position, out NavMeshHit hit, 500.0f, NavMesh.AllAreas))
                {
                    position = hit.position;
                }

                EnemyController eC = GetRandomEnemy();
                if (eC == null) continue;
                Instantiate(eC, position, Quaternion.identity);
                spawnPoint.HasSpawned = true;
                if (SpawnPoint.AllNonUsedSpawnPoints.Contains(spawnPoint)) SpawnPoint.AllNonUsedSpawnPoints.Remove(spawnPoint);
            }

            yield return new WaitForEndOfFrame(); // We need to wait fro the enemy to do their Start() method to init everything needed

            Events.Events.OnOnLevelEnemiesGenerated();
        }

        /// <summary>
        /// Assign patrol paths to all enemies
        /// </summary>
        public static void AssignPatrolPaths()
        {
            if (PatrolGeneration.AllPatrolPaths.Count == 0) return;
            foreach (var enemy in Instance.m_EnemyManager.enemies)
            {
                PatrolPath patrolPath = PatrolGeneration.AllPatrolPaths[SeedManager.EnemyGenerationRandom.Next(0, PatrolGeneration.AllPatrolPaths.Count)];
                patrolPath.enemiesToAssign.Add(enemy);
                enemy.patrolPath = patrolPath;
            }

            Events.Events.OnOnLevelCompletelyFinishedGenerating();
        }

        /// <summary>
        /// Destroys all the enemies in the level
        /// </summary>
        public void DestroyAllEnemies()
        {
            foreach (var enemy in m_EnemyManager.enemies)
            {
                if (enemy) DestroyImmediate(enemy.gameObject);
            }
            m_EnemyManager.enemies.Clear();
        }

        /// <summary>
        /// Returns a random enemy
        /// </summary>
        /// <returns></returns>
        public static EnemyController GetRandomEnemy()
        {
            return Instance.Enemies[SeedManager.EnemyGenerationRandom.Next(0, Instance.Enemies.Length)];
        }

        /// <summary>
        /// Returns a random spawn point
        /// </summary>
        /// <returns></returns>
        public static SpawnPoint GetRandomSpawnPoint()
        {
            if (SpawnPoint.AllNonUsedSpawnPoints.Count == 0) return null;
            SpawnPoint spawnPoint;
            int con = 0;
            do
            {
                spawnPoint = SpawnPoint.AllNonUsedSpawnPoints[SeedManager.EnemyGenerationRandom.Next(0, SpawnPoint.AllNonUsedSpawnPoints.Count - 1)];
                con++;
            } while (spawnPoint.HasSpawned && con < 10);

            return spawnPoint;
        }

        /// <summary>
        /// Increase the enemy count on a level
        /// </summary>
        public static void IncreaseEmenyCount()
        {
            Instance.ExtraEnemySpawnCount++;
        }

        private void OnDestroy()
        {
            Events.Events.OnLevelRoomsGenerated -= GenerateEnemies;
            Events.Events.OnLevelRoomsAlreadyGenerated -= GenerateEnemies;
            Events.Events.OnLevelFinish -= DestroyAllEnemies;
            Events.Events.OnLevelPatrolPathsGenerated -= AssignPatrolPaths;
            Events.Events.OnLevelFinish -= IncreaseEmenyCount;
        }
    }
}
