using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject[] maps;
    private bool canPLay = false;
    private List<Transform> spawnPointsTransform;
    public List<GameObject> _coins = new List<GameObject>();
    public List<Vector3> _coinsPos = new List<Vector3>();
    [SerializeField] public int _coinsMAXLenght = 10;
    [SerializeField] public float coinInterval;


    private void Awake()
    {
        int rand = Random.Range(0, maps.Length);
        if (PhotonNetwork.IsMasterClient)
            FindObjectOfType<PhotonView>().RPC("MapCreate", RpcTarget.All, rand);
    }
    public void MapCreate(int id)
    {
         Instantiate(maps[id], Vector3.zero, Quaternion.identity);
    }


    private void Update()
    {
        if (!canPLay)
        {
            spawnPointsTransform = new List<Transform>();
            foreach (Transform t in GameObject.Find("SpawnPoints").transform)
            {
                spawnPointsTransform.Add(t);
            }

            int rand = Random.Range(0, spawnPointsTransform.Count);
            Vector3 pos = spawnPointsTransform[rand].position +
                new Vector3(
                    Random.Range(-spawnPointsTransform[rand].localScale.x, spawnPointsTransform[rand].localScale.x) * 0.5f,
                    Random.Range(-spawnPointsTransform[rand].localScale.y, spawnPointsTransform[rand].localScale.y) * 0.5f, 0);
            GameObject pl = PhotonNetwork.Instantiate(playerPrefab.name, pos, Quaternion.identity);
            Vector3 col = new Vector3(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            pl.GetComponent<SpriteRenderer>().color = new Color(col[0], col[1], col[2], 1);
            canPLay = true;
        }
    }

    public void DestroyChilld(Vector3 pos)
    {
        int id = _coinsPos.IndexOf(pos);
        Destroy(_coins[id]);
        _coins.RemoveAt(id);
        _coinsPos.RemoveAt(id);
    }

    public Vector3 GetPos()
    {
        int rand = Random.Range(0, spawnPointsTransform.Count);
        Vector3 pos = spawnPointsTransform[rand].position +
            new Vector3(
                Random.Range(-spawnPointsTransform[rand].localScale.x, spawnPointsTransform[rand].localScale.x) * 0.5f,
                Random.Range(-spawnPointsTransform[rand].localScale.y, spawnPointsTransform[rand].localScale.y) * 0.5f, 0);
        return pos;
    }
}
