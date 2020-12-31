using System.Collections;
using System.Collections.Generic;
using Game;
using UI;
using UnityEngine;

namespace Generation
{
    public class RoomGeneration : MonoBehaviour
    {
        public static RoomGeneration Instance;
        // This contains all the current spawned rooms
        public static List<Room> CurrentRooms = new List<Room>();
        // This contains all the rooms which have an empty connection
        public static List<Room> AllEmptyConnectionRooms = new List<Room>();
        // The last room generated
        public static Room EndRoom;
        // End Objective
        public static Objective EndObjective;

        [Tooltip("The objective used to end the game")]
        public GameObject EndObjectivePrefab;
        [Tooltip("The navmesh script reference")]
        public LocalNavMeshBuilder NavMeshBuilder;
        [Tooltip("The starting room script reference")]
        public Room StartingRoom;
        [Tooltip("The amount of rooms which can be inited at once")]
        private int MaxRoomCount = 30;
        [Tooltip("The max branching which can happen on a room")]
        private int MaxBranching = 30;
        [Tooltip("The room prefabs used in the terrain generation")]
        public Room[] AllRoomsPrefabs;
        // Returns true if the rooms have been generated for the level
        public static bool HasGeneratedRooms => CurrentRooms.Count > 1;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else
            {
                Debug.LogError($"A {typeof(RoomGeneration).Name} script already exists within the scene!");
                return;
            }

            Events.Events.OnSceneStart += ClearCachedStatics;
            Events.Events.OnGameStart += StartRoomGeneration;
            Events.Events.OnLevelRoomsGenerated += SpawnEndObjective;
            Events.Events.OnLevelRoomsAlreadyGenerated += SpawnEndObjective;
            Events.Events.OnLevelFinish += DeleteExistingRooms;
        }

        /// <summary>
        /// Button event used to start the room generation
        /// </summary>
        public void StartRoomGeneration()
        {
            StartRoomGeneration(false);
        }

        /// <summary>
        /// Button event used to start the room generation
        /// </summary>
        public void StartRoomGeneration(bool forceGenerate)
        {
            StartCoroutine(Instance.StartGeneration(forceGenerate));
        }


        /// <summary>
        /// Start the room generation
        /// </summary>
        public IEnumerator StartGeneration(bool forceGenerate = false)
        {
            if (!forceGenerate && HasGeneratedRooms)
            {
                StartMenu.Instance.OnSeedChange();
                if (GameFlowManager.IsGameStarted) Events.Events.OnOnLevelRoomsAlreadyGenerated();
                yield break;
            }

            // Delete all the existing rooms
            DeleteExistingRooms();

            yield return new WaitForEndOfFrame();

            // Add the starting room to the generation
            Generate(StartingRoom, 0);

            // GeneratePatrolPaths the rooms
            GenerateRooms();

            // Check if any connections not connected should be (In-case two connections overlapped but were not in the generation)
            CheckHasAnyAjacentConnections();

            // Close all the doors on the rooms
            SetAllDoorsClosed();

            // Start generating the navmesh
            EnableAllRoomsNavmesh();

            yield return new WaitForEndOfFrame();

            if (GameFlowManager.IsGameStarted) Events.Events.OnOnLevelRoomsGenerated();
        }

        /// <summary>
        /// GeneratePatrolPaths all the rooms
        /// </summary>
        public void GenerateRooms()
        {
            for (int i = 0; i < MaxRoomCount; i++)
            {
                Room room = GetRandomRoomFreeConnection();
                if (room == null) break;
                Generate(room, 0);
            }
        }

        /// <summary>
        /// Deletes all existing rooms
        /// </summary>
        public void DeleteExistingRooms()
        {
            if (EndObjective) Destroy(EndObjective.gameObject);
            for (int i = 1; i < CurrentRooms.Count; i++)
            {
                if (CurrentRooms[i] == null || StartingRoom.name == CurrentRooms[i].name) continue;
                Destroy(CurrentRooms[i].gameObject);
            }
            CurrentRooms.Clear();
            AllEmptyConnectionRooms.Clear();
            CurrentRooms.Add(StartingRoom); // Re-add the starting room
            AllEmptyConnectionRooms.Add(StartingRoom); // Re-add the starting room

            foreach (var connection in StartingRoom.Connections)
            {
                connection.Room = null;
            }
        }

