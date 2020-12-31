using System;
using UnityEngine;

namespace Generation
{
    public class Room : MonoBehaviour
    {
        public Connection[] Connections;
        public SphereCollider[] RoomSizes;
        public SpawnPoint[] SpawnPoints;

        [Serializable]
        public class Connection
        {
            public Transform ConnectionPoint; // The connection point itself
            public GameObject Door; // The door for this connection
            public Room Room; // Room connected to this point
        }

        private void OnDrawGizmosSelected()
        {
            foreach (var rC in RoomSizes)
            {
                Vector3 worldCenter = rC.transform.TransformPoint(rC.center);
                Collider[] colliders = Physics.OverlapSphere(worldCenter, rC.radius,1 << 0);
                Gizmos.color = Color.green;
                foreach (var c in colliders)
                {
                    if (!c.transform.root.Equals(transform))
                    {
                        Gizmos.color = Color.red;
                        break;
                    }
                }

                Gizmos.DrawSphere(worldCenter, rC.radius);
            }
        }
    }
}
