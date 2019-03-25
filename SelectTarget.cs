using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HexCell))]
public class SelectTarget : MonoBehaviour {

    public HexCell hexcell;

    private void Awake()
    {
        hexcell = GetComponent<HexCell>();
    }

    public void SelectCell()
    {
        //HexmapEditor._instance.SelectHexCell(hexcell);
    }
}
