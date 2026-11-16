using System.Collections.Generic;
using UnityEngine;

namespace KingLucilia.Combat
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class ProceduralWeaponTrail : MonoBehaviour
    {
        [Header("Anchor Points")]
        [SerializeField] private Transform tipPoint;
        [SerializeField] private Transform basePoint;

        [Header("Trail Settings")]
        [SerializeField] private float lifetime = 0.16f;
        [SerializeField] private float minVertexDistance = 0.04f;
        [SerializeField] private Gradient colorGradient;

        private Mesh mesh;
        private MeshFilter meshFilter;
        private readonly List<TrailPoint> points = new List<TrailPoint>();
        private bool isEmitting = false;

        private struct TrailPoint
        {
            public Vector3 tipPosition;
            public Vector3 basePosition;
            public float timeAlive;
        }

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            mesh = new Mesh { name = "ProceduralTrailMesh" };
            meshFilter.mesh = mesh;

            if (colorGradient == null || colorGradient.colorKeys.Length == 0)
            {
                var g = new Gradient();
                g.SetKeys(
                    new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                    new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
                );
                colorGradient = g;
            }
        }

        public void SetEmit(bool emit)
        {
            isEmitting = emit;
            if (!emit && points.Count > 0)
            {
                points.Clear();
                mesh.Clear();
            }
        }

        private void LateUpdate()
        {
            if (tipPoint == null || basePoint == null) return;

            // Envejecer puntos existentes
            for (int i = points.Count - 1; i >= 0; i--)
            {
                TrailPoint p = points[i];
                p.timeAlive += Time.deltaTime;
                if (p.timeAlive >= lifetime)
                {
                    points.RemoveAt(i);
                }
                else
                {
                    points[i] = p;
                }
            }

            // Registrar nuevo punto si se está emitiendo
            if (isEmitting)
            {
                bool addPoint = false;
                if (points.Count == 0)
                {
                    addPoint = true;
                }
                else
                {
                    float dist = Vector3.Distance(points[0].tipPosition, tipPoint.position);
                    if (dist >= minVertexDistance)
                    {
                        addPoint = true;
                    }
                }

                if (addPoint)
                {
                    points.Insert(0, new TrailPoint
                    {
                        tipPosition = tipPoint.position,
                        basePosition = basePoint.position,
                        timeAlive = 0f
                    });
                }
            }

            GenerateMesh();
        }

        private void GenerateMesh()
        {
            if (points.Count < 2)
            {
                mesh.Clear();
                return;
            }

            int pointCount = points.Count;
            int vertexCount = pointCount * 2;

            Vector3[] vertices = new Vector3[vertexCount];
            Vector2[] uvs = new Vector2[vertexCount];
            Color[] colors = new Color[vertexCount];
            int[] triangles = new int[(pointCount - 1) * 6];

            for (int i = 0; i < pointCount; i++)
            {
                float normalizedLife = Mathf.Clamp01(points[i].timeAlive / lifetime);
                Color c = colorGradient.Evaluate(normalizedLife);

                // Convertir posiciones mundiales a espacio local del mesh
                vertices[i * 2] = transform.InverseTransformPoint(points[i].basePosition);
                vertices[i * 2 + 1] = transform.InverseTransformPoint(points[i].tipPosition);

                float u = 1f - (float)i / (pointCount - 1);
                uvs[i * 2] = new Vector2(u, 0f);
                uvs[i * 2 + 1] = new Vector2(u, 1f);

                colors[i * 2] = c;
                colors[i * 2 + 1] = c;

                if (i < pointCount - 1)
                {
                    int t = i * 6;
                    int v = i * 2;

                    triangles[t] = v;
                    triangles[t + 1] = v + 1;
                    triangles[t + 2] = v + 2;

                    triangles[t + 3] = v + 2;
                    triangles[t + 4] = v + 1;
                    triangles[t + 5] = v + 3;
                }
            }

            mesh.Clear();
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
        }
    }
}
