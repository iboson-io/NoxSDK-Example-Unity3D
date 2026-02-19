using NoxSDK;
using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField]
    private GLBLoader glbLoader;
    [SerializeField]
    private bool showPreview3D = true;

    private bool intrinsicsUpdated = false;
    private XRCpuImage lastCpuImage;
    private XRCameraIntrinsics intrinsics;
    private ARAnchor currentAnchor;


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
        if (API_KEY.Length == 0 || MODEL_ID.Length == 0)
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
        if (showPreview3D)
        {
            objectDetection.GetGLBModel(OnGLBUrlReceived);
        }
    }

    private void OnGLBUrlReceived(string glbUrl)
    {
        if (glbUrl == null)
        {
            Debug.LogError("Failed to Load GLB model");
            return;
        }
        glbLoader.LoadGLBFromURL(glbUrl);
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
        glbLoader.gameObject.SetActive(false);
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

        // Add ARAnchor component
        if(currentAnchor != null)
        {
            Destroy(currentAnchor);
        }
        ARAnchor anchor = detectedParentObject.AddComponent<ARAnchor>();
        if (anchor != null)
        {
            Debug.Log("ARAnchor created at Detected position");
            currentAnchor = anchor;
        }
        else
        {
            Debug.LogError("Failed to create ARAnchor - ARAnchorManager should be in scene");
        }

        ShowDetectionOverlay();
    }

    private void ShowDetectionOverlay()
    {
        if(!showPreview3D)
        {
            return;
        }
        if(glbLoader.transform.childCount == 0)
        {
            Debug.LogError("GLB Model not loaded yet");
            return;
        }
        GameObject obj = glbLoader.transform.GetChild(0).gameObject;
        GameObject preview3D = Instantiate(obj, detectedParentObject.transform);
        preview3D.transform.localPosition = obj.transform.localPosition;
        preview3D.transform.localRotation = Quaternion.identity;
        
        StartCoroutine(FadeInFadeOut(preview3D));
    }

    private IEnumerator FadeInFadeOut(GameObject preview3D)
    {
        float alpha = 0;
        for (int i = 0; i < 15; i++)
        {
            alpha += 0.03f;
            preview3D.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, alpha);
            yield return null;
        }
        yield return new WaitForSeconds(1f);
        for (int i = 15; i > 0; i--)
        {
            alpha -= 0.03f;
            preview3D.GetComponent<Renderer>().material.color = new Color(0.5f, 0.5f, 0.5f, alpha);
            yield return null;
        }
        Destroy(preview3D, 5f);
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
