using Game;
using TMPro;
using UnityEngine;

namespace UI
{
    public class StartMenu : MonoBehaviour
    {
        public static StartMenu Instance;
        public TMP_InputField SeedInput;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Debug.LogError($"A {typeof(StartMenu).Name} script already exists within the scene!");
                return;
            }
        }

        private void Start()
        {
            OnSeedChange();
            Generation.RoomGeneration.Instance.StartRoomGeneration(true);
        }

        /// <summary>
        /// Called through UI event on the seed input.
        /// </summary>
        public void OnSeedChange()
        {
            SeedManager.UpdateSeededRandoms();
        }

        public static void SetSeedText(string value)
        {
            Instance.SeedInput.text = value;
        }

        public static void SetSeedText(int value)
        {
            Instance.SeedInput.text = value.ToString();
        }

        public static int GetSeed()
        {
            return Animator.StringToHash(Instance.SeedInput.text);
        }
    }
}
