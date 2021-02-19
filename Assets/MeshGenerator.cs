using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
{
    //TODO: bad practice, should've used config file
    private static string API_URL = "http://open.mapquestapi.com/elevation/v1/profile?key=KJcGA8F1Ozf1FHxCEYPSxdiAWrMMuBcu&shapeFormat=raw&latLngCollection=";

    Mesh mesh;

    public int xSize = 100;
    public int zSize = 100;

    // height scale of model
    public int maxHeightScale = 50;

    private Vector3[] vertices;
    private int[] trigs;
    private List<ElevationProfile> elevations = new List<ElevationProfile>();

    private Color[] colors;
    public Gradient gradient;

    // Grays Peak, CO
    public float centerX = 39.64002f, centerY = -105.9281f;

    // distance between adjacent points
    public float baseDistance = 60;

    // debug
    public int progress = 0;

    // Start is called before the first frame update
    void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        StartCoroutine(GetElevations());

        //StartCoroutine(CreateShape());
        //CreateShape();
    }

    void Update()
    {
        UpdateMesh();
    }

    private IEnumerator GetElevations()
    {
        // generate the coordinates
        float spacingX = baseDistance * 1 / 111111;
        float spacingY = baseDistance * 1 / (111111 * Mathf.Cos(centerY * Mathf.PI / 180));
        float startX = centerX - spacingX * xSize / 2;
        float startY = centerY - spacingY * zSize / 2;
        Coordinate[] coordinates = new Coordinate[xSize + 1];
        for (int z = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                coordinates[x] = new Coordinate(startX + spacingX * x, startY + spacingY * z);
            }

            // construct the URL
            string latlngs = string.Join(",", coordinates.Select(c => c.latitude + "," + c.longitude).ToArray());
            string url = API_URL + latlngs;

            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError)
                {
                    Debug.Log("Error: " + webRequest.error);
                }
                else
                {
                    string body = webRequest.downloadHandler.text;
                    progress = z;       // debug

                    // save results from API 
                    ElevationDTO elevationDTO = JsonUtility.FromJson<ElevationDTO>(body);
                    elevations.AddRange(elevationDTO.elevationProfile);
                }
            }
        }

        CreateShape();
    }

    void CreateShape()
    {
        initVertices();
        initTriangles();
        setUpColors();
    }

    private void initVertices()
    {
        // handle coordinates with no height value
        // taking their previous coordinate as a quick fix
        for (int i = 1; i < elevations.Count; i++)
        {
            if (elevations[i].height == -32768)
            {
                elevations[i].height = elevations[i - 1].height;
            }
        }


        // find the min and max height for lerping
        float minHeight = elevations[0].height;
        float maxHeight = minHeight;
        foreach (ElevationProfile elevation in elevations)
        {
            minHeight = Mathf.Min(minHeight, elevation.height);
            maxHeight = Mathf.Max(maxHeight, elevation.height);
        }
        Debug.Log(minHeight + " " + maxHeight);


        // normalize height values
        vertices = new Vector3[(xSize + 1) * (zSize + 1)];
        for (int z = 0, i = 0; z <= zSize; z++)
        {
            for (int x = 0; x <= xSize; x++)
            {
                float y = Mathf.InverseLerp(minHeight, maxHeight, elevations[i].height) * maxHeightScale;
                vertices[i] = new Vector3(x, y, z);
                i++;
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
        colors = new Color[vertices.Length];
        for (int i = 0; i < vertices.Length; i++) {
            colors[i] = gradient.Evaluate(vertices[i].y / maxHeightScale);
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
