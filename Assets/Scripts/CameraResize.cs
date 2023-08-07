using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraResize : MonoBehaviour
{
    private SpriteRenderer rink;

    private void Start()
    {
        rink = GameObject.Find("Rink").GetComponent<SpriteRenderer>();
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = rink.bounds.size.x / rink.bounds.size.y;
        if (screenRatio >= targetRatio)
        {
            Camera.main.orthographicSize = rink.bounds.size.y / 2;
        }
        else
        {
            float dif = targetRatio / screenRatio;
            Camera.main.orthographicSize = rink.bounds.size.y / 2 * dif;
        }
    }
}
