using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using Unity.VisualScripting;

public class MenuManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private InputField createRoom;
    [SerializeField] private InputField joinRoom;
    [SerializeField] private InputField nameField;
    [SerializeField] private Transform content;
    [SerializeField] private GameObject _listing;
    private List<Text> _listings = new List<Text>();
    private int playernumb = -1;


    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject roomMenu;
    [SerializeField] private GameObject startButton;

    [SerializeField] private Player player;
    [SerializeField] private Scrollbar scrollbarDM;
    [SerializeField] private Scrollbar scrollbarSpeed;
    [SerializeField] private Scrollbar scrollbarScale;
    [SerializeField] private Scrollbar scrollbarInterval;
    float currDmVal, currSpeedVal, currScaleVal, currIntervalVal; 
    [SerializeField] private Text textDM;
    [SerializeField] private Text textSpeed;
    [SerializeField] private Text textScale;
    [SerializeField] private Text textInterval;
    [SerializeField] private Text roomText;
    private int playerDM;
    private float playerSpeed;
    private float playerScale;
    private float playerInterval;


    private void Update()
    {
        PhotonNetwork.LocalPlayer.NickName = nameField.text;

        if (PhotonNetwork.InRoom)
        {
            if (PhotonNetwork.PlayerList.Length != playernumb)
            {
                foreach(Text x in _listings)
                {
                    Destroy(x.transform.parent.gameObject);
                }
                _listings.Clear();

                foreach (Photon.Realtime.Player x in PhotonNetwork.PlayerList)
                {
                    Text listing = Instantiate(_listing, content).GetComponentInChildren<Text>();
                    if (listing != null)
                    {
                        listing.text = x.NickName;
                        _listings.Add(listing);
                    }
                }
                playernumb = PhotonNetwork.PlayerList.Length;
            }
            PlayerConfig();

            roomText.text = "Room: " + PhotonNetwork.CurrentRoom.Name;
            if (PhotonNetwork.IsMasterClient)
            {
                startButton.SetActive(true);
            }
            else
            {
                startButton.SetActive(false);
            }
        } else playernumb = -1;
    }


    public void CreateRoom()
    {
        if (createRoom.text.Length > 0 && nameField.text.Length > 0)
        {   
            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 10;
            PhotonNetwork.CreateRoom(createRoom.text, roomOptions);
           
        }
    }

    public void JoinRoom()
    {
        if (joinRoom.text.Length > 0 && nameField.text.Length > 0)
            PhotonNetwork.JoinRoom(joinRoom.text);
    }

    public override void OnJoinedRoom()
    {
        mainMenu.SetActive(false);
        roomMenu.SetActive(true);
    }

    public override void OnLeftRoom()
    {
        mainMenu.SetActive(true);
        roomMenu.SetActive(false);       
    }

    public void StartGame()
    {
        PhotonView photonView = GameObject.Find("NetworkManager").GetComponent<PhotonView>();
        photonView.RPC("RPCLoadGame", RpcTarget.All);
    }

    public void LeftRoom()
    {
        Photon.Realtime.Player[] arr = PhotonNetwork.PlayerList;
        if (arr.Length > 1 && PhotonNetwork.IsMasterClient)
            PhotonNetwork.CurrentRoom.SetMasterClient(arr[0].IsMasterClient ? arr[1] : arr[0]); 
        PhotonNetwork.LeaveRoom();
    }


    public void PlayerConfig()
    {
        int dmMin = 10, dmMax = 50;
        float scaleMin = 0.3f, scaleMax = 1.5f;
        float speedMin = 5, speedMax = 30;
        float intervalMin = 1.5f, intervalMax = 0.3f;
        float maxA = 3.5f;

        float currA = (1 + scrollbarDM.value) * (1 + scrollbarScale.value) * (1 + scrollbarSpeed.value) * (1 + scrollbarInterval.value);
        if (currA > maxA)
        {
            scrollbarDM.value = currDmVal;
            scrollbarScale.value = currScaleVal;
            scrollbarSpeed.value = currSpeedVal;
            scrollbarInterval.value = currIntervalVal;
        }
        playerDM = (int)((dmMax - dmMin) * scrollbarDM.value * scrollbarDM.value) + dmMin;
        playerScale = ((scaleMax - scaleMin) * scrollbarScale.value) +scaleMin;
        playerSpeed = ((speedMax - speedMin) * scrollbarSpeed.value) + speedMin;
        playerInterval = (intervalMin - intervalMax) * (1 - scrollbarInterval.value) + intervalMax;
        textDM.text = "Dmg\n" + playerDM;
        textScale.text = "Scale\n" + playerScale;
        textSpeed.text = "Speed\n" + playerSpeed;
        textInterval.text = "HitRate\n" + playerInterval;
        currDmVal = scrollbarDM.value;
        currScaleVal = scrollbarScale.value;
        currSpeedVal = scrollbarSpeed.value;
        currIntervalVal = scrollbarInterval.value;
        player.bulletDM = playerDM;
        player.bulletScale = playerScale;
        player.bulletSpeed = playerSpeed;
        player.hitInterval = playerInterval;
    }

}
