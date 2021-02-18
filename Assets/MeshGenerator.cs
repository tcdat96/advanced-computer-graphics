using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    public int xSize = 20;
    public int zSize = 20;

    private Vector3[] vertices;
    private int[] trigs;

    private Color[] colors;
    public Gradient gradient;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        //StartCoroutine(CreateShape());
        CreateShape();
    }

    private void Update()
    {
        UpdateMesh();
    }

    void CreateShape()
    {
        initVertices();
        initTriangles();
        setUpColors();
    }

    private void initVertices()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 5f;
                vertices[z * (zSize + 1) + x] = new Vector3(x, y, z);
            }
        }
    }

    private void initTriangles()
    {
        trigs = new int[xSize * zSize * 6];
        int vert = 0;
        int triIndex = 0;
        for (int z = 0; z < zSize; z++)
        {
            for (int x = 0; x < xSize; x++)
            {
                trigs[triIndex++] = vert + 0;
                trigs[triIndex++] = vert + xSize + 1;
                trigs[triIndex++] = vert + 1;
                trigs[triIndex++] = vert + 1;
                trigs[triIndex++] = vert + xSize + 1;
                trigs[triIndex++] = vert + xSize + 2;

                vert++;
            }
            vert++;
        }
    }

    private void setUpColors()
    {
        float minHeight = vertices[0].y;
        float maxHeight = minHeight;
        foreach (Vector3 vertex in vertices)
        {
            minHeight = Mathf.Min(minHeight, vertex.y);
            maxHeight = Mathf.Max(maxHeight, vertex.y);
        }

        colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            float height = Mathf.InverseLerp(minHeight, maxHeight, vertices[i].y);
            colors[i] = gradient.Evaluate(height);
            print(height);
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = trigs;
        mesh.colors = colors;

        mesh.RecalculateNormals();
    }

    private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.DrawSphere(vertices[i], .1f);
        }
    }
}
