/**
The MIT License (MIT)
Copyright (c) 2017/1/7 tachen
 * 
 */

using UnityEngine;
using System.Collections;

using ProtoTurtle.BitmapDrawing;

namespace BTGRender
{
    //[ExecuteInEditMode]
    public class BTGDrawMesh : MonoBehaviour
    {
        /// <summary>
        /// 渲染模式
        /// </summary>
        public enum DrawMode
        {
            //线框渲染
            LineType,

            //三角形渲染
            TriangleType,
        }

        /// <summary>
        /// 基类定义
        /// </summary>
        [System.Serializable]
        public class BTGGameObject
        {
            public Vector3 rotation;

            public Vector3 position;

            public Vector3 scale;

            public Mesh meshData;

            public Color meshColor;

            public Matrix4x4 ScaleMatrix
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

            public Matrix4x4 PostionMatrix
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

            public Matrix4x4 RotationMatrix
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
        }

        public Color ClearColor;

        public float focalLength = 1f;

        private Texture2D frontBuffer;

        public bool UpdateData = true;

        public DrawMode mDrawMode = DrawMode.TriangleType;

        public BTGGameObject[] allRenderData = new BTGGameObject[1];

        private Material mMaterial;

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

        public struct VERTEXBUFFER
        {
            public Vector2[] vertex;
        }

        public struct INDEXBUFFER
        {
            public int[] indexBuffer;
        }

        private void Awake()
        {
            frontBuffer = new Texture2D(512, 512, TextureFormat.RGB24, false);
            frontBuffer.wrapMode = TextureWrapMode.Clamp;

            mMaterial = GetComponent<Renderer>().sharedMaterial;
            mMaterial.SetTexture("_MainTex", frontBuffer);
        }

        private void Start()
        {

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
                //rotation.Set(rotation.x, rotation.y + Time.deltaTime * 30f, rotation.z);
                DrawMesh();
            }
        }

        private void DrawMesh()
        {
            //ClearBuffer很慢
            frontBuffer.ClearBuffer(ClearColor);

            if (allRenderData.Length > 0)
            {
                int bufferCount = allRenderData.Length;
                VERTEXBUFFER[] tempVertexBuffer = new VERTEXBUFFER[bufferCount];
                for (int i = 0; i < bufferCount; i++)
                {
                    int vertexCount = allRenderData[i].meshData.vertexCount;
                    Vector3[] vertexBuffer = allRenderData[i].meshData.vertices;
                    int[] indexBuffer = allRenderData[i].meshData.GetIndices(0);
                    tempVertexBuffer[i].vertex = new Vector2[vertexCount];

                    for (int j = 0; j < vertexCount; j++)
                    {
                        //旋转矩阵操作和投影变换矩阵操作很慢
                        Matrix4x4 mvp = allRenderData[i].PostionMatrix *
                                        allRenderData[i].ScaleMatrix *
                                        allRenderData[i].RotationMatrix; //* ProjectMatrix
                        Vector2 projectPostion = mvp.MultiplyPoint(vertexBuffer[j]);
                        projectPostion.Set((int)projectPostion.x + 256, (int)projectPostion.y + 256);
                        tempVertexBuffer[i].vertex[j] = projectPostion;
                    }

                    if (mDrawMode == DrawMode.LineType)
                    {
                        for (int index = 0; index < vertexCount - 2; index++)
                        {
                            frontBuffer.DrawLine(tempVertexBuffer[i].vertex[index], tempVertexBuffer[i].vertex[index + 1], allRenderData[i].meshColor);
                            frontBuffer.DrawLine(tempVertexBuffer[i].vertex[index], tempVertexBuffer[i].vertex[index + 2], allRenderData[i].meshColor);
                        }
                        frontBuffer.DrawLine(tempVertexBuffer[i].vertex[0], tempVertexBuffer[i].vertex[vertexCount - 1], allRenderData[i].meshColor);
                    }
                    else if (mDrawMode == DrawMode.TriangleType)
                    {
                        if (indexBuffer.Length % 3 != 0)
                        {
                            Debug.LogWarning("Index Buffer's length has to be multiple of 3");
                        }
                        else
                        {
                            int triangleCount = indexBuffer.Length / 3;
                            for (int index = 0; index < triangleCount; index++)
                            {
                                Vector3[] sortVectorY = new Vector3[3]
                                {
                                    tempVertexBuffer[i].vertex[indexBuffer[index*3]],
                                    tempVertexBuffer[i].vertex[indexBuffer[ index*3 + 1]],
                                    tempVertexBuffer[i].vertex[indexBuffer[index*3 + 2]],
                                };

                                for (int j = 0; j < 3; j++)
                                {
                                    for (int k = j; k < 3; k++)
                                    {
                                        if (sortVectorY[j].y > sortVectorY[k].y)
                                        {
                                            Vector3 temp = sortVectorY[j];
                                            sortVectorY[j] = sortVectorY[k];
                                            sortVectorY[k] = temp;
                                        }
                                    }
                                }

                                frontBuffer.DrawTriangle(sortVectorY[0], sortVectorY[1], sortVectorY[2], allRenderData[i].meshColor);
                            }
                        }
                    }
                }
            }

            frontBuffer.Apply();
        }
    }

}
