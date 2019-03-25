using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    int cellCount_X;
    int cellCount_Z;
    public Text coordText;
    public HexCell hexcellPrefab;
    public Canvas canvas;

    public HexCell[] cells;
    public HexChunk[] chunks;

    public int chunkCount_X = 4;
    public int chunkCount_Z = 3;
    public HexChunk chunkPrefab;
	void Start ()
    {
        cellCount_X = chunkCount_X * HexMetrics.ChunkSize_X;
        cellCount_Z = chunkCount_Z * HexMetrics.ChunkSize_Z;

        GenHexGrid();
    }

    void GenHexGrid()
    {
        GenHexCells();
        ConnectHexcell();
        GenChunks();
        AddCellsToChunks();
        GenChunksMesh();
    }

     void GenHexCells()
    {
        cells = new HexCell[cellCount_Z * cellCount_X];

        for (int z = 0, i = 0; z < cellCount_Z; z++)
            for (int x = 0; x < cellCount_X; x++, i++)
                GenHexCell(x, z, i);

    }

    void GenHexCell(int x,int z,int i)
    {
        float x_world = (x + z * 0.5f - z / 2) * HexMetrics.InnerRadius * 2;
        float z_world = z * HexMetrics.OutterRadius * 1.5f;

        HexCell cell = cells[i] = Instantiate(hexcellPrefab, new Vector3(x_world, 0, z_world), Quaternion.identity, transform);
        cell.SetCoord(HexCoord.FromOffsetCoordinates(x, z));
        cell.name = "Hexcell_" + i.ToString();

        Text text = Instantiate(coordText, canvas.transform);
        text.rectTransform.anchoredPosition3D = new Vector3(x_world, z_world, 0);
        text.text = cell.coord.ToString();
    }

    void GenChunks()
    {
        chunks = new HexChunk[chunkCount_Z * chunkCount_X];
        for (int z = 0, i = 0; z < chunkCount_Z; z++)
        {
            for (int x = 0; x < chunkCount_X; x++, i++)
            {
                HexChunk chunk = Instantiate(chunkPrefab,transform);
                chunk.name = "chunk" + i.ToString();
                chunks[i] = chunk;
            }
        }
    }

    void AddCellsToChunks()
    {
        for (int x = 0; x<cellCount_X; x++)
        {
            for (int z = 0; z < cellCount_Z; z++)
            {
                int chunk_x = x / HexMetrics.ChunkSize_X;
                int chunk_z = z / HexMetrics.ChunkSize_Z;
                int chunkCell_x = x - chunk_x * HexMetrics.ChunkSize_X;
                int chunkCell_z = z - chunk_z * HexMetrics.ChunkSize_Z;

                int chunkIndex = chunk_x + chunk_z * chunkCount_X;
                int cellIndex = x + z * cellCount_X;
                int chunkCellIndex = chunkCell_x + chunkCell_z * HexMetrics.ChunkSize_X;

                chunks[chunkIndex].AddCell(cells[cellIndex],chunkCellIndex);
            }
        }
    }

    void GenChunksMesh()
    {
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].GenMesh();
        }
    }

    void ConnectHexcell()
    {
        if (cells == null || cells.Length != cellCount_Z * cellCount_X)
        {
            Debug.LogWarning("something wrong");
            return;
        }

        for (int z = 0,cnt = 0; z < cellCount_Z; z++)
        {
            for (int x = 0; x < cellCount_X; x++,cnt++)
            {
                HexCell go = cells[cnt];

                //所有行横向连接
                if (x > 0)
                {
                    go.SetNeigbors(Direction.W, cells[cnt - 1]);
                }

                //偶数行向下连接
                if (z>0 && (z & 1) == 0)
                {
                    go.SetNeigbors(Direction.SE, cells[cnt - cellCount_X]);
                    if (x > 0)
                        go.SetNeigbors(Direction.SW, cells[cnt - cellCount_X - 1]);
                }

                //奇数行向下连接
                if ((z & 1) == 1)
                {
                    go.SetNeigbors(Direction.SW, cells[cnt - cellCount_X]);
                    if (x < cellCount_X-1)
                    {
                        go.SetNeigbors(Direction.SE, cells[cnt - cellCount_X + 1]);
                    }
                }
            }
        }
    }

    public HexCell GetCell(Vector3 worldPos)
    {
        Vector3 pos_Local = transform.InverseTransformPoint(worldPos);
        HexCoord go = HexCoord.FromLocalPos(pos_Local);
        Vector2Int pos_offset = go.ToOffsetCoordinates();
        pos_offset.Clamp(Vector2Int.zero, new Vector2Int(cellCount_X, cellCount_Z));

        return cells[pos_offset.x + pos_offset.y * cellCount_X];
    }

}
