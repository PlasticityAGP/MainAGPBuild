using UnityEngine;

[ExecuteInEditMode]
public class SCR_DepthTexture : MonoBehaviour
{

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.depthTextureMode = DepthTextureMode.Depth;
    }

}
