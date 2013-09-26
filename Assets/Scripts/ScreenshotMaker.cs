using UnityEngine;

public class ScreenshotMaker : MonoBehaviour
{
    public float repeatRate;

    private bool capture;
    private int count;

	void Start ()
    {
        InvokeRepeating("Capture", 0, repeatRate);
	}
	
	void Update ()
    {
	    if (!Input.GetKeyDown(KeyCode.Space)) return;
	    capture = !capture;
	    if (capture) count = 0;
    }

    void Capture()
    {
        if (!capture) return;
        Application.CaptureScreenshot(count + ".png");
        count++;
    }
}
