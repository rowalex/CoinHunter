using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UIElements;

public class Bullet : MonoBehaviour
{
    private Vector3 dir;
    private float speed;
    private int dm;
    private int owner;
    private bool isReadyToReDir= true;
    public void SetParams(Vector3 dir, float speed, float scale, int dm, int owner)
    {
        this.dir = dir;
        this.speed = speed;
        transform.localScale = new Vector3(scale, scale, 0);
        gameObject.GetComponent<TrailRenderer>().widthMultiplier = scale;
        this.dm = dm;
        this.owner = owner;
    }
    public int GetId()
    {
        return owner;
    }
    public int GetDM()
    {
        return dm;
    }

    private void Start()
    {
        StartCoroutine(DestroyMe());
    }

    private void Update()
    {
        transform.position += dir * speed * Time.deltaTime;
    }

    IEnumerator DestroyMe()
    {
        yield return new WaitForSeconds(10);
        Destroy(gameObject);
    }
    IEnumerator ReDirCol()
    {
        yield return new WaitForEndOfFrame();
        isReadyToReDir = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Envi" && isReadyToReDir)
        {
            Debug.Log(dir + "\n" + Vector3.Reflect(dir, collision.contacts[0].normal).normalized);
            dir = Vector3.Reflect(dir, collision.contacts[0].normal).normalized;
            isReadyToReDir = false;
            StartCoroutine(ReDirCol());
        }
    }


}