        /// <summary>
        /// Enable the navmesh for all rooms
        /// </summary>
        public void EnableAllRoomsNavmesh()
        {
            foreach (var nav in NavMeshSourceTag.m_NavMeshSourceTags)
            {
                nav.enabled = true;
            }

            // Enable the navmesh builder script to build the navmesh
            NavMeshBuilder.enabled = true;
        }

        /// <summary>
        /// Spawn the end objective
        /// </summary>
        public static void SpawnEndObjective()
        {
            Instance.PlaceEndObjective(EndRoom);
        }

        /// <summary>
        /// Place end objective in a room
        /// </summary>
        /// <param name="room"></param>
        public void PlaceEndObjective(Room room)
        {
            EndObjective = Instantiate(EndObjectivePrefab, room.SpawnPoints[0].SpawnPointTransform.position, Quaternion.identity).GetComponent<Objective>();
            room.SpawnPoints[0].HasSpawned = true;
        }

        /// <summary>
        /// GeneratePatrolPaths a room given the last room generation
        /// </summary>
        /// <param name="room"></param>
        /// <param name="depth"></param>
        public void Generate(Room room, int depth)
        {
            if (depth >= MaxBranching) return;

            var roomFreeConnection = GetFreeRandomConnection(room);
            if (roomFreeConnection == null) return;

            Room nextRoom = room.name == StartingRoom.name ? CreateRoom(FindPrefabRoomMoreThanOneConnection()) : CreateRoom(GetRandomRoom());

            var nextRoomFreeConnection = GetFreeConnection(nextRoom);
            if (nextRoomFreeConnection == null)
            {
                DestroyImmediate(nextRoom.gameObject);
                return;
            }

            SetNewRoomPosition(roomFreeConnection, nextRoomFreeConnection, nextRoom);

            Physics.SyncTransforms();
            if (!CanPlace(nextRoom))
            {
                //Debug.LogError("Cannot place: " + nextRoom.name);
                DestroyImmediate(nextRoom.gameObject);
                return;
            }

            AddRoom(room);
            AddRoom(nextRoom);

            roomFreeConnection.Room = nextRoom;
            nextRoomFreeConnection.Room = room;

            if (GetFreeConnection(room) == null) AddEmptyConnectionRoom(room);
            else RemoveEmptyConnectionRoom(room);
            if (GetFreeConnection(nextRoom) == null) AddEmptyConnectionRoom(nextRoom);
            else RemoveEmptyConnectionRoom(room);

            //if (!MeshCombiner.CombineParents.Contains(room.gameObject)) MeshCombiner.CombineParents.Add(room.gameObject);
            //if (!MeshCombiner.CombineParents.Contains(nextRoom.gameObject)) MeshCombiner.CombineParents.Add(nextRoom.gameObject);

            EndRoom = nextRoom;
            depth++;
            Generate(nextRoom, depth);
        }

        /// <summary>
        /// Add room to empty connections
        /// </summary>
        /// <param name="room"></param>
        public void AddEmptyConnectionRoom(Room room)
        {
            if (!AllEmptyConnectionRooms.Contains(room)) AllEmptyConnectionRooms.Add(room);
        }

        /// <summary>
        /// Remove room from empty connections
        /// </summary>
        /// <param name="room"></param>
        public void RemoveEmptyConnectionRoom(Room room)
        {
            if (AllEmptyConnectionRooms.Contains(room)) AllEmptyConnectionRooms.Remove(room);
        }

        /// <summary>
        /// Find a prefab that has more than one connection
        /// </summary>
        /// <returns></returns>
        public Room FindPrefabRoomMoreThanOneConnection()
        {
            List<Room> randomList = new List<Room>();
            foreach (var room in AllRoomsPrefabs)
            {
                if (room.Connections.Length > 1) randomList.Add(room);
            }

            if (randomList.Count == 0) return null;

            return randomList[SeedManager.RoomGenerationRandom.Next(0, randomList.Count)];
        }

        /// <summary>
        /// Add a room to the current rooms
        /// </summary>
        /// <param name="room"></param>
        public void AddRoom(Room room)
        {
            if (!CurrentRooms.Contains(room)) CurrentRooms.Add(room);
        }

