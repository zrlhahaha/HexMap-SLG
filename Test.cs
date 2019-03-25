using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {


    public int test;
    public bool check = false;
    public Transform a;
    public Transform[] x;
    public Transform c;

    private void Start()
    {
    }


    void Update ()
    {
        if (check)
        {
            check = false;
        }
    }


}
