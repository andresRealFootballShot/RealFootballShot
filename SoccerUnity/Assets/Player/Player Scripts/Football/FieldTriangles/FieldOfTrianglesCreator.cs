using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FieldTriangleSpace
{
    public class FieldOfTrianglesCreator : MonoBehaviour
    {
        public struct LonelyPoint
        {
            public string name;
            public Vector3 position;
            public LonelyPoint(Vector3 position, string name)
            {
                this.position = position;
                this.name = name;
            }
        }
        List<Point3> allPoints = new List<Point3>();
        public float divideEdgeLonelyPointDistance=5;
        public int divideEdgeMaxPoints=3;
        public bool debug;
        public bool debugText;
        List<Triangle> triangles = new List<Triangle>();
        List<Edge> edges = new List<Edge>();
        List<LonelyPoint> lonelyPoints = new List<LonelyPoint>();
        int triangleSize,edgeSize,lonelyPointSize;
        public float fieldOffset = 2;
        public string teamName;
        public Color color;
        private void Start()
        {

            CreatePoints(15);
            MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldLoaded);
        }
        private void Update()
        {
            UpdatePointPositions();
            calculateLonelyPoints();
        }
        private void UpdatePointPositions()
        {
            Team team = Teams.getTeamByName(teamName);
            for (int i = 0; i < team.publicPlayerDatas.Count; i++)
            {
                allPoints[i].position = team.publicPlayerDatas[i].position;
            }
        }
        private void footballFieldLoaded()
        {
                int i = 11;
                foreach (var corner in MatchComponents.footballField.cornersComponents)
                {
                    Transform cornerTransform = corner.cornerPoint;
                    Vector3 pos = cornerTransform.position + cornerTransform.TransformDirection(new Vector3(fieldOffset, 0, fieldOffset));
                    allPoints[i].position = pos;
                    i++;
                }
        }
        public void CreatePoints(int size)
        {
            for (int i = 0; i < size; i++)
            {
                Point3 point = new Point3(i.ToString());
                allPoints.Add(point);
            }
        }
        void LoadPoints(List<Vector3> positions)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                allPoints[i].position = positions[i];
            }
        }
        public List<LonelyPoint> calculateLonelyPoints()
        {
            //allPoints = points;
            triangles.Clear();
            edges.Clear();
            createFieldOfTriangles(allPoints, ref triangles, ref edges);
            lonelyPoints.Clear(); 
            createLonelyPoints(edges, triangles, divideEdgeLonelyPointDistance, divideEdgeMaxPoints,ref lonelyPoints);
            return lonelyPoints;
        }
        void createLonelyPoints(List<Edge> edges, List<Triangle> triangles, float lonelyPointDistance, int maxPoints,ref List<LonelyPoint> lonelyPoints)
        {
            createLonelyPoints(edges, lonelyPointDistance, maxPoints,ref lonelyPoints);
            createLonelyPoints(triangles, lonelyPointDistance, maxPoints,ref lonelyPoints);
        }
        void createLonelyPoints(List<Triangle> triangles, float lonelyPointDistance, int maxPoints,ref List<LonelyPoint> lonelyPoints)
        {
            foreach (var triangle in triangles)
            {
                createLonelyPoints(triangle,lonelyPointDistance,maxPoints,ref lonelyPoints);
            }
        }
        void createLonelyPoints(List<Edge> edges, float lonelyPointDistance, int maxPoints, ref List<LonelyPoint> lonelyPoints)
        {
            foreach (var edge in edges)
            {
                List<LonelyPoint> lonelyPoints2 = createLonelyPoints(edge, lonelyPointDistance, maxPoints,ref lonelyPoints);
                //lonelyPoints.AddRange(lonelyPoints2);
            }
        }
        void createLonelyPoints(Triangle triangle, float lonelyPointDistance, int maxPoints, ref List<LonelyPoint> lonelyPoints)
        {
            Point3 barycenter = new Point3(triangle.getBarycenter());
            if(checkPoints_Point_Distance(triangle.points, barycenter.position, lonelyPointDistance))
            {
                LonelyPoint lonelyPoint = new LonelyPoint(barycenter.position,barycenter.pointName);
                
                lonelyPoints.Add(lonelyPoint);
            }
            foreach (var point in triangle.points)
            {
                List<Point3> points = divideEdge(barycenter, point, lonelyPointDistance, maxPoints);
                foreach (var point2 in points)
                {
                    if(checkPoints_Point_Distance(triangle.points, point2.position, lonelyPointDistance))
                    {
                        lonelyPoints.Add(new LonelyPoint(point2.position, point2.pointName));
                    }
                }
            }
            
        }
        List<Point3> divideEdge(Point3 point1,Point3 point2,float lonelyPointDistance,int maxCount)
        {
            Vector3 dir = point2.position - point1.position;
            float distance = dir.magnitude;
            dir.Normalize();
            float d1 = distance / lonelyPointDistance;
            List<Point3> points = new List<Point3>();
            if (d1 < maxCount + 1)
            {
                int a = Mathf.FloorToInt(d1);
                if (a == 0)
                {
                    return points;
                }
                float b = distance / a;
                for (int i = 1; i < a; i++)
                {
                    Vector3 pos = point1.position + dir * b*i;
                    points.Add(new Point3(pos));
                }
            }
            else
            {
                float a = distance/(maxCount+1);
                for (int i = 1; i < maxCount+1; i++)
                {
                    Vector3 pos = point1.position + dir * a*i;
                    points.Add(new Point3(pos));
                }
            }
            return points;
        }
        bool checkPoints_Point_Distance(List<Point3> points,Vector3 pointPosition,float lonelyPointDistance)
        {
            bool add = true;
            foreach (var point in points)
            {
                if (Vector3.Distance(pointPosition, point.position) < lonelyPointDistance)
                {
                    add = false;
                    break;
                }
            }
            return add;
        }
        List<LonelyPoint> createLonelyPoints(Edge edge, float lonelyPointDistance,int maxPoints, ref List<LonelyPoint> lonelyPoints)
        {
            List<Point3> points = divideEdge(edge.points[0], edge.points[1], lonelyPointDistance, maxPoints);
            foreach (var point in points)
            {
                if (checkPoints_Point_Distance(edge.points, point.position, lonelyPointDistance))
                {
                    lonelyPoints.Add(new LonelyPoint(point.position, point.pointName));
                }
            }
            return lonelyPoints;
        }
        void createFieldOfTriangles(List<Point3> points,ref List<Triangle> triangles,ref List<Edge> edges)
        {

            List<TrianglesAround> trianglesArounds = new List<TrianglesAround>();
            foreach (var point in points)
            {
                
                TrianglesAround trianglesAround = new TrianglesAround(point, allPoints);
                trianglesArounds.Add(trianglesAround);
            }
            foreach (var trianglesAround in trianglesArounds)
            {
                trianglesAround.addOthersTrianglesAround(trianglesArounds);
            }
            foreach (var trianglesAround in trianglesArounds)
            {
                trianglesAround.createTrianglesAround();
            }
            foreach (var trianglesAround in trianglesArounds)
            {
                triangles.AddRange(trianglesAround.myCreatedTriangles);
                edges.AddRange(trianglesAround.myCreatedEdges);
            }
        }
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (debug && Application.isPlaying && enabled)
            {
                //Color color = Teams.getTeamByName(teamName).Color;
                Gizmos.color = color;
                float i = 0.5f;
                float i2 = 0.5f;
                GUIStyle style = new GUIStyle();
                style.fontSize = 14;
                style.normal.textColor = Color.yellow;
                foreach (var point in allPoints)
                {
                    string text2 = point.pointName;
                    if (debugText)
                    {
                        //Handles.Label(point.position + Vector3.up * i, text2, style);
                    }
                }
                foreach (var edge in edges)
                {
                    Point3 lastPoint = null;
                    string text2 = edge.ToString();
                    if (debugText)
                    {
                        //Handles.Label(edge.getMiddlePoint() + Vector3.up * i, text2, style);
                    }
                    foreach (var point in edge.points)
                    {
                        if (lastPoint != null)
                        {
                            Gizmos.DrawLine(point.position + Vector3.up * i, lastPoint.position + Vector3.up * i);

                        }
                        lastPoint = point;
                    }
                }
                LonelyPoint lastLonelyPoint = new LonelyPoint();
                int j=0;
                bool computer = false;
                foreach (var lonelyPoint in lonelyPoints)
                {
                    j++;
                    Gizmos.DrawSphere(lonelyPoint.position + Vector3.up * i, 0.25f);
                    string text = j.ToString();
                    if (debugText)
                        //Handles.Label(lonelyPoint.point.position + Vector3.up * (i+0.25f), text, style);
                    if (computer)
                    {
                        float distance = Vector3.Distance(lastLonelyPoint.position, lonelyPoint.position);
                        string text2 = "d="+ distance;
                        if (debugText)
                        {

                            //Handles.Label(lonelyPoint.point.position + Vector3.up * (i+0.5f), text2, style);
                        }
                    }
                    lastLonelyPoint = lonelyPoint;
                    computer = true;
                }
                Team team = Teams.getTeamByName(teamName);
                print(team.TeamName + " Probes Generated = " + lonelyPoints.Count);
            }
        }
