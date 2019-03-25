using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gobal : MonoBehaviour
{
    public Texture2D noiseTex;

    private void Awake()
    {
        HexMetrics.noise = noiseTex;
    }

}