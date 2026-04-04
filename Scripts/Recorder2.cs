using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

public class Recorder2 : MonoBehaviour
{
    #region VARIABLES
        public RecorderSettings settings;
        private BackgroundFrameWriter imageWriter;

        public static bool showCameraPreview = false;

        List<CameraPose> cameraPoses;
        private int frameNr = 0;
        private float timeElapsed = 0f;

        private List<CameraPositionData> positionDataList = new List<CameraPositionData>();

        private bool hasRecordingFailed = false;
    #endregion

    public async Task<bool> Record(RecorderSettings settings)
    {
        imageWriter = new();
        this.settings = settings;
        Directory.CreateDirectory(settings.savePath);

        if (settings.shotType == ShotTypes.ArcLeft || settings.shotType == ShotTypes.ArcRight)
            cameraPoses = GenerateCameraPathArc(settings, transform);
        else cameraPoses = GenerateCameraPath(settings, transform);

        GameObject test = new();
        foreach (CameraPose pos in cameraPoses)
        {
            test.transform.position = pos.position;
            test.transform.rotation = pos.rotation;
            Debug.DrawRay(test.transform.position, test.transform.forward * 5f, Color.red, 2f);
        }

        if (!GetComponent<VisibilityTester>().WillShotBeValid(cameraPoses)) { hasRecordingFailed = true; }

        else
        {
            //Debug.DrawRay(settings.interestPoint, Vector3.up * 7f, Color.green, 2f); // show current interestPoint
            await CaptureFramesAsync();
        }

        if (hasRecordingFailed) DeleteFailedRecording();

        return hasRecordingFailed;
    }

    private Task CaptureFramesAsync()
    {
        TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
        StartCoroutine(CaptureFrames(tcs));
        return tcs.Task;
    }

    private IEnumerator CaptureFrames(TaskCompletionSource<bool> tcs)
    {
        GetComponent<Camera>().enabled = showCameraPreview;

        while (frameNr < settings.duration * settings.fps)
        {
            yield return new WaitForSeconds(1f / settings.fps);

            CaptureFrame();
            timeElapsed += 1f / settings.fps;
        }

        imageWriter.Flush();
        imageWriter.Stop();

        MiscFunctions.SerializeCameraPositions(positionDataList, settings.savePath);

        tcs.SetResult(true);  // Signal completion
    }

    private void CaptureFrame()  // czy ten background writer cokolwiek daje bez async gpu request?
    {
        HandleCameraMotion(cameraPoses[frameNr]);
        SaveCameraPosition();

        Camera cam = GetComponent<Camera>();
        RenderTexture rt = RenderTexture.GetTemporary(settings.resolution, settings.resolution, 24);
        cam.targetTexture = rt;
        cam.Render();

        RenderTexture.active = rt;

        Texture2D tex = new Texture2D(settings.resolution, settings.resolution, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, settings.resolution, settings.resolution), 0, 0);
        tex.Apply();

        byte[] jpgBytes = tex.EncodeToJPG(95);
        string filename = Path.Combine(settings.savePath, $"frame_{frameNr:D04}.jpg");
        imageWriter.Enqueue(filename, jpgBytes);

        // Clean up
        RenderTexture.active = null;
        cam.targetTexture = null;
        RenderTexture.ReleaseTemporary(rt);
        Destroy(tex);

