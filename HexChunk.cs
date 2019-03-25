using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HexMesh))]
public class HexChunk : MonoBehaviour {

    public HexMesh mesh;
    public HexCell[] cells;

    private void Awake()
    {
        cells = new HexCell[HexMetrics.ChunkSize_X * HexMetrics.ChunkSize_Z];
        mesh = GetComponent<HexMesh>();
    }

    public void AddCell(HexCell cell,int index)
    {
        cells[index] = cell;
        cell.transform.SetParent(transform, false);
    }

    public void GenMesh()
    {
        mesh.GenHexmapMesh(cells);
    }
    
}
