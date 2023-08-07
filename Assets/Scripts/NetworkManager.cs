using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    [PunRPC]
    private void RPCLoadGame()
    {
        PhotonNetwork.LoadLevel("Game");
    }
    [PunRPC]
    private void MapCreate(int id)
    {
            GameObject.Find("SpawnManager").GetComponent<SpawnManager>().MapCreate(id);
    }
}