        /// <summary>
        /// Connect the room to the free connection
        /// </summary>
        /// <param name="roomFreeConnection"></param>
        /// <param name="nextRoomFreeConnection"></param>
        /// <param name="nextRoom"></param>
        public void SetNewRoomPosition(Room.Connection roomFreeConnection, Room.Connection nextRoomFreeConnection, Room nextRoom)
        {
            Vector3 targetConnectionEuler = roomFreeConnection.ConnectionPoint.eulerAngles;
            Vector3 roomConnectionEuler = nextRoomFreeConnection.ConnectionPoint.eulerAngles;

            float deltaAngle = Mathf.DeltaAngle(roomConnectionEuler.y, targetConnectionEuler.y);
            Quaternion currentConnectionTargetRotation = Quaternion.AngleAxis(deltaAngle, Vector3.up);
            Transform nextRoomTransform;
            (nextRoomTransform = nextRoom.transform).rotation = currentConnectionTargetRotation * Quaternion.Euler(0, 180f, 0);

            Vector3 connectionPositionOffset = nextRoomFreeConnection.ConnectionPoint.position - nextRoomTransform.position;
            nextRoomTransform.position = roomFreeConnection.ConnectionPoint.position - connectionPositionOffset;
        }

        /// <summary>
        /// Get a free connection in a room
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public Room.Connection GetFreeConnection(Room room)
        {
            foreach (var connection in room.Connections)
            {
                if (connection.Room == null) return connection;
            }

            return null;
        }

        /// <summary>
        /// Returns a random connection instead of the next un-occupied connection
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public Room.Connection GetFreeRandomConnection(Room room)
        {
            List<Room.Connection> randomList = new List<Room.Connection>();
            foreach (var connection in room.Connections)
            {
                if (connection.Room == null) randomList.Add(connection);
            }

            if (randomList.Count == 0) return null;

            return randomList[SeedManager.RoomGenerationRandom.Next(0, randomList.Count)];
        }

        /// <summary>
        /// Check if a room can be placed in its current position
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        public static bool CanPlace(Room room)
        {
            foreach (var rC in room.RoomSizes)
            {
                Vector3 wC = rC.transform.TransformPoint(rC.center);

                Collider[] colliders = Physics.OverlapSphere(wC, rC.radius,1 << 0);
                foreach (var c in colliders)
                {
                    if (!c.transform.root.Equals(room.transform))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if any connections not connected should be
        /// This should only be called once
        /// </summary>
        public void CheckHasAnyAjacentConnections()
        {
            foreach (var room in AllEmptyConnectionRooms)
            {
                foreach (var nextRoom in AllEmptyConnectionRooms)
                {
                    foreach (var roomConnection in room.Connections)
                    {
                        foreach (var nextRoomConnection in nextRoom.Connections)
                        {
                            if (nextRoomConnection.ConnectionPoint.position == roomConnection.ConnectionPoint.position)
                            {
                                roomConnection.Room = nextRoom;
                                nextRoomConnection.Room = room;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a random room given all the rooms
        /// </summary>
        /// <returns></returns>
        public Room GetRandomRoom()
        {
            return AllRoomsPrefabs[SeedManager.RoomGenerationRandom.Next(0, AllRoomsPrefabs.Length)];
        }

        /// <summary>
        /// Get a room with a free connection
        /// </summary>
        /// <returns></returns>
        public Room GetRandomRoomFreeConnection()
        {
            if (AllEmptyConnectionRooms.Count == 0) return null;
           return AllEmptyConnectionRooms[SeedManager.RoomGenerationRandom.Next(0, AllEmptyConnectionRooms.Count)];
        }

        /// <summary>
        /// Create a room given a prefab
        /// </summary>
        /// <param name="room"></param>
        /// <param name="prefabRoom"></param>
        /// <returns></returns>
        public Room CreateRoom(Room prefabRoom)
        {
            Room newRoom = Instantiate(prefabRoom, Vector3.zero, Quaternion.identity);
            return newRoom;
        }

        /// <summary>
        /// Sets all the doors without a room as closed
        /// </summary>
        public void SetAllDoorsClosed()
        {
            foreach (var room in CurrentRooms)
            {
                foreach (var connection in room.Connections)
                {
                    if (connection.Room == null) connection.Door.SetActive(true);
                    else connection.Door.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Clears the statics for new games
        /// </summary>
        public static void ClearCachedStatics()
        {
            CurrentRooms.Clear();
            AllEmptyConnectionRooms.Clear();
        }

        private void OnDestroy()
        {
            Events.Events.OnSceneStart -= ClearCachedStatics;
            Events.Events.OnGameStart -= StartRoomGeneration;
            Events.Events.OnLevelRoomsGenerated -= SpawnEndObjective;
            Events.Events.OnLevelRoomsAlreadyGenerated -= SpawnEndObjective;
            Events.Events.OnLevelFinish -= DeleteExistingRooms;
        }
    }
}
