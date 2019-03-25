using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexmapEditor : MonoBehaviour {


    public Color[] colors;
    public int selectedColor;
    public int elevationLevel;


    //public HexCell selectedHexcell;

    public Camera cam;
    public HexGrid grid;

    public static HexmapEditor _instance;

    private void Awake()
    {
        if (_instance != null)
            Debug.LogWarning("2 or more HexmapEditor existed in secen");

        _instance = this;

        cam = Camera.main;
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Mouse0))
            return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (!Physics.Raycast(ray, out hitInfo))
            return;
    }

    public void EditHexCell(HexCell cell)
    {
        cell.Elevation = elevationLevel;
        cell.color = colors[selectedColor];
    }
}
