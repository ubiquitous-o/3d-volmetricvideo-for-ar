using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PointCloudScript : MonoBehaviour
{

    [SerializeField]
    RawImage m_cameraView;
    [SerializeField]
    RawImage m_grayDepthView;

    [SerializeField]
    PointCloudVisualizer m_visualizer;
    


    [SerializeField]
    float near;
    [SerializeField]
    float far;

    [SerializeField]
    float crop_th; //meter 

    Texture2D m_CameraTexture;
    Texture2D m_DepthTexture_Float;
    Texture2D m_DepthTexture_BGRA;
    Texture2D m_DepthConfidenceTexture_R8;
    Texture2D m_DepthConfidenceTexture_RGBA;

    Vector3[] vertices = null;
    Color[] colors=null;
    
    float cx, cy, fx, fy;
    bool isScanning = true;


    void ConvertFloatToGrayScale(Texture2D txFloat, Texture2D txGray)
    {

        //Conversion of grayscale from near to far value
        int length = txGray.width * txGray.height;
        Color[] depthPixels = txFloat.GetPixels();
        Color[] colorPixels = txGray.GetPixels();

        for (int index = 0; index < length; index++)
        {

            var value = (depthPixels[index].r - near) / (far - near);

            colorPixels[index].r = value;
            colorPixels[index].g = value;
            colorPixels[index].b = value;
            colorPixels[index].a = 1;
        }
        txGray.SetPixels(colorPixels);
        txGray.Apply();
    }

    void ConvertR8ToConfidenceMap(Texture2D txR8, Texture2D txRGBA) {
        Color32[] r8 = txR8.GetPixels32();
        Color32[] rgba = txRGBA.GetPixels32();
        for (int i = 0; i < r8.Length; i++)
        {
            switch (r8[i].r)
            {
                case 0:
                    rgba[i].r = 255;
                    rgba[i].g = 0;
                    rgba[i].b = 0;
                    rgba[i].a = 255;
                    break;
                case 1:
                    rgba[i].r = 0;
                    rgba[i].g = 255;
                    rgba[i].b = 0;
                    rgba[i].a = 255;
                    break;
                case 2:
                    rgba[i].r = 0;
                    rgba[i].g = 0;
                    rgba[i].b = 255;
                    rgba[i].a = 255;
                    break;
            }
        }
        txRGBA.SetPixels32(rgba);
        txRGBA.Apply();
    }


    void ReprojectPointCloud()
    {
        m_CameraTexture = (Texture2D)m_cameraView.texture;
        m_DepthTexture_Float = (Texture2D)m_grayDepthView.texture;
        // print("Depth:" + m_DepthTexture_Float.width + "," + m_DepthTexture_Float.height);
        // print("Color:" + m_CameraTexture.width + "," + m_CameraTexture.height);
        int width_depth = m_DepthTexture_Float.width;
        int height_depth = m_DepthTexture_Float.height;
        int width_camera = m_CameraTexture.width;

        if(vertices==null || colors == null)
        {
            vertices = new Vector3[width_depth * height_depth];
            colors = new Color[width_depth * height_depth];
          
            fx = m_CameraTexture.width / 2;
            fy = m_CameraTexture.height /2;
            cx = m_CameraTexture.width / 2;
            cy = m_CameraTexture.height /2;

        }

        Color[] depthPixels = m_DepthTexture_Float.GetPixels();

        int index_dst ;
        float depth;
        
        for(int depth_y = 0; depth_y < height_depth; depth_y++)
        {
            index_dst = depth_y * width_depth;
            for(int depth_x = 0; depth_x < width_depth; depth_x++)
            {
                colors[index_dst] = m_CameraTexture.GetPixelBilinear((float)depth_x/(width_depth), (float)depth_y / (height_depth));

                depth = depthPixels[index_dst].r;
                if(depth < crop_th){
                depth = crop_th;
                }
                if (depth > near && depth < far)
                {
                    vertices[index_dst].z = depth;
                    vertices[index_dst].x = -depth * (depth_x - cx) / fx;
                    vertices[index_dst].y = -depth * (depth_y - cy) / fy;
                }
                else
                {
                    vertices[index_dst].z = -999;
                    vertices[index_dst].x = 0;
                    vertices[index_dst].y = 0;
                }
                index_dst++;
            }
        }


        m_visualizer.UpdateMeshInfo(vertices, colors);


    }

    void Update()
    {
        if (isScanning)
        {
            ReprojectPointCloud();
        }
    }


    public void SwitchScanMode(bool flg)
    {
        isScanning = flg;
        if (flg)
        {
            m_visualizer.transform.localPosition = Vector3.zero;
            m_visualizer.transform.localRotation = Quaternion.identity;
        }
        else
        {
            m_visualizer.transform.parent = null;
        }
    }

    private void Start()
    {
        SwitchScanMode(true);
    }

}
