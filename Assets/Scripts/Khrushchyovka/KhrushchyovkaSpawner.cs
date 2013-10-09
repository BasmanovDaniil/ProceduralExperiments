using UnityEngine;

public class KhrushchyovkaSpawner : MonoBehaviour
{
    public int x;
    public int y;
    public GameObject prefab;

    private Transform tr;
	
	void Start()
	{
	    tr = transform;
        var position = new Vector3(-x*30+30, 0, -y*30+30);
	    for (int i = 0; i < x; i++)
	    {
            for (int j = 0; j < y; j++)
            {
                var clone = (GameObject)Instantiate(prefab, position, Quaternion.identity);
                clone.transform.parent = tr;
                position.x += 60;
            }
            position.x = -x * 30;
	        position.z += 60;
	    }
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
        if (Input.GetMouseButtonDown(0))
        {
            foreach (Transform clone in transform)
            {
                Destroy(clone.gameObject);
            }

            var position = new Vector3(-x * 30 + 30, 0, -y * 30 + 30);
            for (int i = 0; i < x; i++)
            {
                for (int j = 0; j < y; j++)
                {
                    var clone = (GameObject)Instantiate(prefab, position, Quaternion.identity);
                    clone.transform.parent = tr;
                    position.x += 60;
                }
                position.x = -x * 30;
                position.z += 60;
            }
        }
    }
}
