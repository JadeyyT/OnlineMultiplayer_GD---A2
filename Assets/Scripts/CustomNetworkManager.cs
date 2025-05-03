using Mirror;
using UnityEngine;
using System.Collections.Generic;

public class CustomNetworkManager : NetworkManager
{
    [Header("Player Prefabs")]
    public List<GameObject> playerPrefabs; 

    private List<int> usedIndices = new List<int>(); 

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        int index;

        // All prefabs have been used, reset the list
        if (usedIndices.Count >= playerPrefabs.Count)
        {
            usedIndices.Clear();
        }

        // Pick a unique random index not already used
        do
        {
            index = Random.Range(0, playerPrefabs.Count);
        } while (usedIndices.Contains(index));

        usedIndices.Add(index);

        GameObject prefabToSpawn = playerPrefabs[index];

        Transform startPos = GetStartPosition();
        GameObject player = Instantiate(prefabToSpawn, startPos != null ? startPos.position : Vector3.zero, Quaternion.identity);

        NetworkServer.AddPlayerForConnection(conn, player);
    }
}
