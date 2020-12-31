using UI;
using UnityEngine;

namespace Game
{
    public class SeedManager : MonoBehaviour
    {
        private static System.Random _baseRandom; // The base random
        public static System.Random RoomGenerationRandom;
        public static System.Random EnemyGenerationRandom;
        public static System.Random PatrolPathGenerationRandom;

        // The seed random for the random
        private static readonly int _seedRange = 1000000;

        private void Start()
        {
            SetRandomSeed();
        }

        /// <summary>
        /// Set seeds as random
        /// </summary>
        public static void SetRandomSeed()
        {
            int seed = UnityEngine.Random.Range(-_seedRange, _seedRange);
            StartMenu.SetSeedText(seed);
            SeedBaseRandom(seed);
            SeedAllRandoms();
        }

        /// <summary>
        /// Updates the seeds given the startmenu seed
        /// </summary>
        public static void UpdateSeededRandoms()
        {
            SeedBaseRandom(StartMenu.GetSeed());
            SeedAllRandoms();
        }

        /// <summary>
        /// Sets the base randoms seed 
        /// </summary>
        private static void SeedBaseRandom()
        {
            SeedBaseRandom(UnityEngine.Random.Range(-_seedRange, _seedRange));
        }

        /// <summary>
        /// Sets the base randoms seed 
        /// </summary>
        private static void SeedBaseRandom(int seed)
        {
            _baseRandom = new System.Random(seed);
        }

        /// <summary>
        /// Sets all the randoms seeds
        /// </summary>
        private static void SeedAllRandoms()
        {
            RoomGenerationRandom = new System.Random(GetNextSeed());
            EnemyGenerationRandom = new System.Random(GetNextSeed());
            PatrolPathGenerationRandom = new System.Random(GetNextSeed());
        }

        /// <summary>
        /// Gets the next seed in the base random
        /// </summary>
        /// <returns></returns>
        private static int GetNextSeed()
        {
            return _baseRandom.Next(-_seedRange, _seedRange);
        }
    }
}
