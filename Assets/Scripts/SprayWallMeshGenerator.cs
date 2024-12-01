using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class SprayWallMeshGenerator : MonoBehaviour
{
    [SerializeField] Material mat;
    private Mesh mesh;

    private GameObject meshObject;

    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mesh = new Mesh();
        meshObject = new GameObject("Mesh Object", typeof(MeshRenderer), typeof(MeshFilter));

        meshObject.GetComponent<MeshFilter>().mesh = mesh;
        meshObject.GetComponent<MeshRenderer>().material = mat;

        CreateShape();
        UpdateMesh();
    }

    void CreateShape(){
        vertices = new Vector3[]{
            new Vector3 (0,0,0),
            new Vector3 (0,0,1),
            new Vector3 (1,0,0),
            new Vector3 (1,0,1)
        };

        triangles = new int[]{
            0,1,2,
            1,3,2
        };

        uv = new Vector2[]{
            new Vector2 (0,0),
            new Vector2 (0,1),
            new Vector2 (1,1),
            new Vector2 (1,0)
        };
    }

    void UpdateMesh(){
        mesh.Clear();

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
    }
}
