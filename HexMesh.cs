using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour {

    public List<Vector3> vertices = new List<Vector3>();
    public List<int> indices = new List<int>();

    MeshFilter meshFilter;
    MeshCollider meshCollider;
    Mesh mesh ;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        mesh = new Mesh();
    }

    public void GenHexmapMesh(HexCell[] cells)
    {
        mesh.name = "HexCell";

        indices.Clear();
        vertices.Clear();
        mesh.Clear();

        for (int i = 0; i < cells.Length; i++)
        {
            GenCellMesh(cells[i]);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = indices.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = meshCollider.sharedMesh = mesh;
    }

    void GenCellMesh(HexCell cell)
    {
        for (Direction dir = 0; dir <= Direction.NW; dir++)
        {
            Triangulate(cell, dir);
        }
    }

    void Triangulate(HexCell cell,Direction dir)
    {
        Vector3 center = cell.Position;
        //center.y = HexMetrics.GetElevationHeight(cell.Elevation);
        Vector3 v1 = center + HexMetrics.SolidCorner(dir);
        Vector3 v2 = center + HexMetrics.GetNextSolidCorner(dir);
        Vector3 e1 = Vector3.Lerp(v1, v2, 0.333f);
        Vector3 e2 = Vector3.Lerp(v1, v2, 0.666f);
        AddTriangle(center, v1, e1);
        AddTriangle(center, e1, e2);
        AddTriangle(center, e2, v2);

        if (dir <= Direction.SE )
            GenBridge(cell, dir, v1, v2,e1,e2);
    }

    void GenBridge(HexCell cell, Direction dir, Vector3 v1,Vector3 v2,Vector3 ie1,Vector3 ie2)
    {
        if (cell.GetNeigbors(dir) == null)
            return;

        Vector3 bridge = HexMetrics.GetBridge(cell, dir);
        BridgeType bridgeType = GetBridgeType(cell,dir);

        //生成两个六边形间的连接
        //只有slope类型的连接会生成梯度上升
        if (bridgeType == BridgeType.Slope)
            GenSlopBridge(bridge, v1, v2);
        else
            GenFlatBridge(bridge, v1, v2);
        //生成三个六边形的间的三角形
        if (dir != Direction.SE && cell.GetNeigbors(dir.Next())!=null)
        {
            Vector3 begin = v2;
            Vector3 left = begin + HexMetrics.GetBridge(cell, dir);
            Vector3 right = begin + HexMetrics.GetBridge(cell, dir.Next());

            HexCell leftCell = cell.GetNeigbors(dir);
            HexCell rightCell = cell.GetNeigbors(dir.Next());

            int beginLevel = cell.Elevation;
            int leftLevel = leftCell.Elevation;
            int rightLevel = rightCell.Elevation;

            //这里需要将三个点按高度排序，传参又要符合特定顺序
            if (beginLevel < leftLevel)
            {
                if (beginLevel < rightLevel)
                    GenCornerTriangle(begin, cell, left, leftCell, right, rightCell);
                else
                    GenCornerTriangle(right, rightCell, begin, cell, left, leftCell);
            }
            else
            {
                if (leftLevel < rightLevel)
                    GenCornerTriangle(left, leftCell, right, rightCell, begin, cell);
                else
                    GenCornerTriangle(right, rightCell, begin, cell, left, leftCell);
            }
        }

    }

    //v1,v2为两个六边形的相接边的点
    void GenSlopBridge(Vector3 bridge,Vector3 v1,Vector3 v2)
    {
        Vector3 v3 = v1, v4 = v2;
        Vector3 ie1 = Vector3.Lerp(v3, v4, 0.333f);
        Vector3 ie2 = Vector3.Lerp(v3, v4, 0.666f);

        for (int i = 0, step = 1; i < HexMetrics.StepAmount; i++, step++)
        {
            Vector3 delta = HexMetrics.StepLerp(Vector3.zero, bridge, step);
            Vector3 v5 = v1 + delta;
            Vector3 v6 = v2 + delta;
            Vector3 oe1 = Vector3.Lerp(v5, v6, 0.333f);
            Vector3 oe2 = Vector3.Lerp(v5, v6, 0.666f);
            AddQuad(v3, v5, oe1, ie1);
            AddQuad(ie1, oe1, oe2, ie2);
            AddQuad(ie2, oe2, v6, v4);

            v3 = v5;
            v4 = v6;
            ie1 = oe1;
            ie2 = oe2;
        }
    }

    void GenFlatBridge(Vector3 bridge, Vector3 v1, Vector3 v2)
    {
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        Vector3 ie1 = Vector3.Lerp(v1, v2, 0.333f);
        Vector3 ie2 = Vector3.Lerp(v1, v2, 0.666f);
        Vector3 oe1 = Vector3.Lerp(v3, v4, 0.333f);
        Vector3 oe2 = Vector3.Lerp(v3, v4, 0.666f);

        AddQuad(v1, v3, oe1, ie1);
        AddQuad(ie1, oe1, oe2, ie2);
        AddQuad(ie2, oe2, v4, v2);
    }

    void GenCornerTriangle(Vector3 low, HexCell lowCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        BridgeType ToLeft = HexMetrics.GetBridgeType(lowCell.Elevation, leftCell.Elevation);
        BridgeType ToRight = HexMetrics.GetBridgeType(lowCell.Elevation, rightCell.Elevation);
        BridgeType Opposite = HexMetrics.GetBridgeType(leftCell.Elevation, rightCell.Elevation);

        //0-Flate,1-Slope,2-Cliff
        int[] type = new int[3];
        type[(int)ToLeft]++;
        type[(int)ToRight]++;
        type[(int)Opposite]++;

        if (type[1] == 2 && type[0] == 1)
        {
            GenCornerTriangle_SFS(low, lowCell, left, leftCell, right, rightCell);
        }
        else if (type[1] == 2 && type[2] == 1)
        {
            GenCornerTriangle_SSC(low, lowCell, left, leftCell, right, rightCell);
        }
        else if (type[2] == 2 && type[1] == 1)
        {
            if (leftCell.Elevation > rightCell.Elevation)
                GenCornerTriangle_SCCL(low, lowCell, left, leftCell, right, rightCell);
            else
                GenCornerTriangle_SCCR(low, lowCell, left, leftCell, right, rightCell);
        }
        else
        {
            GenCornerTriangle_Flat(low, lowCell, left, leftCell, right, rightCell);
        }
    }

    //用于SFS
    void GenCornerTriangle_SFS(Vector3 low, HexCell lowCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Vector3 unique,unique_L, unique_R;

        if (lowCell.Elevation == leftCell.Elevation)
        {
            unique = right;
            unique_L = low;
            unique_R = left;
        }
        else if (lowCell.Elevation == rightCell.Elevation)
        {
            unique = left;
            unique_L = right;
            unique_R = low;
        }
        else
        {
            unique = low;
            unique_L = left;
            unique_R = right;
        }

        Vector3 v3 = HexMetrics.StepLerp(unique, unique_L, 1);
        Vector3 v4 = HexMetrics.StepLerp(unique, unique_R, 1);
        AddTriangle(unique, v3, v4);

        for (int i = 2; i <=HexMetrics.StepAmount; i++)
        {
            Vector3 v5 = HexMetrics.StepLerp(unique, unique_L, i);
            Vector3 v6 = HexMetrics.StepLerp(unique, unique_R, i);
            AddQuad(v3, v5, v6, v4);
            v3 = v5;
            v4 = v6;
        }
    }

    //用于FFF和CFC
    void GenCornerTriangle_Flat(Vector3 low, HexCell lowCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        AddTriangle(low, left, right);
    }

    void GenCornerTriangle_SSC(Vector3 low, HexCell lowCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        Vector3 mid, mid_L,mid_R;

        if (right.y < left.y)
        {
            mid = right;
            mid_L = low;
            mid_R = left;
        }
        else
        {
            mid = left;
            mid_L = right;
            mid_R = low;
        }

        Vector3 disturb_L = HexMetrics.DisturbXZ(mid_L);
        Vector3 disturb_R = HexMetrics.DisturbXZ(mid_R);
        Vector3 disturb_mid = HexMetrics.DisturbXZ(mid);
        Vector3 junction = Vector3.Lerp(disturb_L, disturb_R, (disturb_L.y - disturb_mid.y) / (disturb_L.y - disturb_R.y));

        Vector3 v1 = mid;
        for (int i = 1; i <=HexMetrics.StepAmount; i++)
        {
            Vector3 v2 = HexMetrics.StepLerp(mid, mid_L, i);
            AddTriangleUndisturbed(HexMetrics.DisturbXZ( v1),HexMetrics.DisturbXZ( v2), junction);

            v1 = v2;
        }

        Vector3 v3 = mid;
        for (int i = 1; i <= HexMetrics.StepAmount; i++)
        {
            Vector3 v4 = HexMetrics.StepLerp(mid, mid_R, i);
            AddTriangleUndisturbed(HexMetrics.DisturbXZ(v4), HexMetrics.DisturbXZ(v3), junction);
            v3 = v4;
        }
    }

    //leftCell最高，并且low left right 呈顺时针方向
    void GenCornerTriangle_SCCL(Vector3 low, HexCell lowCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        BridgeType oppsite = HexMetrics.GetBridgeType(rightCell.Elevation, leftCell.Elevation);

        Vector3 disturb_low = HexMetrics.DisturbXZ(low);
        Vector3 disturb_left = HexMetrics.DisturbXZ(left);
        Vector3 disturb_right = HexMetrics.DisturbXZ(right);

        float t = ((disturb_right - disturb_low).y / (disturb_left - disturb_low).y);
        Vector3 junction = Vector3.Lerp(disturb_low, disturb_left, t);

        if (oppsite == BridgeType.Slope)
        {
            AddTriangleUndisturbed(HexMetrics.DisturbXZ( low), junction,HexMetrics.DisturbXZ( right));

            Vector3 v1 = right;
            for (int i = 1; i <= HexMetrics.StepAmount; i++)
            {
                Vector3 v2 = HexMetrics.StepLerp(right, left, i);
                AddTriangleUndisturbed(HexMetrics.DisturbXZ(v2), HexMetrics.DisturbXZ(v1), junction);
                v1 = v2;
            }
        }
        else
        {
            AddTriangleUndisturbed(HexMetrics.DisturbXZ(left),  HexMetrics.DisturbXZ(right), junction);

            Vector3 v1 = low;
            for (int i = 1; i <= HexMetrics.StepAmount; i++)
            {
                Vector3 v2 = HexMetrics.StepLerp(low, right, i);
                AddTriangleUndisturbed(HexMetrics.DisturbXZ(v2), HexMetrics.DisturbXZ(v1), junction);
                v1 = v2;
            }

        }

    }


    //rightCell最高 Low left Right呈现顺时针方向
    void GenCornerTriangle_SCCR(Vector3 low, HexCell lowCell, Vector3 left, HexCell leftCell, Vector3 right, HexCell rightCell)
    {
        BridgeType oppsite = HexMetrics.GetBridgeType(rightCell.Elevation, leftCell.Elevation);

        Vector3 disturb_low = HexMetrics.DisturbXZ(low);
        Vector3 disturb_left = HexMetrics.DisturbXZ(left);
        Vector3 disturb_right = HexMetrics.DisturbXZ(right);

        float t = (disturb_left - disturb_low).y / (disturb_right - disturb_low).y;
        Vector3 junction = Vector3.Lerp(disturb_low, disturb_right, t);

        if (oppsite == BridgeType.Slope)
        {
            AddTriangleUndisturbed(disturb_low, disturb_left, junction);

            Vector3 v1 = left;
            for (int i = 1; i <= HexMetrics.StepAmount; i++)
            {
                Vector3 v2 = HexMetrics.StepLerp(left, right, i);
                AddTriangleUndisturbed(v1.DisturbXZ(), v2.DisturbXZ(), junction);
                v1 = v2;
            }
        }
        else
        {
            AddTriangleUndisturbed(disturb_left, disturb_right, junction);

            Vector3 v1 = low;
            for (int i = 1; i <= HexMetrics.StepAmount; i++)
            {
                Vector3 v2 = HexMetrics.StepLerp(low, left, i);
                AddTriangleUndisturbed(v1.DisturbXZ(), v2.DisturbXZ(), junction);
                v1 = v2;
            }

        }

    }


    public BridgeType GetBridgeType(HexCell hexcell, Direction dir)
    {
        return HexMetrics.GetBridgeType(hexcell.Elevation, hexcell.GetNeigbors(dir).Elevation);
    }

    void AddTriangle(Vector3 v1,Vector3 v2,Vector3 v3)
    {
        int cnt = vertices.Count;
        vertices.Add(HexMetrics.DisturbXZ(v1));
        vertices.Add(HexMetrics.DisturbXZ(v2));
        vertices.Add(HexMetrics.DisturbXZ(v3));                      
        indices.Add(cnt);
        indices.Add(cnt + 1);
        indices.Add(cnt + 2);
    }

    void AddTriangleUndisturbed(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int cnt = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        indices.Add(cnt);
        indices.Add(cnt + 1);
        indices.Add(cnt + 2);
    }

    //从上往下看，顺时针方向时法线朝上
    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3,Vector3 v4)
    {
        int cnt = vertices.Count;
        vertices.Add(HexMetrics.DisturbXZ(v1));
        vertices.Add(HexMetrics.DisturbXZ(v2));
        vertices.Add(HexMetrics.DisturbXZ(v3));
        vertices.Add(HexMetrics.DisturbXZ(v4));
        indices.Add(cnt);
        indices.Add(cnt + 1);
        indices.Add(cnt + 3);
        indices.Add(cnt + 1);
        indices.Add(cnt + 2);
        indices.Add(cnt + 3);
    }

}
