using UnityEngine;
using System.Collections.Generic;
using TriangleNet.Geometry;

public class DelaunayTriangulation : MonoBehaviour
{
    public List<Vector2> points;
    public List<Triangle> triangles = new List<Triangle>();

    Polygon polygon = new Polygon();
    void Start()
    {
        points = GenerateRandomPoints(10); // Generar 10 puntos aleatorios, puedes cambiar el número de puntos
        //triangles = Triangulate(points);
        
    }
    private void Update()
    {
        for (int i = 0; i < 2; i++)
        {

            triangles = Triangulate(points);
            DrawTriangles();
        }
    }
    List<Vector2> GenerateRandomPoints(int count)
    {
        List<Vector2> points = new List<Vector2>();
        for (int i = 0; i < count; i++)
        {
            points.Add(new Vector2(Random.Range(-10f, 10f), Random.Range(-10f, 10f)));
        }
        return points;
    }

    List<Triangle> Triangulate(List<Vector2> points)
    {
        triangles.Clear();
        polygon.Points.Clear();
        foreach (var point in points)
        {
            polygon.Add(new Vertex(point.x, point.y));
        }

        var mesh = polygon.Triangulate();

        foreach (var tri in mesh.Triangles)
        {
            Vertex v1 = tri.GetVertex(0);
            Vertex v2 = tri.GetVertex(1);
            Vertex v3 = tri.GetVertex(2);
            triangles.Add(new Triangle(new Vector2((float)v1.X, (float)v1.Y),
                                       new Vector2((float)v2.X, (float)v2.Y),
                                       new Vector2((float)v3.X, (float)v3.Y)));
        }
        return triangles;
    }

    void DrawTriangles()
    {
        foreach (var triangle in triangles)
        {
            Debug.DrawLine(triangle.v1, triangle.v2, Color.red, 100f);
            Debug.DrawLine(triangle.v2, triangle.v3, Color.red, 100f);
            Debug.DrawLine(triangle.v3, triangle.v1, Color.red, 100f);
        }
    }
}

public class Triangle
{
    public Vector2 v1, v2, v3;

    public Triangle(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        this.v1 = v1;
        this.v2 = v2;
        this.v3 = v3;
    }
}
