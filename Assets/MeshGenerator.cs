using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    Mesh mesh;

    public int xSize = 20;
    public int zSize = 20;

    Vector3[] vertices;
    int[] trigs;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        StartCoroutine(CreateShape());
    }

    private void Update()
    {
        UpdateMesh();
    }

    IEnumerator CreateShape()
    {
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.PerlinNoise(x * .3f, z * .3f) * 2f;
                vertices[z * (zSize + 1) + x] = new Vector3(x, y, z);
            }
        }

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

                yield return new WaitForSeconds(.01f);
            }
            vert++;
        }
    }

    void UpdateMesh()
    {
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = trigs;

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
