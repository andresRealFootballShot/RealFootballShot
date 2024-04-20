using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FieldTriangleSpace
{
    public class Triangle
    {
        public List<Edge> edges;
        public Point point1 { get => points[0]; }
        public Point point2 { get => points[1]; }
        public Point point3 { get => points[2]; }
        public List<Point> points { get; set; }
        private void LoadPoints()
        {
            points= new List<Point>();
            foreach (var edge in edges)
            {
                foreach (var point in edge.points)
                {
                    if (!points.Contains(point))
                    {
                        points.Add(point);
                    }
                }
            }
        }
        public Triangle(List<Edge> edges)
        {
            this.edges = edges;
            LoadPoints();
        }
        public float getArea()
        {
            Vector3 dir1 = point2.position - point1.position;
            Vector3 dir2 = point3.position - point1.position;
            float angle = Vector3.Angle(dir1, dir2);
            float area = (dir1.magnitude * dir2.magnitude * Mathf.Sin(angle * Mathf.Deg2Rad))/2;
            return area;
        }
        public float getPerimeter()
        {
            Vector3 dir1 = point2.position - point1.position;
            Vector3 dir2 = point3.position - point1.position;
            Vector3 dir3 = point2.position - point3.position;
            float perimeter = dir1.magnitude + dir2.magnitude + dir3.magnitude;
            return perimeter;
        }
        public List<Edge> getConectedEdges(Point point)
        {
            List<Edge> conectedEdges = new List<Edge>();
            foreach (var edge in edges)
            {
                if (edge.points.Contains(point))
                {
                    conectedEdges.Add(edge);
                }
            }
            return conectedEdges;
        }
        public List<Point> getOtherPoints(Point point)
        {
            List<Point> otherPoints = new List<Point>();
            foreach (var _point in points)
            {
                if (!_point.Equals(point))
                {
                    otherPoints.Add(_point);
                }
            }
            return otherPoints;
        }
        public float getAngle(Point point)
        {
            List<Point> otherPoints = new List<Point>();
            foreach (var _point in points)
            {
                if (!_point.Equals(point))
                {
                    otherPoints.Add(_point);
                }
            }
            Vector3 dir1 = otherPoints[0].position - point.position;
            Vector3 dir2 = otherPoints[1].position - point.position;
            return Vector3.Angle(dir1, dir2);
        }
        public Vector3 getIncentro()
        {
            float denominator = 0;
            Vector3 result = Vector3.zero;
            foreach (var point in points)
            {
                Edge edge = edges.Find(x => !x.points.Contains(point));
                float d = edge.getDistance();
                result += point.position * d;
                denominator += d;
            }
            result /= denominator;
            return result;
        }
        public Vector3 getBarycenter()
        {
            Vector3 result = Vector3.zero;
            foreach (var point in points)
            {
                result += point.position;
            }
            return result / 3;
        }
        public override string ToString()
        {
            return "Triangle " + points[0].pointName + "-" + points[1].pointName + "-" + points[2].pointName;
        }
    }
    public class Point
    {
        public Vector3 position;
        public string pointName;
        public Point(Vector3 position,string pointName) : this(position)
        {
            this.pointName = pointName;
        }
        public Point(Vector3 position) 
        {
            this.position = position;
        }
        public Point(string pointName)
        {
            
        }
        public int Sort(Point point1, Point point2)
        {
            float d1 = Vector3.Distance(position, point1.position);
            float d2 = Vector3.Distance(position, point2.position);

            return d1.CompareTo(d2);
        }
        public override string ToString()
        {
            return pointName;
        }
    }
    public class Edge
    {
        public List<Point> points = new List<Point>();
        public int index;
        public Edge(Point point1, Point point2)
        {
            points.Add(point1);
            points.Add(point2);
        }
        public Point getFirstOtherPoint(Point point)
        {
            foreach (var _point in points)
            {
                if (!_point.Equals(point))
                {
                    return _point;
                }
            }
            return null;
        }
        public float getDistance()
        {
            float d = 0;
            Point lastPoint = null;
            foreach (var point in points)
            {
                if (lastPoint != null)
                {
                    d += Vector3.Distance(lastPoint.position, point.position);
                }
                lastPoint = point;
            }
            return d;
        }
        public Vector3 getMiddlePoint(){
            float d = getDistance();
            d = d / 2;
            Point point1 = points[0];
            Point point2 = points[1];
            Vector3 dir = point1.position - point2.position;
            dir.Normalize();
            return point2.position + dir * d;
        }
        public override string ToString()
        {
            return "Edge "+points[0].pointName+"-"+ points[1].pointName;
        }
    }
    public class Limit
    {
        public class Line
        {
            public Point otherPoint;
            public Point basePoint;
            public Edge edge;
            public Vector3 dir;
            bool _right;
            public bool right { get => _right; set { _right = value; ySign = _right ? 1 : -1; } }
            public float ySign;
            public Line(Point basePoint,Edge edge)
            {
                otherPoint = edge.getFirstOtherPoint(basePoint);
                this.basePoint = basePoint;
                this.edge = edge;
                dir = otherPoint.position - basePoint.position;

            }
            public Line(Point basePoint, Edge edge,bool right) : this(basePoint,edge)
            {
                this.right = right;
                ySign = right ? 1 : -1;
            }
            public void setPoint(Edge edge)
            {
                otherPoint = edge.getFirstOtherPoint(basePoint);
                this.edge = edge;
                dir = otherPoint.position - basePoint.position;
            }
            public override string ToString()
            {
                return "Line " + edge.points[0].pointName + "-" + edge.points[1].pointName;
            }
        }
        public Point basePoint;
        public List<Line> lines = new List<Line>();
        Line line1 { get => lines[0]; }
        Line line2 { get => lines[1]; }
        bool isObtuse { get => angle > 180; }
        public float angle;
        public Limit(Point basePoint,Triangle triangle,float triangleAngle)
        {
            this.basePoint = basePoint;
            List<Edge> edges = triangle.getConectedEdges(basePoint);
            foreach (var edge in edges)
            {
                Line newLine = new Line(basePoint, edge);
                lines.Add(newLine);
            }
            float ySign = Mathf.Sign(Vector3.Cross(line1.dir, line2.dir).y);
            bool right = ySign == 1;
            line1.right = right;
            line2.right = !right;
            angle = triangleAngle;
        }
        public Limit(Point basePoint, List<Line> lines)
        {
            this.basePoint = basePoint;
            this.lines = lines;
            //añadir nuevo angle
            bool right = Vector3.Cross(line1.dir, line2.dir).y>=0;
            float a = Vector3.Angle(line1.dir, line2.dir);
            angle = line1.right == right ? a : 360 - a;
        }
        public class NotifyNewTriangleResult
        {
            public bool isConected;
            public Limit conectedLimit;
            public bool isCompleted;
            public class ClosestEdgeData
            {
                public Point joinedPoint;
                public Edge closestEdge;

                public ClosestEdgeData(Point joinedPoint, Edge closestEdge)
                {
                    this.joinedPoint = joinedPoint;
                    this.closestEdge = closestEdge;
                }
            }
            public List<ClosestEdgeData> closestEdgeData = new List<ClosestEdgeData>();
        }
        public NotifyNewTriangleResult notifyNewTriangle(Triangle triangle,float triangleAngle)
        {
            List<Edge> edges = triangle.getConectedEdges(basePoint);
            NotifyNewTriangleResult notifyNewTriangleResult = new NotifyNewTriangleResult();
            Line lineRemoved=null;
            Edge notRemovedEdge = null;
            foreach (var edge in edges)
            {
                Line line = lines.Find(x => x.edge.Equals(edge));
                if (line != null)
                {
                    NotifyNewTriangleResult.ClosestEdgeData closestEdgeData = new NotifyNewTriangleResult.ClosestEdgeData(edge.getFirstOtherPoint(basePoint), line.edge);
                    notifyNewTriangleResult.closestEdgeData.Add(closestEdgeData);
                    lineRemoved = line;
                    lines.Remove(line);
                    if (notifyNewTriangleResult.isConected)
                    {
                        notifyNewTriangleResult.isCompleted = true;
                    }
                    notifyNewTriangleResult.isConected = true;
                    notifyNewTriangleResult.conectedLimit = this;
                }
                else
                {
                    notRemovedEdge = edge;
                }
            }
            if (notifyNewTriangleResult.isConected && !notifyNewTriangleResult.isCompleted)
            {
                Line newLine = new Line(basePoint, notRemovedEdge, lineRemoved.right);
                lines.Add(newLine);
            }
            if (notifyNewTriangleResult.isConected)
            {
                //angle += triangle.getAngle(basePoint);
                angle += triangleAngle;
            }
            return notifyNewTriangleResult;
        }
        public bool pointIsInside(Point point)
        {
            Vector3 dir3 = point.position - basePoint.position;
            Vector3 cross1 = Vector3.Cross(line1.dir, dir3);
            Vector3 cross2 = Vector3.Cross(line2.dir, dir3);
            bool isInside;
            if (isObtuse)
            {
                isInside = Mathf.Sign(cross1.y).Equals(line1.ySign)&& cross1.y !=0 || Mathf.Sign(cross2.y).Equals(line2.ySign) && cross2.y != 0;
            }
            else
            {
                isInside = Mathf.Sign(cross1.y).Equals(line1.ySign) && cross1.y!=0 && Mathf.Sign(cross2.y).Equals(line2.ySign) && cross2.y != 0;
            }
            return isInside;
        }
    }
    public class LimitList
    {
        List<Limit> limits = new List<Limit>();
        public Point basePoint;

        public LimitList(Point basePoint)
        {
            this.basePoint = basePoint;
        }
        public void AddLimit(Limit limit)
        {
            limits.Add(limit);
        }
        public bool isInside(Point point)
        {
            bool isInside = false;
            foreach (var limit in limits)
            {
                if (isInside = limit.pointIsInside(point))
                {
                    break;
                }
            }
            return isInside;
        }
        public Limit.NotifyNewTriangleResult notifyNewTriangle(Triangle triangle)
        {
            bool createNewLimit = true;
            Limit.NotifyNewTriangleResult notifyNewTriangleResult= new Limit.NotifyNewTriangleResult();
            List<Limit> removeLimits = new List<Limit>();
            float triangleAngle = triangle.getAngle(basePoint);
            foreach (var limit in limits)
            {
                Limit.NotifyNewTriangleResult notifyNewTriangleResult2 = limit.notifyNewTriangle(triangle, triangleAngle);
                notifyNewTriangleResult.closestEdgeData.AddRange(notifyNewTriangleResult2.closestEdgeData);
                if (notifyNewTriangleResult2.isCompleted)
                {
                    createNewLimit = false;
                    notifyNewTriangleResult.isCompleted = notifyNewTriangleResult2.isCompleted;
                    removeLimits.Add(notifyNewTriangleResult2.conectedLimit);
                    break;
                }
                if (notifyNewTriangleResult2.isConected)
                {
                    createNewLimit = false;
                    if (notifyNewTriangleResult.isConected)
                    {
                        Limit.Line line1 = notifyNewTriangleResult2.conectedLimit.lines[0];
                        Limit.Line line2 = notifyNewTriangleResult.conectedLimit.lines[0];
                        removeLimits.Add(notifyNewTriangleResult2.conectedLimit);
                        removeLimits.Add(notifyNewTriangleResult.conectedLimit);
                        List<Limit.Line> lines = new List<Limit.Line>();
                        lines.Add(line1);
                        lines.Add(line2);
                        Limit newLimit = new Limit(basePoint, lines);
                        limits.Add(newLimit);
                        break;
                    }
                    else
                    {
                        notifyNewTriangleResult.isConected = notifyNewTriangleResult2.isConected;
                        notifyNewTriangleResult.conectedLimit = notifyNewTriangleResult2.conectedLimit;
                        
                    }
                }
            }
            limits.RemoveAll(x=>removeLimits.Contains(x));
            if (createNewLimit)
            {
                
                Limit limit = new Limit(basePoint,triangle,triangleAngle);
                limits.Add(limit);
            }
            return notifyNewTriangleResult;
        }
    }
}
