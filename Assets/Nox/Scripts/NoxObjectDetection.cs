using NoxSDK;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class NoxObjectDetection : MonoBehaviour
{

    private ObjectDetection objectDetection;
    [SerializeField] 
    private ARCameraManager cameraManager;
    private UnityEngine.Camera arCamera;
    public GameObject detectedParentObject;
    [SerializeField]
    private string MODEL_ID = ""; //Enter MODEL_ID from https://noxvision.ai/
    [SerializeField]
    private string API_KEY = ""; //Enter API_KEY from https://noxvision.ai/
    public TMP_Text statusText;
    private bool intrinsicsUpdated = false;
    private XRCpuImage lastCpuImage;
    private XRCameraIntrinsics intrinsics;

    // Timestamp and throttling
    private long lastTimestampUsed = 0;


    private void OnEnable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    private void OnDisable()
    {
        if (cameraManager != null)
        {
            cameraManager.frameReceived -= OnCameraFrameReceived;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        arCamera = cameraManager.GetComponent<Camera>();
        if(API_KEY.Length == 0 || MODEL_ID.Length == 0)
        {
            Debug.LogError("Enter API_KEY and MODEL_ID"); //Get API_KEY & MODEL_ID from https://noxvision.ai/
        }
        ObjectDetectionInitAsync();
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        //Debug.Log("OnCameraFrameReceived");
        if (objectDetection.isDetected)
        {
            return;
        }
        if (!objectDetection.isConnected)
        {
            return;
        }

        // Get current timestamp (in microseconds to match Java code)
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000;
        long timeDiff = (currentTimestamp - lastTimestampUsed) / 1000; // Convert to milliseconds

        // Throttle: only process if more than 1000ms (1 second) has passed
        if (timeDiff > 1000)
        {
            lastTimestampUsed = currentTimestamp;

            lastCpuImage = new XRCpuImage();
            if (!cameraManager.TryAcquireLatestCpuImage(out lastCpuImage))
            {
                return;
            }
            if (!intrinsicsUpdated)
            {
                UpdateCameraIntrinsics();
            }

            objectDetection.UpdateCameraFrame(lastCpuImage, arCamera.transform.GetWorldPose());
        }

        lastCpuImage.Dispose();
    }

    private void UpdateCameraIntrinsics()
    {
        if (!cameraManager.TryGetIntrinsics(out intrinsics))
        {
            Debug.LogWarning("Failed to acquire camera intrinsics.");
            return;
        }

        objectDetection.UpdateCameraIntrinsics(intrinsics);
        intrinsicsUpdated = true;
    }


    private async Task ObjectDetectionInitAsync()
    {
        objectDetection = new ObjectDetection();
        objectDetection.OnInitialized += OnInitialized;
        objectDetection.OnFailed += OnFailed;
        objectDetection.OnDetectionStatus += OnDetectionStatus;
        objectDetection.OnObjectTransformationUpdated += OnObjectTransformationUpdated;
        await objectDetection.InitAsync();
    }

    private void OnInitialized()
    {
        Debug.Log("ObjectDetection init");
        objectDetection.SetConfig(MODEL_ID, API_KEY);
        objectDetection.StartScan();
    }

    private void OnFailed(string error)
    {
        Debug.Log("ObjectDetection failed "+error);
        statusText.text = "Failed : "+error;
    }

    private void OnObjectTransformationUpdated(float[] transformation)
    {
        // Create Matrix4x4 from array (ARCore format)
        Matrix4x4 transformationMatrix = new Matrix4x4();
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                transformationMatrix[i, j] = transformation[i * 4 + j];
            }
        }

        // Extract position from last column
        Vector3 detectedPosition = new Vector3(
            transformationMatrix.m03,
            transformationMatrix.m13,
            transformationMatrix.m23
        );

        // Extract rotation
        Quaternion detectedRotation = transformationMatrix.rotation;

        detectedParentObject.transform.position = detectedPosition;
        detectedParentObject.transform.rotation = detectedRotation;
    }
    private void OnDetectionStatus(string status)
    {
        Debug.Log("Plugin status " + status);
        statusText.text = status;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDestroy()
    {
        objectDetection.StopScan();
        objectDetection.Dispose();
    }

}
