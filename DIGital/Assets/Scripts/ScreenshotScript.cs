using UnityEngine;
using UnityEngine.InputSystem;

public class ScreenshotScript : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.pKey.wasPressedThisFrame)
        {
            string screenshotFilename = "screenshot_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            string path = System.IO.Path.Combine(Application.persistentDataPath, screenshotFilename);
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log("Screenshot will be saved to: " + path);
        }
    }
}
