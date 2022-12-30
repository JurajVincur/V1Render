using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTrace_Shader : MonoBehaviour
{
    public Camera renderCam;
    public bool toRenderTextureFlag = true;
    public Renderer target;
    public Transform ellipsoid;

    // Start is called before the first frame update
    protected void LateUpdate()
    {
        var pm = GL.GetGPUProjectionMatrix(renderCam.projectionMatrix, toRenderTextureFlag);
        target.material.SetMatrix("_matrixP", pm);
        target.material.SetMatrix("_matrixIP", pm.inverse);

        target.material.SetMatrix("_matrixCW2L", renderCam.worldToCameraMatrix);
        target.material.SetMatrix("_matrixCL2W", renderCam.cameraToWorldMatrix);

        target.material.SetMatrix("_matrixEW2L", ellipsoid.worldToLocalMatrix);
        target.material.SetMatrix("_matrixEL2W", ellipsoid.localToWorldMatrix);
        target.material.SetVector("_escale", ellipsoid.localScale);

        target.material.SetMatrix("_matrixDW2L", target.worldToLocalMatrix);
        target.material.SetMatrix("_matrixDL2W", target.localToWorldMatrix);
    }
}