using NoxSDK;
using System;
using System.Threading.Tasks;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI;
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
    public string MODEL_ID = ""; //Enter MODEL_ID from https://noxvision.ai/
    [SerializeField]
    public string API_KEY = ""; //Enter API_KEY from https://noxvision.ai/
    public TMP_Text statusText;
    public Button scanButton;
    private bool intrinsicsUpdated = false;
    private XRCpuImage lastCpuImage;
    private XRCameraIntrinsics intrinsics;


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

        detectedParentObject.SetActive(false);
        scanButton.onClick.AddListener(StartScan);
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        //Debug.Log("OnCameraFrameReceived");
        if (!objectDetection.isScanning)
        {
            return;
        }
        if(objectDetection.isProcessing)
        {
            return;
        }

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
        objectDetection.OnDetected += OnDetected;
        await objectDetection.InitAsync();
    }

    public void StartScan()
    {
        if (objectDetection.isProcessing)
            return;

        statusText.text = "Scanning...";
        objectDetection.StartScan();
        scanButton.interactable = false;
    }

    private void OnInitialized()
    {
        Debug.Log("ObjectDetection init");
        objectDetection.SetConfig(MODEL_ID, API_KEY);
    }

    private void OnFailed(string error)
    {
        Debug.Log(error);
        statusText.text = error;
        scanButton.interactable = true;
    }

    private void OnDetected(float[] transformation)
    {
        statusText.text = "Detected";
        scanButton.interactable = true;
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
        detectedParentObject.SetActive(true);
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
