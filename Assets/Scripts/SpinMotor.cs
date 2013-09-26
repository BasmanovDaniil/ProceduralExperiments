using UnityEngine;

public class SpinMotor : MonoBehaviour
{
    public Vector3 speed = new Vector3(0, 0.5f, 0);

    private Transform tr;

	void Start()
	{
	    tr = transform;
	}

	void Update()
	{
	    tr.rotation *= Quaternion.Euler(speed*Time.deltaTime);
	}
}
