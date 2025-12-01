using System.Threading.Tasks;
using UnityEngine;
using GLTFast;

public class GLBLoader : MonoBehaviour
{
    private GltfImport gltfImport;
    [SerializeField]
    private Material translucentMat;
    private bool modelLoaded = false;

    public async void LoadGLBFromURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("GLB URL is empty or null");
            return;
        }

        gltfImport = new GltfImport();
        bool success = await gltfImport.Load(url);

        if (success)
        {
            Debug.Log("GLB model loaded successfully");
            await gltfImport.InstantiateMainSceneAsync(transform);
            modelLoaded = true;
            CalculateMeshBoundsAndScale();
        }
        else
        {
            Debug.LogError("Failed to load GLB model");
        }
    }

    private void CalculateMeshBoundsAndScale()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        
        if (renderers.Length == 0)
        {
            Debug.LogWarning("No renderers found to calculate bounds");
            return;
        }

        // Calculate combined bounds of all renderers
        Bounds combinedBounds = renderers[0].bounds;
        renderers[0].material = translucentMat; // Apply translucent material
        for (int i = 1; i < renderers.Length; i++)
        {
            combinedBounds.Encapsulate(renderers[i].bounds);
            renderers[i].material = translucentMat; // Apply translucent material
        }

        // Get the largest dimension
        float largestDimension = Mathf.Max(combinedBounds.size.x, combinedBounds.size.y, combinedBounds.size.z);
        
        if (largestDimension <= 0)
        {
            Debug.LogWarning("Invalid bounds size");
            return;
        }

        // Calculate scale factor to make largest dimension 0.5 meters
        float targetSize = 0.5f;
        float scaleFactor = targetSize / largestDimension;

        // Apply scale to transform
        transform.localScale *= scaleFactor;
        
        Debug.Log($"Scaled model from {largestDimension:F3}m to {targetSize}m (scale factor: {scaleFactor:F3})");
    }

    private void Update()
    {
        if (modelLoaded)
        {
            transform.Rotate(Vector3.up, 40f * Time.deltaTime);
        }
    }

    private void OnDestroy()
    {
        if (gltfImport != null)
        {
            gltfImport.Dispose();
        }
    }
}
