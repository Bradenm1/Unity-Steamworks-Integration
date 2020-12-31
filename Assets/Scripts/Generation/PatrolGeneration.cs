using System.Collections.Generic;
using Game;
using UnityEngine;

namespace Generation
{
    public class PatrolGeneration : MonoBehaviour
    {
        public static List<PatrolPath> AllPatrolPaths = new List<PatrolPath>();
        public static int PatrolPaths = 2;
        public static int PatrolPathsPoints = 3;

        private void Awake()
        {
            Events.Events.OnLevelEnemiesGenerated += GeneratePatrolPaths;
            Events.Events.OnLevelFinish += DestroyAllPatrolPaths;
        }

        public static void GeneratePatrolPaths()
        {
            for (int i = 0; i < PatrolPaths; i++)
            {
                GameObject patrolPathObj = new GameObject("PatrolPath");
                PatrolPath patrolPath = patrolPathObj.AddComponent<PatrolPath>();
                AllPatrolPaths.Add(patrolPath);

                for (int o = 0; o < PatrolPathsPoints; o++)
                {
                    SpawnPoint spawnPoint = GetRandomPatrolPoint();;
                    if (spawnPoint) patrolPath.pathNodes.Add(spawnPoint.SpawnPointTransform);
                    else break;
                }
            }

            Events.Events.OnOnLevelPatrolPathsGenerated();
        }

        public static SpawnPoint GetRandomPatrolPoint()
        {
            if (SpawnPoint.AllSpawnPoints.Count == 0) return null;
            SpawnPoint spawnPoint;
            int con = 0;
            do
            {
                spawnPoint = SpawnPoint.AllSpawnPoints[SeedManager.PatrolPathGenerationRandom.Next(0, SpawnPoint.AllSpawnPoints.Count - 1)];
                con++;
            } while (spawnPoint == null && con < 10);

            return spawnPoint;
        }

        public static void DestroyAllPatrolPaths()
        {
            foreach (var patrolPath in AllPatrolPaths)
            {
                if (patrolPath) DestroyImmediate(patrolPath.gameObject);
            }
            AllPatrolPaths.Clear();
        }

        private void OnDestroy()
        {
            Events.Events.OnLevelEnemiesGenerated -= GeneratePatrolPaths;
        }
    }
}
