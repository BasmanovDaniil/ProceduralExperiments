using System.Collections.Generic;
using UnityEngine;

namespace PGToolkit
{
    public class PGMesh
    {
        public enum Shape
        {
            Triangle,
            Quad,
            Plane,
            Tetrahedron,
            Cube,
            Octahedron,
            Icosahedron,
        };

        public static Mesh Triangle(Vector3 vertex0, Vector3 vertex1, Vector3 vertex2)
        {
            var normal = Vector3.Cross((vertex1 - vertex0), (vertex2 - vertex0)).normalized;
            var mesh = new Mesh
            {
                vertices = new[] { vertex0, vertex1, vertex2 },
                normals = new[] { normal, normal, normal },
                uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1) },
                triangles = new[] { 0, 1, 2 }
            };
            return mesh;
        }

        public static Mesh Quad(Vector3 origin, Vector3 width, Vector3 length)
        {
            var normal = Vector3.Cross(length, width).normalized;
            var mesh = new Mesh
            {
                vertices = new[] { origin, origin + length, origin + length + width, origin + width },
                normals = new[] { normal, normal, normal, normal },
                uv = new[] { new Vector2(0, 0), new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0) },
                triangles = new[] { 0, 1, 2, 
                                    0, 2, 3}
            };
            return mesh;
        }

        public static Mesh Plane(Vector3 origin, Vector3 width, Vector3 length, int widthCount, int lengthCount)
        {
            var combine = new CombineInstance[widthCount * lengthCount];

            var i = 0;
            for (var x = 0; x < widthCount; x++)
            {
                for (var y = 0; y < lengthCount; y++)
                {
                    combine[i].mesh = Quad(origin + width * x + length * y, width, length);
                    i++;
                }
            }

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        //public static Mesh Tetrahedron(float radius)
        //{
        //    var v0 = new Vector3(0, radius, 0);
        //    var v1 = new Vector3(0, -radius * 0.333f, radius * 0.943f);
        //    var v2 = new Vector3(radius * 0.816f, -radius * 0.333f, -radius * 0.471f);
        //    var v3 = new Vector3(-radius * 0.816f, -radius * 0.333f, -radius * 0.471f);

        //    var combine = new CombineInstance[4];
        //    combine[0].mesh = Triangle(v0, v1, v2);
        //    combine[1].mesh = Triangle(v1, v3, v2);
        //    combine[2].mesh = Triangle(v0, v2, v3);
        //    combine[3].mesh = Triangle(v0, v3, v1);

        //    var mesh = new Mesh();
        //    mesh.CombineMeshes(combine, true, false);
        //    return mesh;
        //}

        public static Mesh Tetrahedron(float radius)
        {
            var tetrahedralAngle = Mathf.PI * 109.4712f / 180;
            var segmentAngle = Mathf.PI * 2 / 3;
            var currentAngle = 0f;

            var v = new Vector3[4];
            v[0] = new Vector3(0, radius, 0);
            for (var i = 1; i <= 3; i++)
            {
                v[i] = new Vector3(radius * Mathf.Sin(currentAngle) * Mathf.Sin(tetrahedralAngle),
                                    radius * Mathf.Cos(tetrahedralAngle),
                                    radius * Mathf.Cos(currentAngle) * Mathf.Sin(tetrahedralAngle));
                currentAngle = currentAngle + segmentAngle;
            }

            var combine = new CombineInstance[4];
            combine[0].mesh = Triangle(v[0], v[1], v[2]);
            combine[1].mesh = Triangle(v[1], v[3], v[2]);
            combine[2].mesh = Triangle(v[0], v[2], v[3]);
            combine[3].mesh = Triangle(v[0], v[3], v[1]);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        public static Mesh BaselessPyramid(Vector3 baseCenter, Vector3 apex, float radius, int segments, bool inverted = false)
        {
            var segmentAngle = Mathf.PI * 2 / segments;
            var currentAngle = 0f;

            var v = new Vector3[segments + 1];
            v[0] = apex;
            for (var i = 1; i <= segments; i++)
            {
                v[i] = new Vector3(radius * Mathf.Sin(currentAngle), 0,
                                   radius * Mathf.Cos(currentAngle)) + baseCenter;
                if (inverted) currentAngle -= segmentAngle;
                else currentAngle += segmentAngle;
            }

            var combine = new CombineInstance[segments];
            for (var i = 0; i < segments - 1; i++)
            {
                combine[i].mesh = Triangle(v[0], v[i + 1], v[i + 2]);
            }
            combine[combine.Length - 1].mesh = Triangle(v[0], v[v.Length - 1], v[1]);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        public static Mesh BaselessPyramid(float radius, int segments, float heignt, bool inverted = false)
        {
            if (inverted) return BaselessPyramid(Vector3.zero, Vector3.down * heignt, radius, segments, true);
            return BaselessPyramid(Vector3.zero, Vector3.up * heignt, radius, segments);
        }
        
        public static Mesh TriangleFan(Vector3 center, float radius, int segments, bool inverted = false)
        {
            return BaselessPyramid(center, center, radius, segments, inverted);
        }

        public static Mesh Pyramid(float radius, int segments, float heignt, bool grounded = false)
        {
            var combine = new CombineInstance[2];
            combine[0].mesh = BaselessPyramid(radius, segments, heignt);
            combine[1].mesh = TriangleFan(Vector3.zero, radius, segments, true);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        public static Mesh BiPyramid(float radius, int segments, float heignt)
        {
            var combine = new CombineInstance[2];
            combine[0].mesh = BaselessPyramid(radius, segments, heignt);
            combine[1].mesh = BaselessPyramid(radius, segments, heignt, true);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        public static Mesh Cube(float edge)
        {
            return Parallelepiped(Vector3.right*edge, Vector3.forward*edge, Vector3.up*edge);
        }

        public static Mesh Parallelepiped(Vector3 width, Vector3 length, Vector3 height)
        {
            var corner0 = -width/2 - length/2 - height/2;
            var corner1 = width/2 + length/2 + height/2;

            var combine = new CombineInstance[6];
            combine[0].mesh = Quad(corner0, length, width);
            combine[1].mesh = Quad(corner0, width, height);
            combine[2].mesh = Quad(corner0, height, length);
            combine[3].mesh = Quad(corner1, -width, -length);
            combine[4].mesh = Quad(corner1, -height, -width);
            combine[5].mesh = Quad(corner1, -length, -height);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        //public static Mesh Octahedron(float radius)
        //{
        //    var v = new Vector3[6];
        //    v[0] = new Vector3(0, -radius, 0);
        //    v[1] = new Vector3(-radius, 0, 0);
        //    v[2] = new Vector3(0, 0, -radius);
        //    v[3] = new Vector3(+radius, 0, 0);
        //    v[4] = new Vector3(0, 0, +radius);
        //    v[5] = new Vector3(0, radius, 0);

        //    var mesh = new Mesh
        //    {
        //        vertices = v,
        //        triangles = new[] { 0, 1, 2,
        //                            0, 2, 3,
        //                            0, 3, 4,
        //                            0, 4, 1,
        //                            5, 2, 1,
        //                            5, 3, 2,
        //                            5, 4, 3,
        //                            5, 1, 4}
        //    };
        //    mesh.RecalculateNormals();
        //    return mesh;
        //}

        //public static Mesh Octahedron(float radius)
        //{
        //    var v0 = new Vector3(0, -radius, 0);
        //    var v1 = new Vector3(-radius, 0, 0);
        //    var v2 = new Vector3(0, 0, -radius);
        //    var v3 = new Vector3(+radius, 0, 0);
        //    var v4 = new Vector3(0, 0, +radius);
        //    var v5 = new Vector3(0, radius, 0);

        //    var combine = new CombineInstance[8];
        //    combine[0].mesh = Triangle(v0, v1, v2);
        //    combine[1].mesh = Triangle(v0, v2, v3);
        //    combine[2].mesh = Triangle(v0, v3, v4);
        //    combine[3].mesh = Triangle(v0, v4, v1);
        //    combine[4].mesh = Triangle(v5, v2, v1);
        //    combine[5].mesh = Triangle(v5, v3, v2);
        //    combine[6].mesh = Triangle(v5, v4, v3);
        //    combine[7].mesh = Triangle(v5, v1, v4);

        //    var mesh = new Mesh();
        //    mesh.CombineMeshes(combine, true, false);
        //    return mesh;
        //}

        public static Mesh Octahedron(float radius)
        {
            return BiPyramid(1, 4, 1);
        }

        //public static Mesh Icosahedron(float radius)
        //{
        //    float a = 1;
        //    float b = (1 + Mathf.Sqrt(5)) / 2;
        //    float c = 0;
        //    float scale = radius / Mathf.Sqrt(a * a + b * b + c * c);
        //    a = a * scale;
        //    b = b * scale;
        //    c = c * scale;

        //    var v0 = new Vector3(-a, b, c);
        //    var v1 = new Vector3(a, b, c);
        //    var v2 = new Vector3(-a, -b, c);
        //    var v3 = new Vector3(a, -b, c);
        //    var v4 = new Vector3(c, -a, b);
        //    var v5 = new Vector3(c, a, b);
        //    var v6 = new Vector3(c, -a, -b);
        //    var v7 = new Vector3(c, a, -b);
        //    var v8 = new Vector3(b, c, -a);
        //    var v9 = new Vector3(b, c, a);
        //    var v10 = new Vector3(-b, c, -a);
        //    var v11 = new Vector3(-b, c, a);

        //    var combine = new CombineInstance[20];
        //    combine[0].mesh = Triangle(v0, v11, v5);
        //    combine[1].mesh = Triangle(v0, v5, v1);
        //    combine[2].mesh = Triangle(v0, v1, v7);
        //    combine[3].mesh = Triangle(v0, v7, v10);
        //    combine[4].mesh = Triangle(v0, v10, v11);
        //    combine[5].mesh = Triangle(v1, v5, v9);
        //    combine[6].mesh = Triangle(v5, v11, v4);
        //    combine[7].mesh = Triangle(v11, v10, v2);
        //    combine[8].mesh = Triangle(v10, v7, v6);
        //    combine[9].mesh = Triangle(v7, v1, v8);
        //    combine[10].mesh = Triangle(v3, v9, v4);
        //    combine[11].mesh = Triangle(v3, v4, v2);
        //    combine[12].mesh = Triangle(v3, v2, v6);
        //    combine[13].mesh = Triangle(v3, v6, v8);
        //    combine[14].mesh = Triangle(v3, v8, v9);
        //    combine[15].mesh = Triangle(v4, v9, v5);
        //    combine[16].mesh = Triangle(v2, v4, v11);
        //    combine[17].mesh = Triangle(v6, v2, v10);
        //    combine[18].mesh = Triangle(v8, v6, v7);
        //    combine[19].mesh = Triangle(v9, v8, v1);

        //    var mesh = new Mesh();
        //    mesh.CombineMeshes(combine, true, false);
        //    return mesh;
        //}

        public static Mesh Icosahedron(float radius)
        {
            var magicAngle = Mathf.PI*26.565f/180;
            var segmentAngle = Mathf.PI * 72 / 180;
            var currentAngle = 0f;

            var v = new Vector3[12];
            v[0] = new Vector3(0, radius, 0);
            v[11] = new Vector3(0, -radius, 0);
            
            for(var i=1; i<6; i++)
            {
                v[i] = new Vector3(radius * Mathf.Sin(currentAngle) * Mathf.Cos(magicAngle),
                    radius * Mathf.Sin(magicAngle),
                    radius * Mathf.Cos(currentAngle) * Mathf.Cos(magicAngle));
                currentAngle += segmentAngle;
            }
            currentAngle = Mathf.PI*36/180;
            for(var i=6; i<11; i++)
            {
                v[i] = new Vector3(radius * Mathf.Sin(currentAngle) * Mathf.Cos(-magicAngle),
                    radius * Mathf.Sin(-magicAngle),
                    radius * Mathf.Cos(currentAngle) * Mathf.Cos(-magicAngle));
                currentAngle += segmentAngle;
            }

            var combine = new CombineInstance[20];
            combine[0].mesh = Triangle(v[0], v[1], v[2]);
            combine[1].mesh = Triangle(v[0], v[2], v[3]);
            combine[2].mesh = Triangle(v[0], v[3], v[4]);
            combine[3].mesh = Triangle(v[0], v[4], v[5]);
            combine[4].mesh = Triangle(v[0], v[5], v[1]);

            combine[5].mesh = Triangle(v[11], v[7], v[6]);
            combine[6].mesh = Triangle(v[11], v[8], v[7]);
            combine[7].mesh = Triangle(v[11], v[9], v[8]);
            combine[8].mesh = Triangle(v[11], v[10], v[9]);
            combine[9].mesh = Triangle(v[11], v[6], v[10]);

            combine[10].mesh = Triangle(v[2], v[1], v[6]);
            combine[11].mesh = Triangle(v[3], v[2], v[7]);
            combine[12].mesh = Triangle(v[4], v[3], v[8]);
            combine[13].mesh = Triangle(v[5], v[4], v[9]);
            combine[14].mesh = Triangle(v[1], v[5], v[10]);

            combine[15].mesh = Triangle(v[6], v[7], v[2]);
            combine[16].mesh = Triangle(v[7], v[8], v[3]);
            combine[17].mesh = Triangle(v[8], v[9], v[4]);
            combine[18].mesh = Triangle(v[9], v[10], v[5]);
            combine[19].mesh = Triangle(v[10], v[6], v[1]);

            var mesh = new Mesh();
            mesh.CombineMeshes(combine, true, false);
            return mesh;
        }

        //public static Mesh Cylinder(Vector3 origin, Vector3 width, Vector3 length, int widthCount, int lengthCount)
        //{
        //    var combine = new CombineInstance[widthCount * lengthCount];

        //    var i = 0;
        //    for (var x = 0; x < widthCount; x++)
        //    {
        //        for (var y = 0; y < widthCount; y++)
        //        {
        //            combine[i].mesh = Quad(origin + width * x + length * y, width, length);
        //            i++;
        //        }
        //    }

        //    var mesh = new Mesh();
        //    mesh.CombineMeshes(combine, true, false);
        //    return mesh;
        //}

        //public static Vector3 MiddlePoint(Vector3 vertex0, Vector3 vertex1)
        //{
        //    return (vertex0 + vertex1) * 0.5f;
        //}

        //public static Mesh Subdivide(Mesh mesh, int subdivisions)
        //{
        //    for (var i = 0; i < subdivisions; i++)
        //    {
        //        List<int> ilist = new List<int>();
        //        for (var f = 0; f < data.IndexList.Count; f += 3)
        //        {
        //            int v1 = data.IndexList[f + 0];
        //            int v2 = data.IndexList[f + 1];
        //            int v3 = data.IndexList[f + 2];

        //            int va = GetMiddlePoint(data.VertexList, v1, v2, indexCache);
        //            int vb = GetMiddlePoint(data.VertexList, v2, v3, indexCache);
        //            int vc = GetMiddlePoint(data.VertexList, v3, v1, indexCache);

        //            AddTriangle(ilist, v1, va, vc);
        //            AddTriangle(ilist, v2, vb, va);
        //            AddTriangle(ilist, v3, vc, vb);
        //            AddTriangle(ilist, va, vb, vc);

        //            if (data.VertexList.Count > 64950)
        //            {
        //                UnityEngine.Debug.LogError("Subdivisions is too large => Mesh is limited to 65000 vertices!");
        //                break;
        //            }
        //        }
        //        data.IndexList = ilist;
        //    }

        //    return mesh;
        //}

        //public static Mesh Ring(int segmentCount, Vector3 centre, float radius, float v, bool buildTriangles)
        //{
        //    var vertices = new List<Vector3>();
        //    var uv = new List<Vector2>();
        //    var normals = new List<Vector3>();
        //    var triangles = new List<int>();

        //    float angleInc = (Mathf.PI * 2.0f) / segmentCount;

        //    for (int i = 0; i <= segmentCount; i++)
        //    {
        //        float angle = angleInc * i;

        //        var unitPosition = Vector3.zero;
        //        unitPosition.x = Mathf.Cos(angle);
        //        unitPosition.z = Mathf.Sin(angle);

        //        vertices.Add(centre + unitPosition * radius);
        //        normals.Add(unitPosition);
        //        uv.Add(new Vector2((float)i / segmentCount, v));

        //        if (i > 0 && buildTriangles)
        //        {
        //            int baseIndex = vertices.Count - 1;

        //            int vertsPerRow = segmentCount + 1;

        //            int index0 = baseIndex;
        //            int index1 = baseIndex - 1;
        //            int index2 = baseIndex - vertsPerRow;
        //            int index3 = baseIndex - vertsPerRow - 1;

        //            triangles.AddRange(new [] {index0, index2, index1});
        //            triangles.AddRange(new [] {index2, index3, index1});
        //        }
        //    }
        //    var mesh = new Mesh
        //        {
        //            vertices = vertices.ToArray(),
        //            normals = normals.ToArray(),
        //            uv = uv.ToArray(),
        //            triangles = triangles.ToArray()
        //        };

        //    return mesh;
        //}
    }
}
