using JetBrains.Annotations;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class Player : MonoBehaviour
{

    public bool isMain = false;

    private Vector3 leftVect, rightVect;
    private float timer;
    [SerializeField] float notshootingSpeed;
    [SerializeField] float shootingSpeed;


    [SerializeField] GameObject bullet;
    public float bulletSpeed;
    public int bulletDM;
    public float bulletScale;
    public float hitInterval;
    private Slider reloadBar;

    public int hp = 100;
    public bool isAlive = true;
    public int deadPlayerAmount = 0;


    private Slider hpBar;
    public int coins;
    private Text coinsText;
    public GameObject coin;
    public Text nickname;

    public bool isEnd = false;
    private GameObject finMenu;

    public Text endText;
    private Text alivePlayerText;

    private GameObject pause;
    private GameObject gameplayUI;


    [SerializeField] bool isLeftStick;
    [SerializeField] bool isRightStick;

    PhotonView view;
    Joystick joystickL;
    Joystick joystickR;



    private void Start()
    {

        view = GetComponent<PhotonView>();
        nickname.text = view.Owner.NickName;
        if (view.IsMine)
        {
            joystickL = GameObject.FindGameObjectWithTag("Left Joystick").GetComponent<Joystick>();
            joystickR = GameObject.FindGameObjectWithTag("Right Joystick").GetComponent<Joystick>();

            reloadBar = GameObject.Find("ReloadSlider").GetComponent<Slider>();
            hpBar = GameObject.Find("HPSlider").GetComponent<Slider>();
            coinsText = GameObject.Find("Money counter").GetComponent<Text>();
            pause = GameObject.Find("Pause");
            gameplayUI = GameObject.Find("Gameplay");
            finMenu = GameObject.Find("FinMenu");
            GameObject.Find("PauseButton").GetComponent<Button>().onClick.AddListener(OpenPause);
            GameObject.Find("PauseLeaveRoom").GetComponent<Button>().onClick.AddListener(LeaveRoom);
            GameObject.Find("PauseBack").GetComponent<Button>().onClick.AddListener(ClosePause);
            endText = GameObject.Find("Winner").GetComponent<Text>();
            GameObject.Find("FinLeaveRoom").GetComponent<Button>().onClick.AddListener(LeaveRoom);
            alivePlayerText = GameObject.Find("CurrPlayers").GetComponent<Text>();
            pause.SetActive(false);
            finMenu.SetActive(false);
            StartCoroutine(CoinFactory());
            isMain = true;
        }
            
    }

    IEnumerator CoinFactory() 
    {
        while (true)
        {
            yield return new WaitForSeconds(GameObject.Find("SpawnManager").GetComponent<SpawnManager>().coinInterval);
            if (GameObject.Find("SpawnManager").GetComponent<SpawnManager>()._coins.Count <= GameObject.Find("SpawnManager").GetComponent<SpawnManager>()._coinsMAXLenght)
                view.RPC("CreateCoin", RpcTarget.All, GameObject.Find("SpawnManager").GetComponent<SpawnManager>().GetPos());
        }
    }

    private void Update()
    {


        if (hp <= 0 && isAlive)
        {
            PlayerDisable();
        }

        if (view.IsMine)
        {
            leftVect = new Vector3(joystickL.Horizontal, joystickL.Vertical, 0);
            rightVect = new Vector3(joystickR.Horizontal, joystickR.Vertical, 0);
            isLeftStick = leftVect != Vector3.zero ? true : false;
            isRightStick = rightVect != Vector3.zero ? true : false;
            if (hp <= 0 && isAlive)
            {
                view.RPC("OtherPlayerDeath", RpcTarget.All);
                isAlive = false;
                PlayerDisable();
            }

            AliveCheck();

            if (isEnd)
            {
                finMenu.SetActive(true);
                PlayerDisable();
            }


            hpBar.value = hp / 100f;
            coinsText.text = coins.ToString();
            reloadBar.value = timer > 0 ? (hitInterval - timer) / hitInterval : 1;
            alivePlayerText.text = (PhotonNetwork.PlayerList.Length - deadPlayerAmount).ToString();

            if (!isRightStick)
                transform.position += leftVect * notshootingSpeed * Time.deltaTime;
            else
            {
                transform.position += leftVect * shootingSpeed * Time.deltaTime;
                if (timer < 0)
                {
                    view.RPC("Shooting", RpcTarget.All, transform.position + rightVect.normalized, rightVect.normalized, bulletSpeed, bulletScale, bulletDM, view.ViewID);
                    timer = hitInterval;
                }
            }

        }

        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }
    }

    private void PlayerDisable()
    {
        GetComponent<CircleCollider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;

        nickname.gameObject.SetActive(false);

        joystickL.gameObject.SetActive(false);
        joystickR.gameObject.SetActive(false);
        hpBar.gameObject.SetActive(false);
        reloadBar.gameObject.SetActive(false);

        pause.SetActive(false);
    }
    private void AliveCheck()
    {
        int n = PhotonNetwork.PlayerList.Length;
        if (n - deadPlayerAmount <= 1 && isAlive && n > 1 && !isEnd)
        {
           
            view.RPC("SendEndInfo", RpcTarget.All, view.Owner.NickName, coins.ToString()); 
        }
    }

    public void OpenPause()
    {
        pause.SetActive(true);
        gameplayUI.SetActive(false);
    }
    public void ClosePause()
    {
        pause.SetActive(false);
        gameplayUI.SetActive(true);
    }

    public void LeaveRoom()
    {
        if (!isAlive && view.IsMine)
            view.RPC("OtherPlayerDeathFalse", RpcTarget.All);

            PhotonNetwork.LeaveRoom();
            PhotonNetwork.LoadLevel("Menu");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            if (view.IsMine)
            {
                coins++;
                view.RPC("DestroyCoin", RpcTarget.All, collision.transform.position);
            }
            
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet")
        {
            if (view.IsMine)
            {
                Bullet buf = collision.gameObject.GetComponent<Bullet>();
                if (buf.GetId() != view.ViewID)
                {
                    hp -= buf.GetDM();

                }
            }
            Destroy(collision.gameObject);
        }
    }


    [PunRPC]
    private void SendEndInfo(string name, string score)
    {
        foreach (GameObject x in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (x.GetComponent<Player>().isMain)
            {
                x.GetComponent<Player>().isEnd = true;
                x.GetComponent<Player>().endText.text = name + " is Winner with " + score + " coins";
            }
        }



    }

    [PunRPC]
    private void OtherPlayerDeath()
    {
        foreach (GameObject x in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (x.GetComponent<Player>().isMain)
            {
                x.GetComponent<Player>().deadPlayerAmount++;
            }
        }
    }

    [PunRPC]
    private void OtherPlayerDeathFalse()
    {
        foreach (GameObject x in GameObject.FindGameObjectsWithTag("Player"))
        {
            if (x.GetComponent<Player>().isMain)
            {
                x.GetComponent<Player>().deadPlayerAmount--;
            }
        }
    }

    [PunRPC]
    private void Shooting(Vector3 pos, Vector3 dir, float bulletSpeed , float bulletScale, int bulletDM, int id)
    {
        GameObject obj;
        obj = Instantiate(bullet, pos, quaternion.identity);
        obj.GetComponent<Bullet>().SetParams(dir, bulletSpeed, bulletScale, bulletDM, id);

    }

    [PunRPC]
    private void CreateCoin(Vector3 pos)
    {
        GameObject obj = Instantiate(coin, pos, quaternion.identity);
        GameObject.Find("SpawnManager").GetComponent<SpawnManager>()._coins.Add(obj);
        GameObject.Find("SpawnManager").GetComponent<SpawnManager>()._coinsPos.Add(obj.transform.position);

    }


    [PunRPC]
    private void DestroyCoin(Vector3 pos)
    {
        GameObject.Find("SpawnManager").GetComponent<SpawnManager>().DestroyChilld(pos);
    }

}
