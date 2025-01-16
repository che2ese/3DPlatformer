using UnityEngine;
using System.Collections;
using System.IO;

public class ScreenCaptureScript : MonoBehaviour
{
    public string shareText;
    public string screenshotName = "Screenshot.png";

    public void CaptureScreenshotAndShare()
    {
        StartCoroutine(TakeScreenshotAndShare());
    }

    private IEnumerator TakeScreenshotAndShare()
    {
        // Wait for end of frame to avoid capturing UI overlay
        yield return new WaitForEndOfFrame();

        // Take screenshot
        Texture2D screenshot = ScreenCapture.CaptureScreenshotAsTexture();

        // Share screenshot
        ShareScreenshot(screenshot, screenshotName, shareText);

        // Free memory
        Destroy(screenshot);
    }

    private void ShareScreenshot(Texture2D screenshot, string filename, string shareText)
    {
        // Save screenshot to persistent data path
        string path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllBytes(path, screenshot.EncodeToPNG());

        // Share screenshot using native sharing intent
        //new NativeShare().AddFile(path).SetText(shareText).Share();
    }
}