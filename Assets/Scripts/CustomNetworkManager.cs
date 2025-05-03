using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager
{
    [Header("Player Prefabs")]
    public List<GameObject> playerPrefabs; // Assign multiple player prefabs here

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        // Randomly pick one of the prefabs
        int index = Random.Range(0, playerPrefabs.Count);
        GameObject prefabToSpawn = playerPrefabs[index];

        Transform startPos = GetStartPosition();
        GameObject player = Instantiate(prefabToSpawn, startPos != null ? startPos.position : Vector3.zero, Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
