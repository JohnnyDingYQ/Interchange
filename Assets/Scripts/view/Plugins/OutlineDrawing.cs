using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://willweissman.com/unity-outlines
public class OutlineDrawing : MonoBehaviour
{
    public Shader DrawAsSolidColor;
    public Shader Outline;
    Material _outlineMaterial;
    Camera TempCam;

    void Start()
    {
        _outlineMaterial = new Material(Outline);

        //setup the second camera which will render outlined objects
        TempCam = new GameObject().AddComponent<Camera>();
        TempCam.name = "Outline Camera";
        TempCam.enabled = false;
    }

    //OnRenderImage is the hook after our scene's image has been rendered, so we can do post-processing.
    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        //set up the temporary camera
        TempCam.CopyFrom(Camera.current);
        TempCam.backgroundColor = Color.black;
        TempCam.clearFlags = CameraClearFlags.Color;
        // TempCam.transform.SetPositionAndRotation(Camera.main.transform.position, Camera.main.transform.rotation);

        //cull anything that isn't in the outline layer
        TempCam.cullingMask = 1 << LayerMask.NameToLayer("Outline");

        //allocate the video memory for the texture
        var rt = RenderTexture.GetTemporary(src.width, src.height, 0, RenderTextureFormat.ARGB32);

        //set up the camera to render to the new texture
        TempCam.targetTexture = rt;

        //use the simplest 3D shader you can find to redraw those objects
        TempCam.RenderWithShader(DrawAsSolidColor, "");

        _outlineMaterial.SetTexture("_SceneTex", src);

        // rt.filterMode = FilterMode.Point;

        //pass the temporary texture through the material, and to the destination texture.
        Graphics.Blit(rt, dst, _outlineMaterial);

        // TempCam.targetTexture = src;

        //free the video memory
        RenderTexture.ReleaseTemporary(rt);
    }
}