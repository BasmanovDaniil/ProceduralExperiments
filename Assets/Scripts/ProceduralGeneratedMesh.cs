using UnityEngine;
using PGToolkit;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class ProceduralGeneratedMesh : MonoBehaviour
{
    public PGMesh.Shape shape;

    private MeshFilter meshFilter;

	void Start ()
	{
	    meshFilter = GetComponent<MeshFilter>();
	    switch (shape)
	    {
	        case PGMesh.Shape.Triangle:
                meshFilter.sharedMesh = PGMesh.Triangle(Vector3.left / 2 + Vector3.down / 2, Vector3.up, Vector3.right / 2 + Vector3.down / 2);
	            break;
	        case PGMesh.Shape.Quad:
                meshFilter.sharedMesh = PGMesh.Quad(Vector3.left / 2 + Vector3.down / 2, Vector3.right, Vector3.up);
	            break;
            case PGMesh.Shape.Plane:
                meshFilter.sharedMesh = PGMesh.Plane(Vector3.left / 2 + Vector3.down / 2, Vector3.right, Vector3.up, 1, 3);
	            break;
	        case PGMesh.Shape.Tetrahedron:
                meshFilter.sharedMesh = PGMesh.Tetrahedron(1);
	            break;
            case PGMesh.Shape.Cube:
                meshFilter.sharedMesh = PGMesh.Cube(1);
	            break;
            case PGMesh.Shape.Octahedron:
                meshFilter.sharedMesh = PGMesh.Octahedron(1);
                break;
	        case PGMesh.Shape.Icosahedron:
                meshFilter.sharedMesh = PGMesh.Icosahedron(1);
                break;
	    }
	}
}