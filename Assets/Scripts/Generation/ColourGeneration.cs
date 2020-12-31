using System;
using Game;
using Steamworks.NET;
using UnityEngine;

namespace Generation
{
    public class ColourGeneration : MonoBehaviour
    {
        public static ColourGeneration Instance;
        public Material FloorMaterial01;
        public Material FloorMaterial02;
        public Material WallMaterial01;
        public Material WallMaterial02;

        public Gradient LevelDeptGradient;
        private static readonly float DarkerShade = 0.2f;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Debug.LogError($"A {typeof(ColourGeneration).Name} script already exists within the scene!");
                return;
            }

            SetLevelColour(0); // Set the default colour

            Events.Events.OnLevelStart += UpdateLevelDepth;
        }

        public static void UpdateLevelDepth()
        {
            SetLevelColour(Stats.NPCurrentDepth);
        }

        public static void SetLevelColour(int depth, int maxDepth = 100)
        {
            Color normalShade = GetLevelDepthColour(depth, maxDepth);
            Color darkerShade = new Color(normalShade.r - DarkerShade, normalShade.g - DarkerShade, normalShade.b - DarkerShade, normalShade.a);

            SetGroundColour(normalShade, darkerShade);
            SetWallColour(normalShade, darkerShade);
        }

        public static void SetGroundColour(Color normalShade, Color darkerShade)
        {
            SetMaterialColour(Instance.FloorMaterial01, normalShade);
            SetMaterialColour(Instance.FloorMaterial02, darkerShade);
        }
        public static void SetWallColour(Color normalShade, Color darkerShade)
        {
            SetMaterialColour(Instance.WallMaterial01, normalShade);
            SetMaterialColour(Instance.WallMaterial02, darkerShade);
        }

        public static void SetMaterialColour(Material material, Color color)
        {
            material.color = color;
        }

        public static Color GetLevelDepthColour(int depth, int maxDepth)
        {
            return Instance.LevelDeptGradient.Evaluate(Mathf.Clamp((float)depth / maxDepth, 0, maxDepth));
        }

        private void OnDestroy()
        {
            Events.Events.OnLevelStart -= UpdateLevelDepth;
        }
    }
}