        frameNr++;
    }

    private void HandleCameraMotion(CameraPose cameraPose)
    {
        Debug.DrawRay(settings.interestPoint, Vector3.up * 7f, Color.green, 2f);//settings.duration / settings.fps); // show current interestPoint

        Camera cam = GetComponent<Camera>();
        cam.transform.position = cameraPose.position;
        cam.transform.rotation = cameraPose.rotation;

        // Draw a ray in the direction the camera is looking
        //Debug.DrawRay(cam.transform.position, cam.transform.forward * 5f, Color.cyan, settings.duration - timeElapsed);

        if (cameraPose.fieldOfView > 0.1f)
            cam.fieldOfView = cameraPose.fieldOfView; 
    }

    public List<CameraPose> GenerateCameraPath(RecorderSettings settings, Transform baseCameraTransform)
    {
        List<CameraPose> poses = new List<CameraPose>();
        int totalFrames = (int)(settings.duration * settings.fps);

        float angleStep = settings.rotationSpeed / settings.fps;

        for (int frame = 0; frame < totalFrames; frame++)
        {
            float t = (float)frame / totalFrames;
            float progress = t;

            Vector3 position = Vector3.Lerp(settings.startPoint, settings.endPoint, progress);
            Quaternion rotation = baseCameraTransform.rotation;
            float fov = Mathf.Lerp(settings.startFOV, settings.endFOV, progress);

            switch (settings.shotType)
            {
                case ShotTypes.Static:
                case ShotTypes.ZoomIn:
                case ShotTypes.ZoomOut:
                case ShotTypes.PushIn:
                case ShotTypes.PushOut:
                case ShotTypes.SidewaysLeft:
                case ShotTypes.SidewaysRight:
                case ShotTypes.VerticalUp:
                case ShotTypes.VerticalDown:
                case ShotTypes.DollyZoomIn:
                case ShotTypes.DollyZoomOut:
                    break;

                case ShotTypes.PanLeft:
                case ShotTypes.PanRight:
                    rotation = Quaternion.Euler(0f, angleStep * frame, 0f) * baseCameraTransform.rotation;
                    break;

                case ShotTypes.RollLeft:
                case ShotTypes.RollRight:
                    rotation = baseCameraTransform.rotation * Quaternion.Euler(0f, 0f, angleStep * frame);
                    break;

                case ShotTypes.WhipPan:
                    int currentRepetition = Mathf.FloorToInt(progress * settings.repetitions);
                    float actualPan = ((currentRepetition % 2) == 0 ? 1 : -1) * angleStep * frame;
                    rotation = Quaternion.Euler(0f, actualPan, 0f) * baseCameraTransform.rotation;
                    break;
            }

            poses.Add(new CameraPose(position, rotation, fov));
        }

        return poses;
    }

    public List<CameraPose> GenerateCameraPathArc(RecorderSettings settings, Transform baseCameraTransform)
    {
        List<CameraPose> poses = new List<CameraPose>();
        int totalFrames = (int)(settings.duration * settings.fps);

        Transform pivotTransform = baseCameraTransform.parent;
        Vector3 originalPivotPos = pivotTransform.position;
        Quaternion originalPivotRot = pivotTransform.rotation;
        float angleStep = settings.rotationSpeed / settings.fps;

        for (int frame = 0; frame < totalFrames; frame++)
        {
            float angle = angleStep * frame;

            Vector3 position = baseCameraTransform.position;
            Quaternion rotation = baseCameraTransform.rotation;
            float fov = settings.startFOV;

            switch (settings.shotType)
            {
                case ShotTypes.ArcLeft:
                    pivotTransform.rotation = originalPivotRot * Quaternion.Euler(0f, angle, 0f);
                    position = pivotTransform.TransformPoint(baseCameraTransform.localPosition);
                    rotation = pivotTransform.rotation * baseCameraTransform.localRotation;
                    fov = settings.startFOV;
                    break;

                case ShotTypes.ArcRight:
                    pivotTransform.rotation = originalPivotRot * Quaternion.Euler(0f, -angle, 0f);
                    position = pivotTransform.TransformPoint(baseCameraTransform.localPosition);
                    rotation = pivotTransform.rotation * baseCameraTransform.localRotation;
                    fov = settings.startFOV;
                    break;
            }

            poses.Add(new CameraPose(position, rotation, fov));
        }
        pivotTransform.position = originalPivotPos;
        pivotTransform.rotation = originalPivotRot;

        return poses;
    }

    public void DeleteFailedRecording()
    {
        Directory.Delete(settings.savePath, true);
        Debug.LogWarning($"Deleted recording folder: {settings.savePath}");
    }

    private void SaveCameraPosition()
    {
        positionDataList.Add(new CameraPositionData
        {
            shotType = settings.shotType,
            shotPlanId = settings.shotPlanId,
            timestamp = timeElapsed,
            position = transform.position,
            direction = transform.forward,
            fov = GetComponent<Camera>().fieldOfView
        });
    }
}