#endif
    }
    
    public class TrianglesAround
    {
        public class ClosestEdgeDataList
        {
            public Point3 point;
            public class ClosestEdgeData
            {
                public string type;
                public bool founded;
                public bool exist;
                public Point3 closestPoint;
                public Edge closestEdge;
                public float minAngle = Mathf.Infinity;
                public float ySign;
                public ClosestEdgeData(string type,float ySign)
                {
                    this.type = type;
                    this.ySign = ySign;
                }
            }
            public List<ClosestEdgeData> closestEdgeDatas = new List<ClosestEdgeData>();
            public ClosestEdgeDataList(Point3 point)
            {
                this.point = point;
                ClosestEdgeData closestEdgeData = new ClosestEdgeData("right",1);
                ClosestEdgeData closestEdgeData2 = new ClosestEdgeData("left",-1);
                closestEdgeDatas.Add(closestEdgeData);
                closestEdgeDatas.Add(closestEdgeData2);
            }
            public float getAngle(string type) => closestEdgeDatas.Find(x => x.type.Equals(type)).minAngle;
            public void setAngle(string type, float angle)=> closestEdgeDatas.Find(x => x.type.Equals(type)).minAngle = angle;
            public void setEdge(string type, Edge edge,Point3 otherPoint)
            {
                ClosestEdgeData closestEdgeData = closestEdgeDatas.Find(x => x.type.Equals(type));
                closestEdgeData.closestEdge = edge;
                closestEdgeData.closestPoint = otherPoint;
                closestEdgeData.exist = true;
            }
            public void notifyNewTriangle(Triangle triangle,Point3 basePoint)
            {
                List<Point3> points = triangle.getOtherPoints(basePoint);
                Point3 otherPoint = points.Find(x => !x.Equals(point));
                Vector3 dir1 = point.position - basePoint.position;
                Vector3 dir2 = otherPoint.position - basePoint.position;
                float y = Mathf.Sign(Vector3.Cross(dir1, dir2).y);
                closestEdgeDatas.Find(x => x.ySign == y).founded = true;
                closestEdgeDatas.Find(x => x.ySign == y).exist = true;
                closestEdgeDatas.Find(x => x.ySign == y).closestPoint = otherPoint;
                List<Edge> edges = triangle.getConectedEdges(basePoint);
                Edge edge = edges.Find(x => x.points.Contains(otherPoint));
                closestEdgeDatas.Find(x => x.ySign == y).closestEdge = edge;
            }
            public bool founded(string type) => closestEdgeDatas.Find(x => x.type.Equals(type)).founded;
        }
        public class EdgeWithDirection
        {
            public Point3 originPoint;
            public Point3 targetPoint;
            public Vector3 direction;
            public Edge edge;
            public EdgeWithDirection(Point3 originPoint, Point3 targetPoint,Edge edge)
            {
                this.originPoint = originPoint;
                this.targetPoint = targetPoint;
                direction = targetPoint.position - originPoint.position;
                this.edge = edge;
            }
        }
        public Point3 basePoint;
        public List<Point3> otherPoints = new List<Point3>();
        public List<Point3> otherPointsCompleted = new List<Point3>();
        public LimitList limits;
        public List<Triangle> triangles = new List<Triangle>();
        public List<Edge> edges { get; set; } = new List<Edge>();
        public List<Edge> myCreatedEdges { get; set; } = new List<Edge>();
        public List<Triangle> myCreatedTriangles { get; set; } = new List<Triangle>();
        public List<EdgeWithDirection> edgesWithDirection { get; set; } = new List<EdgeWithDirection>();
        public List<ClosestEdgeDataList> closestEdgeDatas { get; set; } = new List<ClosestEdgeDataList>();
        List<TrianglesAround> othersTrianglesAround;
        bool isCompleted;
        public TrianglesAround(Point3 basePoint, List<Point3> allPoints)
        {
            this.basePoint = basePoint;
            limits = new LimitList(basePoint);

            foreach (var item in allPoints)
            {
                otherPoints.Add(item);
            }
            otherPoints.Remove(basePoint);
            otherPoints.Sort(basePoint.Sort);
            int i = 1;
            foreach (var point in otherPoints)
            {
                
                closestEdgeDatas.Add(new ClosestEdgeDataList(point));
                i++;
            }
        }
        public void createTrianglesAround()
        {
            foreach (var otherPoint in otherPoints)
            {
                if (isCompleted)
                    break;
                if (!otherPointsCompleted.Contains(otherPoint))
                {
                    createTrianglesOfPoint(otherPoint);
                }
            }
        }
        void createTrianglesOfPoint(Point3 otherPoint)
        {
            Edge otherPointEdge = getEdgeWithPoint(otherPoint);
            bool otherPointEdgeCreatedByMe = false;
            bool otherPointEdgeCreatedByMeIsAdded = false;
            if (otherPointEdge == null)
            {
                if (limits.isInside(otherPoint))
                {
                    closestEdgeDatas.RemoveAll(x => x.point.Equals(otherPoint));
                    return;
                }
                else
                {
                    otherPointEdge = new Edge(basePoint, otherPoint);
                    otherPointEdgeCreatedByMe = true;
                }
            }
            ClosestEdgeDataList closestEdgeDataList = closestEdgeDatas.Find(x => x.point.Equals(otherPoint));
            Vector3 otherPointDir = otherPoint.position - basePoint.position;
            foreach (var edgeWithDirection in edgesWithDirection)
            {
                if (!edgeWithDirection.targetPoint.Equals(otherPoint))
                {
                    int ySign = (int)Mathf.Sign(Vector3.Cross(otherPointDir, edgeWithDirection.direction).y);

                    string type = ySign == 1 ? "right" : "left";
                    if (!closestEdgeDataList.founded(type))
                    {
                        float angle = closestEdgeDataList.getAngle(type);
                        float angle2 = Vector3.Angle(otherPointDir, edgeWithDirection.direction);
                        if (angle2 < angle)
                        {
                            closestEdgeDataList.setEdge(type, edgeWithDirection.edge, edgeWithDirection.targetPoint);
                            closestEdgeDataList.setAngle(type, angle2);
                        }
                    }
                }
            }

            if (!closestEdgeDataList.closestEdgeDatas.Exists(x => !x.founded && x.exist) && !closestEdgeDataList.closestEdgeDatas.Exists(x => x.founded))
            {
                otherPointEdgeCreatedByMeIsAdded = addEdge(otherPointEdge, otherPoint, true, true, otherPointEdgeCreatedByMe);
            }
            foreach (var closestEdgeData in closestEdgeDataList.closestEdgeDatas)
            {
                if (!closestEdgeData.founded)
                {
                    if (closestEdgeData.exist)
                    {
                        List<Edge> triangleEdges = new List<Edge>();

                        triangleEdges.Add(otherPointEdge);
                        triangleEdges.Add(closestEdgeData.closestEdge);

                        TrianglesAround trianglesAround = getTrianglesAroundFromPoint(otherPoint);
                        Edge externEdge = null;
                        if (trianglesAround != null)
                        {
                            externEdge = trianglesAround.getEdgeWithPoint(closestEdgeData.closestPoint);
                        }
                        bool externEdgeIsCreated = false;
                        if (externEdge != null)
                        {
                            triangleEdges.Add(externEdge);
                        }
                        else
                        {
                            externEdge = new Edge(otherPoint, closestEdgeData.closestPoint);
                            triangleEdges.Add(externEdge);
                            externEdgeIsCreated = true;
                        }
                        Triangle newTriangle = new Triangle(triangleEdges);
                        if (callCheckNewTriangle(newTriangle, basePoint))
                        {
                            if (externEdgeIsCreated)
                            {
                                TrianglesAround otherTriangleAround = getTrianglesAroundFromPoint(otherPoint);
                                otherTriangleAround.myCreatedEdges.Add(externEdge);
                            }
                            if (otherPointEdgeCreatedByMe && !otherPointEdgeCreatedByMeIsAdded)
                            {
                                myCreatedEdges.Add(otherPointEdge);
                                otherPointEdgeCreatedByMeIsAdded = true;
                            }
                            addTriangle(newTriangle,true);
                            callNotifyNewTriangle(newTriangle, otherPoint);
                            callNotifyNewTriangle(newTriangle, closestEdgeData.closestPoint);
                        }
                    }
                }
            }
        }
        public List<Edge> getAllEdgesOfTriangles()
        {
            List<Edge> edges = new List<Edge>();
            foreach (var triangle in triangles)
            {
                foreach (var edge in triangle.edges)
                {
                    if (!edges.Contains(edge))
                    {
                        edges.Add(edge);
                    }
                }
            }
            return edges;
        }
        public void addOthersTrianglesAround(List<TrianglesAround> _othersTrianglesAround)
        {
            othersTrianglesAround = new List<TrianglesAround>();
            foreach (var item in _othersTrianglesAround)
            {
                if (!item.Equals(this))
                {
                    othersTrianglesAround.Add(item);
                }
            }
        }
        public TrianglesAround getTrianglesAroundFromPoint(Point3 point)
        {
            foreach (var item in othersTrianglesAround)
            {
                if (item.basePoint.Equals(point))
                {
                    return item;
                }
            }
            return null;
        }
        public Edge getEdgeWithPoint(Point3 point)
        {
            foreach (var edge in edges)
            {
                if (edge.points.Contains(point))
                {
                    return edge;
                }
            }
            return null;
        }
        bool callCheckNewEdge(Edge edge,Point3 point,Point3 otherPoint)
        {
            TrianglesAround trianglesAround = getTrianglesAroundFromPoint(point);
            if (trianglesAround != null)
            {
               return trianglesAround.checkNewEdge(edge, otherPoint);
            }
            else{ return false; }
        }
        void callNotifyNewTriangle(Triangle triangle, Point3 otherPoint)
        {
            TrianglesAround trianglesAround = getTrianglesAroundFromPoint(otherPoint);
            if (trianglesAround != null)
            {
                trianglesAround.addTriangle(triangle,false);
            }
        }
       
        bool addEdge(Edge edge,Point3 otherPoint,bool checkIfExist,bool notifyToOtherPoint,bool isCreatedForMe)
        {
            if (checkIfExist)
            {
                if (!edges.Contains(edge))
                {
                    if (notifyToOtherPoint)
                    {
                        if(callCheckNewEdge(edge, otherPoint, basePoint))
                        {
                            addEdge(edge, otherPoint, isCreatedForMe);
                            return true;
                        }
                    }
                    else
                    {
                        addEdge(edge, otherPoint, isCreatedForMe);
                        return true;
                    }
                }
            }
            else
            {
                if (notifyToOtherPoint)
                {
                    if(callCheckNewEdge(edge, otherPoint, basePoint))
                    {
                        addEdge(edge, otherPoint, isCreatedForMe);
                        return true;
                    }
                }
                else
                {
                    addEdge(edge, otherPoint, isCreatedForMe);
                    return true;
                }
            }
            return false;
        }
        public void notifyNewEdge(Edge edge, Point3 otherPoint)
        {
            if (!edges.Contains(edge))
            {
                edges.Add(edge);
                EdgeWithDirection edgeWithDirection = new EdgeWithDirection(basePoint, otherPoint, edge);
                edgesWithDirection.Add(edgeWithDirection);
            }
        }
        void addEdge(Edge edge, Point3 otherPoint,bool isCreatedForMe)
        {
            if (isCreatedForMe)
            {
                myCreatedEdges.Add(edge);
            }
            edges.Add(edge);
            EdgeWithDirection edgeWithDirection = new EdgeWithDirection(basePoint, otherPoint, edge);
            edgesWithDirection.Add(edgeWithDirection);
        }
        public bool checkNewEdge(Edge edge,Point3 otherPoint)
        {
            if (!limits.isInside(otherPoint)&&!isCompleted)
            {
                notifyNewEdge(edge, otherPoint);
                return true;
            }
            else { return false; }
        }
        public bool callCheckNewTriangle(Triangle triangle, Point3 otherPoint)
        {
            List<Point3> otherPoints = triangle.getOtherPoints(basePoint);

            foreach (var point in otherPoints)
            {
                TrianglesAround trianglesAround = getTrianglesAroundFromPoint(point);
                if (trianglesAround != null)
                {
                    if(!trianglesAround.checkNewTriangle(triangle, otherPoint))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }
        public bool checkNewTriangle(Triangle triangle, Point3 otherPoint)
        {
            if (limits.isInside(otherPoint)|| isCompleted)
            {
                return false;
            }
            return true;
        }
        public void addTriangle(Triangle triangle,bool isCreatedByMe)
        {
            if (!triangles.Contains(triangle))
            {
                triangles.Add(triangle);
                if (isCreatedByMe)
                {
                    myCreatedTriangles.Add(triangle);
                }
                List<Edge> edges = triangle.getConectedEdges(basePoint);
                edges.ForEach(x => addEdge(x, x.getFirstOtherPoint(basePoint),true,false, false));
                Limit.NotifyNewTriangleResult notifyNewTriangleResult = limits.notifyNewTriangle(triangle);
                List<Point3> points = triangle.getOtherPoints(basePoint);
                foreach (var point in points)
                {
                    ClosestEdgeDataList closestEdgeDataList = closestEdgeDatas.Find(x=>x.point.Equals(point));
                    if (closestEdgeDataList != null)
                    {
                        bool closestEdgeDatasCompleted = notifyNewTriangleResult.closestEdgeData.Find(x => x.joinedPoint.Equals(point)) != null;
                        if (closestEdgeDatasCompleted)
                        {
                            closestEdgeDatas.Remove(closestEdgeDataList);
                            otherPointsCompleted.Add(point);
                            edgesWithDirection.RemoveAll(x => x.targetPoint.Equals(point));
                        }
                        else
                        {
                            closestEdgeDataList.notifyNewTriangle(triangle, basePoint);
                        }
                    }
                }
                isCompleted = notifyNewTriangleResult.isCompleted;
            }
        }
#if UNITY_EDITOR
        public void DrawGizmos()
        {
            float i = 0.1f;
            float i2 = 0.5f;
            GUIStyle style = new GUIStyle();
            style.fontSize = 14;
            style.normal.textColor = Color.yellow;

            string text = "Base Point3";
            Handles.Label(basePoint.position + Vector3.up * i, text, style);
            int j = 1;
            foreach (var item in otherPoints)
            {
                float distance = Vector3.Distance(basePoint.position, item.position);
                string text2 = item.pointName + " distance=" + distance;
                Handles.Label(item.position + Vector3.up * i, text2, style);
                j++;
            }
            Gizmos.color = new Color(0.25f, 0.75f, 1);
            j = 1;
            i += i2;
            foreach (var edge in getAllEdgesOfTriangles())
            {
                Point3 lastPoint = null;
                string text2 = "Edge "+j;
                Handles.Label(edge.getMiddlePoint() + Vector3.up * i, text2, style);
                foreach (var point in edge.points)
                {
                    if(lastPoint!=null)
                    {
                        Gizmos.DrawLine(point.position + Vector3.up * i, lastPoint.position + Vector3.up * 0.5f);
                        
                    }
                    lastPoint = point;
                }
                j++;
            }
        }
#endif
    }


}
/*
 public void createTrianglesAround()
        {
            foreach (var otherPoint in otherPoints)
            {
                Limit closestLimit;
                Limit.Line closestLine;
                if (!limits.isInside(otherPoint,out closestLimit,out closestLine))
                {
                    if (closestLimit == null)
                    {
                        closestLimit = new Limit(basePoint);
                        limits.AddLimit(closestLimit);
                        Edge edge = createEdge(otherPoint);
                        closestLimit.setEdge(edge,otherPoint);
                        callNotifyNewEdge(edge, otherPoint, basePoint);
                    }
                    else
                    {
                        if (closestLine != null)
                        {
                            Point3 closestLinePoint= closestLine.point;
                            List<Edge> triangleEdges = new List<Edge>();
                            triangleEdges.Add(closestLine.edge);
                            Edge edge = createEdge(otherPoint);
                            closestLimit.setEdge(edge, otherPoint);
                            triangleEdges.Add(edge);
                            TrianglesAround trianglesAround = callNotifyNewEdge(edge,otherPoint,basePoint);
                            Edge externEdge=null;
                            if (trianglesAround != null)
                            {
                                externEdge = trianglesAround.getEdgeWithPoint(closestLinePoint);
                                triangleEdges.Add(externEdge);
                            }
                            if (externEdge != null)
                            {
                                triangleEdges.Add(externEdge);
                            }
                            else
                            {
                                externEdge = new Edge(otherPoint, closestLinePoint);
                                triangleEdges.Add(externEdge);
                                callNotifyNewEdge(externEdge, otherPoint, closestLinePoint);
                                callNotifyNewEdge(externEdge, closestLinePoint, otherPoint);
                            }
                            Triangle newTriangle = new Triangle(triangleEdges);
                            addTriangle(newTriangle);
                        }
                    }
                }
            }
        }
 */


