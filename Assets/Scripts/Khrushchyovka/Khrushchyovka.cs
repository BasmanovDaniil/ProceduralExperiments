using System.Collections.Generic;
using UnityEngine;
using PGToolkit;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Khrushchyovka : MonoBehaviour
{
    public float width = 12;
    public float length = 23;
    public int floors = 5;
    public int entrances = 1;
    public bool attic = false;
    public RoofType roofType;

    public List<Mesh> balcony25;
    public List<Mesh> balcony30;
    public List<Mesh> wall25;
    public List<Mesh> wall30;
    public List<Mesh> window25;
    public List<Mesh> window30;
    public List<Mesh> socle25;
    public List<Mesh> socle30;
    public List<Mesh> entrance25;
    public List<Mesh> entrance30;
    public List<Mesh> entranceWall25;
    public List<Mesh> entranceWall30;
    public List<Mesh> entranceWallLast25;
    public List<Mesh> entranceWallLast30;
    public List<Mesh> attic25;
    public List<Mesh> attic30;
    public List<Mesh> roofFlat;
    public List<Mesh> roofGabled;
    public List<Mesh> roofHipped;

    private MeshFilter meshFilter;
    private float height;
    private float[] panels = {3, 2.5f};
    private System.Random random;
    private float socleHeight = 1;
    private float ceilingHeight = 3;
    private int entranceMeshIndex = 0;
    private int entranceWallMeshIndex = 0;
    private int entranceWallLastMeshIndex = 0;

    public enum RoofType
    {
        Flat,
        FlatOverhang,
        Gabled,
        Hipped,
    }

    public enum PanelType
    {
        Wall,
        Window,
        Balcony,
        Entrance,
        EntranceWall,
        EntranceWallLast,
        Socle,
        Attic,
    };

	void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        random = new System.Random();
	    GenerateBuilding();
    }

    void GenerateBuilding()
    {
        width = random.Next(10, 13);
        length = random.Next(30, 55);
        floors = random.Next(1, 7);
        entrances = (int)(length/10 - 1);
        height = ceilingHeight * floors + socleHeight;
        entranceMeshIndex = random.Next(entrance25.Count);
        entranceWallMeshIndex = random.Next(entranceWall25.Count);
        entranceWallLastMeshIndex = random.Next(entranceWallLast25.Count);
        attic = Random.value > 0.5f;
        var roofHeight = Vector3.up * height;
        if (attic)
        {
            roofHeight += Vector3.up;
        }
        roofType = RandomItem(new List<RoofType> {RoofType.Flat, RoofType.FlatOverhang, RoofType.Gabled, RoofType.Hipped});

        var corner0 = Vector3.back * width / 2 + Vector3.left * length / 2;
        var corner1 = Vector3.forward * width / 2 + Vector3.left * length / 2;
        var corner2 = Vector3.forward * width / 2 + Vector3.right * length / 2;
        var corner3 = Vector3.back * width / 2 + Vector3.right * length / 2;

        var wallSizes0 = ExteriorWallSizes(width);
        var panelPattern0 = FacadePattern(wallSizes0.Count, floors, attic);
        var wallSizes1 = ExteriorWallSizes(length);
        var panelPattern1 = FacadePattern(wallSizes1.Count, floors, attic, true);
        var wallSizes2 = ExteriorWallSizes(width);
        var panelPattern2 = FacadePattern(wallSizes2.Count, floors, attic);
        var wallSizes3 = ExteriorWallSizes(length);
        var panelPattern3 = FacadePattern(wallSizes3.Count, floors, attic, true, entrances);
        
        var combine = new List<Mesh>();
        var matrices = new List<Matrix4x4>();

        combine.Add(Facade(corner0, Vector3.forward, wallSizes0, panelPattern0));
        matrices.Add(Matrix4x4.identity);
        combine.Add(Facade(corner1, Vector3.right, wallSizes1, panelPattern1));
        matrices.Add(Matrix4x4.identity);
        combine.Add(Facade(corner2, Vector3.back, wallSizes2, panelPattern2));
        matrices.Add(Matrix4x4.identity);
        combine.Add(Facade(corner3, Vector3.left, wallSizes3, panelPattern3));
        matrices.Add(Matrix4x4.identity);
        switch (roofType)
        {
            case RoofType.Flat:
                combine.Add(RandomItem(roofFlat));
                matrices.Add(Matrix4x4.TRS(roofHeight, Quaternion.identity, new Vector3(length, 1, width)));
                break;
            case RoofType.FlatOverhang:
                combine.Add(RandomItem(roofFlat));
                matrices.Add(Matrix4x4.TRS(roofHeight, Quaternion.identity, new Vector3(length + 1, 1, width + 1)));
                break;
            case RoofType.Gabled:
                combine.Add(RandomItem(roofGabled));
                matrices.Add(Matrix4x4.TRS(roofHeight, Quaternion.identity, new Vector3(length, 1, width + 1)));
                break;
            case RoofType.Hipped:
                combine.Add(RandomItem(roofHipped));
                matrices.Add(Matrix4x4.TRS(roofHeight, Quaternion.identity, new Vector3(length + 1, 1, width + 1)));
                break;
        }
        
        meshFilter.mesh.Clear();
        meshFilter.mesh = PGMesh.CombineMeshes(combine, matrices);
        meshFilter.mesh.RecalculateBounds();
        meshFilter.mesh.Optimize();
        renderer.material.color = new Color(Random.value, Random.value, Random.value);
    }

    Mesh WindowPanel(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        var normal = Vector3.Cross((vertex1 - vertex0), (vertex2 - vertex0)).normalized;
        var window0 = vertex0 + (vertex3 - vertex0) * 0.25f + (vertex1 - vertex0) * 0.25f;
        var window1 = vertex0 + (vertex3 - vertex0) * 0.25f + (vertex1 - vertex0) * 0.75f;
        var window2 = vertex0 + (vertex3 - vertex0) * 0.75f + (vertex1 - vertex0) * 0.75f;
        var window3 = vertex0 + (vertex3 - vertex0) * 0.75f + (vertex1 - vertex0) * 0.25f;

        var mesh = new Mesh
        {
            vertices = new[] {vertex0, vertex1, vertex2, vertex3, window0, window1, window2, window3,
                                  window0, window1, window2, window3},
            normals = new[] { normal, normal, normal, normal, normal, normal, normal, normal, normal, normal, normal, normal },
            uv = new[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0),
                            new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.75f), new Vector2(0.75f, 0.75f), new Vector2(0.75f, 0.25f),
                            new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)},
            triangles = new[]
                    {
                        0, 1, 4,
                        4, 1, 5,
                        1, 2, 5,
                        5, 2, 6,
                        2, 3, 6,
                        6, 3, 7,
                        3, 0, 7,
                        7, 0, 4,

                        8, 9, 10,
                        10, 11, 8},
            subMeshCount = 2
        };
        mesh.SetTriangles(new[] { 0, 1, 4,
                                  4, 1, 5,
                                  1, 2, 5,
                                  5, 2, 6,
                                  2, 3, 6,
                                  6, 3, 7,
                                  3, 0, 7,
                                  7, 0, 4}, 0);
        mesh.SetTriangles(new[] { 8, 9, 10, 10, 11, 8 }, 1);
        return mesh;
    }

    Mesh EntrancePanel(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2, Vector3 vertex3)
    {
        var normal = Vector3.Cross((vertex1 - vertex0), (vertex2 - vertex0)).normalized;
        var window0 = vertex0 + (vertex3 - vertex0) * 0.25f + (vertex1 - vertex0) * 0.25f;
        var window1 = vertex0 + (vertex3 - vertex0) * 0.25f + (vertex1 - vertex0) * 0.75f;
        var window2 = vertex0 + (vertex3 - vertex0) * 0.75f + (vertex1 - vertex0) * 0.75f;
        var window3 = vertex0 + (vertex3 - vertex0) * 0.75f + (vertex1 - vertex0) * 0.25f;

        var mesh = new Mesh
        {
            vertices = new[] {vertex0, vertex1, vertex2, vertex3,
                            window0, window1, window2, window3,
                            window0, window1, window2, window3},
            normals = new[] { normal, normal, normal, normal,
                            normal, normal, normal, normal,
                            normal, normal, normal, normal },
            uv = new[] {new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0),
                            new Vector2(0.25f, 0.25f), new Vector2(0.25f, 0.75f),
                            new Vector2(0.75f, 0.75f), new Vector2(0.75f, 0.25f),
                            new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0)},
            triangles = new[]
                    {
                        0, 1, 4,
                        4, 1, 5,
                        1, 2, 5,
                        5, 2, 6,
                        2, 3, 6,
                        6, 3, 7,
                        3, 0, 7,
                        7, 0, 4,

                        8, 9, 10,
                        10, 11, 8},
            subMeshCount = 2
        };
        mesh.SetTriangles(new[] { 0, 1, 4,
                                    4, 1, 5,
                                    1, 2, 5,
                                    5, 2, 6,
                                    2, 3, 6,
                                    6, 3, 7,
                                    3, 0, 7,
                                    7, 0, 4}, 0);
        mesh.SetTriangles(new[] { 8, 9, 10, 10, 11, 8 }, 1);
        return mesh;
    }

    Mesh BalconyPanel(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var normal = Vector3.Cross((v1 - v0), (v3 - v0)).normalized;
        var v4 = v0 + (v1 - v0) * 0.4f;
        var v5 = v3 + (v2 - v3) * 0.4f;

        var combine = new List<Mesh>();
        var submeshMask = new List<List<int>>();
        combine.Add(PGMesh.Quad(v0, v4, v4 + normal, v0 + normal));
        submeshMask.Add(new List<int> { 0 });
        combine.Add(PGMesh.Quad(v0 + normal, v4 + normal, v5 + normal, v3 + normal));
        submeshMask.Add(new List<int> { 0 });
        combine.Add(PGMesh.Quad(v3 + normal, v5 + normal, v5, v3));
        submeshMask.Add(new List<int> { 0 });
        combine.Add(PGMesh.Quad(v0, v0 + normal, v3 + normal, v3));
        submeshMask.Add(new List<int> { 0 });

        combine.Add(PGMesh.Quad(v4, v1, v1 + normal, v4 + normal));
        submeshMask.Add(new List<int> { 1 });
        combine.Add(PGMesh.Quad(v4 + normal, v1 + normal, v2 + normal, v5 + normal));
        submeshMask.Add(new List<int> { 1 });
        combine.Add(PGMesh.Quad(v5 + normal, v2 + normal, v2, v5));
        submeshMask.Add(new List<int> { 1 });

        combine.Add(PGMesh.Quad(v2, v2 + normal, v1 + normal, v1));
        submeshMask.Add(new List<int> { 0 });

        return PGMesh.CombineMeshes(combine);
    }

    Mesh Facade(Vector3 origin, Vector3 direction, List<float> wallSizes, List<List<PanelType>> panelPattern)
    {
        var floorMeshes = new List<Mesh>();
        var facadeMeshes = new List<Mesh>();
        var matrices = new List<Matrix4x4>();
        var panelOrigin = origin;

        for (var i = 0; i < panelPattern.Count; i++)
        {
            for (var j = 0; j < panelPattern[i].Count; j++)
            {
                if (Mathf.Abs(wallSizes[j] - 2.5f) < Mathf.Epsilon)
                {
                    switch (panelPattern[i][j])
                    {
                        case PanelType.Window:
                            floorMeshes.Add(RandomItem(window25));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Balcony:
                            floorMeshes.Add(RandomItem(balcony25));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Wall:
                            floorMeshes.Add(RandomItem(wall25));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Socle:
                            floorMeshes.Add(RandomItem(socle25));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Entrance:
                            floorMeshes.Add(entrance25[entranceMeshIndex]);
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.EntranceWall:
                            floorMeshes.Add(entranceWall25[entranceWallMeshIndex]);
                            matrices.Add(Matrix4x4.TRS(panelOrigin + Vector3.up * (ceilingHeight - socleHeight), Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.EntranceWallLast:
                            floorMeshes.Add(entranceWallLast25[entranceWallLastMeshIndex]);
                            matrices.Add(Matrix4x4.TRS(panelOrigin + Vector3.up * (ceilingHeight - socleHeight), Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Attic:
                            floorMeshes.Add(RandomItem(attic25));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                    }
                }
                else
                {
                    switch (panelPattern[i][j])
                    {
                        case PanelType.Window:
                            floorMeshes.Add(RandomItem(window30));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Balcony:
                            floorMeshes.Add(RandomItem(balcony30));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Wall:
                            floorMeshes.Add(RandomItem(wall30));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Socle:
                            floorMeshes.Add(RandomItem(socle30));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Entrance:
                            floorMeshes.Add(entrance30[entranceMeshIndex]);
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.EntranceWall:
                            floorMeshes.Add(entranceWall30[entranceWallMeshIndex]);
                            matrices.Add(Matrix4x4.TRS(panelOrigin + Vector3.up * (ceilingHeight - socleHeight), Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.EntranceWallLast:
                            floorMeshes.Add(entranceWallLast30[entranceWallLastMeshIndex]);
                            matrices.Add(Matrix4x4.TRS(panelOrigin + Vector3.up * (ceilingHeight - socleHeight), Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                        case PanelType.Attic:
                            floorMeshes.Add(RandomItem(attic30));
                            matrices.Add(Matrix4x4.TRS(panelOrigin, Quaternion.LookRotation(Vector3.Cross(direction, Vector3.up)), Vector3.one));
                            break;
                    }
                }
                panelOrigin += direction * wallSizes[j];
            }
            facadeMeshes.Add(PGMesh.CombineMeshes(floorMeshes, matrices));
            floorMeshes.Clear();
            matrices.Clear();
            panelOrigin = origin + Vector3.up * (i * ceilingHeight + socleHeight);
        }
        
        return PGMesh.CombineMeshes(facadeMeshes);
    }

    List<float> ExteriorWallSizes(float wallLength)
    {
        var draft = ExteriorWallSizesDraft(wallLength);
        var wallSizes = new List<float>();
        for (var i = 0; i < draft.Length; i++)
        {
            for (var j = 0; j < draft[i]; j++)
            {
                wallSizes.Add(panels[i]);
            }
        }
        wallSizes.Shuffle();
        return wallSizes;
    }

    int[] ExteriorWallSizesDraft(float remainder, int[] draft = null, int startIndex = 0)
    {
        if (draft == null)
        {
            draft = new int[panels.Length];
            for (int i = 0; i < draft.Length; i++)
            {
                draft[i] = 0;
            }
        }
        if (remainder < panels[panels.Length - 1])
        {
            draft[draft.Length - 1] = 1;
            return draft;
        }
        for (var i = startIndex; i < panels.Length; i++)
        {
            draft[i] += (int)(remainder / panels[i]);
            remainder %= panels[i];
        }
        if (remainder > 0)
        {
            for (var i = 0; i < draft.Length; i++)
            {
                if (draft[i] != 0)
                {
                    if (i == draft.Length - 1)
                    {
                        return draft;
                    }
                    draft[i]--;
                    remainder += panels[i];
                    startIndex = i+1;
                    break;
                }
            }
            draft = ExteriorWallSizesDraft(remainder, draft, startIndex);
        }
        return draft;
    }

    List<List<PanelType>> FacadePattern(int panelCount, int floorCount, bool haveAttic=false, bool longFacade=false, int entrancesCount=0)
    {
        var panelPattern = new List<List<PanelType>>();
        var entranceIndex = panelCount / (entrances + 1);
        var entranceCount = 1;

        for (var i = 0; i < floorCount+1; i++)
        {
            panelPattern.Add(new List<PanelType>());
            for (var j = 0; j < panelCount; j++)
            {
                if (i == 0)
                {
                    if (entrancesCount > 0 && j == entranceIndex && entranceCount <= entrances)
                    {
                        panelPattern[0].Add(PanelType.Entrance);
                        entranceCount++;
                        entranceIndex = panelCount*entranceCount/(entrances + 1);
                    }
                    else
                    {
                        panelPattern[0].Add(PanelType.Socle);
                    }
                }
                else if (i == 1)
                {
                    if (panelPattern[0][j] == PanelType.Entrance)
                    {
                        panelPattern[1].Add(PanelType.EntranceWall);
                    }
                    else if (longFacade)
                    {
                        panelPattern[1].Add(PanelType.Window);
                    }
                    else
                    {
                        panelPattern[1].Add(PanelType.Wall);
                    }
                }
                else
                {
                    panelPattern[i].Add(panelPattern[i - 1][j]);
                }
                if (i == floorCount)
                {
                    if (panelPattern[i - 1][j] == PanelType.Entrance || panelPattern[i - 1][j] == PanelType.EntranceWall)
                    {
                        panelPattern[i][j] = PanelType.EntranceWallLast;
                    }
                }
            }
            if (i == 1 && !longFacade)
            {
                for (int j = 0; j <= panelPattern[1].Count / 2; j++)
                {
                    if (j != 0 && j != panelCount - 1 && Random.value > 0.5f)
                    {
                        panelPattern[1][j] = PanelType.Window;
                        panelPattern[1][panelPattern[1].Count - 1 - j] = PanelType.Window;
                    }
                }
            }
            if (i == 2)
            {
                for (int j = 0; j <= panelPattern[2].Count/2; j++)
                {
                    if (panelPattern[2][j] == PanelType.Window && panelPattern[2][panelPattern[2].Count - 1 - j] == PanelType.Window && Random.value > 0.5f)
                    {
                        panelPattern[2][j] = PanelType.Balcony;
                        panelPattern[2][panelPattern[2].Count - 1 - j] = PanelType.Balcony;
                    }
                }
            }
        }
        if (haveAttic)
        {
            panelPattern.Add(new List<PanelType>());
            for (var j = 0; j < panelCount; j++)
            {
                panelPattern[panelPattern.Count-1].Add(PanelType.Attic);
            }
        }
        return panelPattern;
    }

    T RandomItem<T>(IList<T> itemList)
    {
        return itemList[random.Next(itemList.Count)];
    }
}

static class MyExtensions
{
    public static void Shuffle<T>(this IList<T> list)
    {
        var random = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = random.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}
