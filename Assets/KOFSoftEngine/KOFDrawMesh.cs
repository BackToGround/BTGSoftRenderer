//#define DrawCustomCube

/**
The MIT License (MIT)
Copyright (c) 2017/1/7 tachen
 * 
 */

using UnityEngine;
using System.Collections;

using ProtoTurtle.BitmapDrawing;

[ExecuteInEditMode]
public class KOFDrawMesh : MonoBehaviour
{
    public Mesh MeshData;

    public Color ClearColor;

    public Color LineColor;

    public float focalLength = 1f;

    public Vector3 rotation;

    private Texture2D frontBuffer;

    public Vector3 position;

    public Vector3 scale = Vector3.one;

    public bool UpdateData = true;

    private Material mMaterial;

    private Matrix4x4 ScaleMatrix
    {
        get
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(scale.x, 0f, 0f, 0f));
            matrix.SetRow(1, new Vector4(0f, scale.y, 0f, 0f));
            matrix.SetRow(2, new Vector4(0f, 0f, scale.z, 0f));
            matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
            return matrix;
        }
    }

    private Matrix4x4 PostionMatrix
    {
        get
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(1f, 0f, 0f, position.x));
            matrix.SetRow(1, new Vector4(0f, 1f, 0f, position.y));
            matrix.SetRow(2, new Vector4(0f, 0f, 1f, position.z));
            matrix.SetRow(3, new Vector4(0f, 0f, 0f, 1f));
            return matrix;
        }
    }

    private Matrix4x4 RotationMatrix
    {
        get
        {
            float radX = rotation.x * Mathf.Deg2Rad;
            float radY = rotation.y * Mathf.Deg2Rad;
            float radZ = rotation.z * Mathf.Deg2Rad;
            float sinX = Mathf.Sin(radX);
            float cosX = Mathf.Cos(radX);
            float sinY = Mathf.Sin(radY);
            float cosY = Mathf.Cos(radY);
            float sinZ = Mathf.Sin(radZ);
            float cosZ = Mathf.Cos(radZ);

            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetColumn(0, new Vector4(
                cosY * cosZ,
                cosX * sinZ + sinX * sinY * cosZ,
                sinX * sinZ - cosX * sinY * cosZ,
                0f
            ));
            matrix.SetColumn(1, new Vector4(
                -cosY * sinZ,
                cosX * cosZ - sinX * sinY * sinZ,
                sinX * cosZ + cosX * sinY * sinZ,
                0f
            ));
            matrix.SetColumn(2, new Vector4(
                sinY,
                -sinX * cosY,
                cosX * cosY,
                0f
            ));
            matrix.SetColumn(3, new Vector4(0f, 0f, 0f, 1f));
            return matrix;
        }
    }

    private Matrix4x4 ProjectMatrix
    {
        get
        {
            Matrix4x4 matrix = new Matrix4x4();
            matrix.SetRow(0, new Vector4(focalLength, 0f, 0f, 0f));
            matrix.SetRow(1, new Vector4(0f, focalLength, 0f, 0f));
            matrix.SetRow(2, new Vector4(0f, 0f, 0f, 0f));
            matrix.SetRow(3, new Vector4(0f, 0f, 1f, 0f));
            return matrix;
        }
    }

    private Vector3[] VertexBuffer = new Vector3[] 
    {
        new Vector3(1, -1, 1),
        new Vector3(-1, -1, 1),
        new Vector3(-1, 1, 1),
        new Vector3(1, 1, 1),
        new Vector3(1, -1, -1),
        new Vector3(-1, -1, -1),
        new Vector3(-1, 1, -1),
        new Vector3(1, 1, -1),
    };

    private void Awake()
    {
        frontBuffer = new Texture2D(512, 512, TextureFormat.RGB24, false);
        frontBuffer.wrapMode = TextureWrapMode.Clamp;

        mMaterial = GetComponent<Renderer>().sharedMaterial;
        mMaterial.SetTexture("_MainTex", frontBuffer);
    }

    private void Start()
    {
#if !DrawCustomCube
        VertexBuffer = MeshData.vertices;
#endif

        InvokeRepeating("GraphicTest", 5f, 5f);
    }

    private void OnDestroy()
    {
        DestroyImmediate(frontBuffer);
        frontBuffer = null;
    }

    private void GraphicTest()
    {
        UpdateData = !UpdateData;
    }

    private void LateUpdate()
    {
        if (UpdateData)
        {
            rotation.Set(rotation.x, rotation.y + Time.deltaTime * 30f, rotation.z);
            DrawMesh();
        }
    }

    private void DrawMesh()
    {
        //ClearBuffer很慢
        frontBuffer.ClearBuffer(ClearColor);

        if (MeshData)
        {
            int vertecCount = VertexBuffer.Length;
            Vector3[] tempVertexBuffer = new Vector3[vertecCount];
            for (int i = 0; i < vertecCount; i++)
            {
                //旋转矩阵操作和投影变换矩阵操作很慢
                Matrix4x4 mvp = PostionMatrix * ScaleMatrix * RotationMatrix; //* ProjectMatrix
                Vector2 projectPostion = mvp.MultiplyPoint(VertexBuffer[i]);
                projectPostion.Set((int)projectPostion.x + 256, (int)projectPostion.y + 256);
                tempVertexBuffer[i] = projectPostion;
            }

#if !DrawCustomCube
            for (int i = 0; i < vertecCount - 2; i++)
            {
                frontBuffer.DrawLine(tempVertexBuffer[i], tempVertexBuffer[i + 1], LineColor);
                frontBuffer.DrawLine(tempVertexBuffer[i], tempVertexBuffer[i + 2], LineColor);
            }
#else
            frontBuffer.DrawLine(tempVertexBuffer[0], tempVertexBuffer[1], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[1], tempVertexBuffer[2], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[2], tempVertexBuffer[3], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[3], tempVertexBuffer[0], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[0], tempVertexBuffer[4], Color.red);

            frontBuffer.DrawLine(tempVertexBuffer[4], tempVertexBuffer[5], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[5], tempVertexBuffer[6], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[6], tempVertexBuffer[7], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[4], tempVertexBuffer[7], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[3], tempVertexBuffer[7], Color.red);

            frontBuffer.DrawLine(tempVertexBuffer[5], tempVertexBuffer[1], Color.red);
            frontBuffer.DrawLine(tempVertexBuffer[6], tempVertexBuffer[2], Color.red);
#endif
        }

        frontBuffer.Apply();
    }
}
