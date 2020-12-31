using System.Collections.Generic;
using UnityEngine;

namespace Generation
{
    public class SpawnPoint : MonoBehaviour
    {
        public static List<SpawnPoint> AllSpawnPoints = new List<SpawnPoint>();
        public static List<SpawnPoint> AllNonUsedSpawnPoints = new List<SpawnPoint>();
        public Transform SpawnPointTransform;
        public bool HasSpawned;

        public void Awake()
        {
            AllSpawnPoints.Add(this);
            AllNonUsedSpawnPoints.Add(this);
        }

        public void OnDestroy()
        {
            AllSpawnPoints.Remove(this);
            if (AllNonUsedSpawnPoints.Contains(this)) AllNonUsedSpawnPoints.Remove(this);
        }
    }
}
