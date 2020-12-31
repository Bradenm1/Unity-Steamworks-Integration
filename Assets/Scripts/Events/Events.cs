
namespace Events
{
    public class Events
    {
        public delegate void EventHandler(); // Base event type which returns a void and has no parameters.

        public static event EventHandler OnSceneStart; // When the application runs for the first time
        public static event EventHandler OnGameStart; // When the game starts for the first time
        public static event EventHandler OnGameEnd; // When the game end. E.g - Game Over screen.
        public static event EventHandler OnLevelFinish; // When the level has finished
        public static event EventHandler OnLevelStart; // When the level has started
        public static event EventHandler OnLevelRoomsGenerated; // When rooms have been generated for a level
        public static event EventHandler OnLevelRoomsAlreadyGenerated; // When rooms have been generated for a level
        public static event EventHandler OnLevelEnemiesGenerated; // When the enemies has been generated for a level
        public static event EventHandler OnLevelPatrolPathsGenerated; // When the paths has been generated for a level
        public static event EventHandler OnLevelCompletelyFinishedGenerating; // When the enemies has been generated for a level
        public static event EventHandler OnEnemyKill; // When the player kills an enemy
        public static event EventHandler OnPlayerDeath; // When the player dies

        /// <summary>
        /// All the invoke method calls for the above events
        /// </summary>

        public static void OnOnLevelStart()
        {
            OnLevelStart?.Invoke();
        }

        public static void OnOnLevelFinish()
        {
            OnLevelFinish?.Invoke();
        }

        public static void OnOnGameEnd()
        {
            OnGameEnd?.Invoke();
        }

        public static void OnOnGameStart()
        {
            OnGameStart?.Invoke();
        }

        public static void OnOnSceneStart()
        {
            OnSceneStart?.Invoke();
        }

        public static void OnOnLevelRoomsGenerated()
        {
            OnLevelRoomsGenerated?.Invoke();
        }

        public static void OnOnLevelEnemiesGenerated()
        {
            OnLevelEnemiesGenerated?.Invoke();
        }

        public static void OnOnLevelRoomsAlreadyGenerated()
        {
            OnLevelRoomsAlreadyGenerated?.Invoke();
        }

        public static void OnOnLevelPatrolPathsGenerated()
        {
            OnLevelPatrolPathsGenerated?.Invoke();
        }

        public static void OnOnLevelCompletelyFinishedGenerating()
        {
            OnLevelCompletelyFinishedGenerating?.Invoke();
        }

        public static void OnOnEnemyKill()
        {
            OnEnemyKill?.Invoke();
        }

        public static void OnOnPlayerDeath()
        {
            OnPlayerDeath?.Invoke();
        }
    }
}
