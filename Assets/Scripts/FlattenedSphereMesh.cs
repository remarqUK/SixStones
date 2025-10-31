using UnityEngine;

/// <summary>
/// Generates a flattened sphere (oblate spheroid) mesh
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class FlattenedSphereMesh : MonoBehaviour
{
    [Header("Sphere Dimensions")]
    [Tooltip("Radius on X and Z axes (equatorial radius)")]
    [SerializeField] private float equatorialRadius = 1.0f;

    [Tooltip("Radius on Y axis (polar radius) - smaller value = more flattened")]
    [SerializeField] private float polarRadius = 0.5f;

    [Header("Mesh Resolution")]
    [Tooltip("Number of horizontal segments (longitude lines)")]
    [SerializeField] private int segments = 32;

    [Tooltip("Number of vertical segments (latitude lines)")]
    [SerializeField] private int rings = 16;

    [Header("Options")]
    [SerializeField] private bool generateOnStart = true;
    [SerializeField] private bool recalculateNormalsSmooth = true;

    private MeshFilter meshFilter;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        if (generateOnStart)
        {
            GenerateMesh();
        }
    }

    /// <summary>
    /// Generate the flattened sphere mesh
    /// </summary>
    public void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "Flattened Sphere";

        // Calculate vertex count
        int vertexCount = (rings + 1) * (segments + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        // Generate vertices
        int vertexIndex = 0;
        for (int ring = 0; ring <= rings; ring++)
        {
            float v = (float)ring / rings;
            float phi = v * Mathf.PI; // 0 to PI (top to bottom)

            for (int segment = 0; segment <= segments; segment++)
            {
                float u = (float)segment / segments;
                float theta = u * Mathf.PI * 2f; // 0 to 2PI (around)

                // Calculate position on oblate spheroid
                float x = equatorialRadius * Mathf.Sin(phi) * Mathf.Cos(theta);
                float y = polarRadius * Mathf.Cos(phi);
                float z = equatorialRadius * Mathf.Sin(phi) * Mathf.Sin(theta);

                vertices[vertexIndex] = new Vector3(x, y, z);
                uvs[vertexIndex] = new Vector2(u, v);

                vertexIndex++;
            }
        }

        // Generate triangles
        int triangleCount = rings * segments * 6;
        int[] triangles = new int[triangleCount];

        int triangleIndex = 0;
        for (int ring = 0; ring < rings; ring++)
        {
            for (int segment = 0; segment < segments; segment++)
            {
                int current = ring * (segments + 1) + segment;
                int next = current + segments + 1;

                // First triangle
                triangles[triangleIndex++] = current;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = current + 1;

                // Second triangle
                triangles[triangleIndex++] = current + 1;
                triangles[triangleIndex++] = next;
                triangles[triangleIndex++] = next + 1;
            }
        }

        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

        if (recalculateNormalsSmooth)
        {
            mesh.RecalculateNormals();
        }
        else
        {
            // Calculate normals manually for the oblate spheroid
            Vector3[] normals = new Vector3[vertexCount];
            for (int i = 0; i < vertexCount; i++)
            {
                Vector3 pos = vertices[i];
                // For oblate spheroid, normals need to account for different radii
                float nx = pos.x / (equatorialRadius * equatorialRadius);
                float ny = pos.y / (polarRadius * polarRadius);
                float nz = pos.z / (equatorialRadius * equatorialRadius);
                normals[i] = new Vector3(nx, ny, nz).normalized;
            }
            mesh.normals = normals;
        }

        mesh.RecalculateBounds();

        meshFilter.mesh = mesh;
    }

    private void OnValidate()
    {
        // Regenerate mesh when values change in inspector
        if (Application.isPlaying && meshFilter != null)
        {
            GenerateMesh();
        }
    }
}
