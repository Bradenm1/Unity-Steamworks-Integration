using UnityEngine;

namespace Steamworks.NET
{
    public class SteamworksManager : MonoBehaviour
    {
        void Start()
        {
            if (SteamManager.Initialized)
            {
                string name = SteamFriends.GetPersonaName();
                Debug.Log(name);
            }
        }
    }
}