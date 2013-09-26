using UnityEngine;
using PGToolkit;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class TriangleCloud : MonoBehaviour
{
    public int count = 200;
    public int radius = 10;
    public float speed = 0.5f;

    private MeshFilter meshFilter;

	void Start ()
	{
	    meshFilter = GetComponent<MeshFilter>();

        var combine = new CombineInstance[count];

        for (int i = 0; i < count; i++)
	    {
            var offset = Random.onUnitSphere * radius;
            combine[i].mesh = PGMesh.Triangle(offset + Random.onUnitSphere, offset + Random.onUnitSphere, offset + Random.onUnitSphere);
	    }

	    var mesh = new Mesh();
        mesh.CombineMeshes(combine, true, false);
	    meshFilter.sharedMesh = mesh;
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        transform.rotation *= Quaternion.Euler(0, speed, 0);
    }
}
