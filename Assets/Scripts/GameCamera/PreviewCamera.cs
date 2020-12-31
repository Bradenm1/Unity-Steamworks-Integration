using UnityEngine;

namespace GameCamera
{
    public class PreviewCamera : MonoBehaviour
    {
        public Camera Camera;
        public Camera MainCamera; // The main camera for the scene usually the player camera

        private void Awake()
        {
            Events.Events.OnSceneStart += EnableCamera;
            Events.Events.OnGameStart += DisableCamera;
        }

        public void ToggleCamera(bool toggle)
        {
            if (toggle)
                EnableCamera();
            else 
                DisableCamera();
        }

        public void EnableCamera()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Camera.enabled = true;
            MainCamera.enabled = false;
        }

        public void DisableCamera()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            MainCamera.enabled = true;
            Camera.enabled = false;
        }

        private void OnDestroy()
        {
            Events.Events.OnSceneStart -= EnableCamera;
            Events.Events.OnGameStart -= DisableCamera;
        }
    }
}
