using FieldTriangleSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using FieldTriangleV2;
using Unity.Collections;
using UnityEditor;

namespace NextMove_Algorithm
{

    public class SearchLonelyPointsManager : MonoBehaviour
    {
        [System.Serializable]
        public class SearchLonelyPointsParams
        {
            public int maxSizeEdgeBuffer = 100;
            public int teamSize = 11;
            public int maxSizeTriangleBuffer = 100;
            public int maxSizeLonelyPointsBuffer = 200;
            public int minLonelyPointDistance = 5;
            public int divideEdgeMaxPoints = 5;
            public float minNormalizedArea = 0.5f;
            public float fieldOffset = 5f;
        }
        [System.Serializable]
        public class TeamDebug
        {
            public bool debug, debugTriangles, debugEdges, debugPoints, debugLonelyPoints, debugTriangleText, debugText;
        }
        int extraPoints = 4;
        
        public List<Transform> pointTransforms;
       
        NextMoveSystem nextMoveSystem;
        EntityManager entityManager;
        public Dictionary<string,Entity> searchLonelyPointsEntitys { get; set; }
        public SearchLonelyPointsParams searchLonelyPointsParams;
        public bool testingMode;
        public string testingTeam;
        public Transform testPointsParentTransform;
        [HideInInspector]
        public List<string> teamNames = new List<string>();
        
        public List<TeamDebug> teamDebugs = new List<TeamDebug>();
        float gizmoSphereRadio = 0.25f;
        void Start()
        {
            if (enabled)
            {
                searchLonelyPointsEntitys = new Dictionary<string, Entity>();
                entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                nextMoveSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<NextMoveSystem>();
                //nextMoveSystem.Enabled = false;
                
                if (testingMode)
                {
                    getChildsPoints();
                    createEntities(testingTeam);
                    teamNames.Add(testingTeam);
                }
                else
                {
                    Teams.teamsAreLoadedEvent.AddListenerConsiderInvoked(teamsSetuped);
                }
                    
                nextMoveSystem.InitialNextMoveCreator.Start();
                nextMoveSystem.NextMoveSystemManager = this;
            }
        }
        void getChildsPoints()
        {
            int j = 0;
            for (int i = 0; i < testPointsParentTransform.childCount; i++)
            {
                if (testPointsParentTransform.GetChild(i).gameObject.activeInHierarchy)
                {
                    Transform pointTransform = testPointsParentTransform.GetChild(i);
                    pointTransform.name = "Point " + j;
                    pointTransforms.Add(pointTransform);
                    j++;
                }
            }
        }
        void createEntities(string teamName)
        {
            Entity searchLonelyPointsEntity = entityManager.CreateEntity();
            DynamicBuffer<PointElement> points = entityManager.AddBuffer<PointElement>(searchLonelyPointsEntity);
            NativeArray<PointElement> pointsArray = new NativeArray<PointElement>(new PointElement[searchLonelyPointsParams.teamSize + extraPoints], Allocator.Temp);
            points.AddRange(pointsArray);

            DynamicBuffer<EdgeElement> edges = entityManager.AddBuffer<EdgeElement>(searchLonelyPointsEntity);
            NativeArray<EdgeElement> edgesArray = new NativeArray<EdgeElement>(new EdgeElement[searchLonelyPointsParams.maxSizeEdgeBuffer], Allocator.Temp);
            edges.AddRange(edgesArray);

            DynamicBuffer<TriangleElement> triangles = entityManager.AddBuffer<TriangleElement>(searchLonelyPointsEntity);
            NativeArray<TriangleElement> trianglesArray = new NativeArray<TriangleElement>(new TriangleElement[searchLonelyPointsParams.maxSizeTriangleBuffer],Allocator.Temp);
            triangles.AddRange(trianglesArray);

            DynamicBuffer<LonelyPointElement> lonelyPoints = entityManager.AddBuffer<LonelyPointElement>(searchLonelyPointsEntity);
            NativeArray<LonelyPointElement> lonelyPointsArray = new NativeArray<LonelyPointElement>(new LonelyPointElement[searchLonelyPointsParams.maxSizeLonelyPointsBuffer], Allocator.Temp);
            lonelyPoints.AddRange(lonelyPointsArray);

            BufferSizeComponent bufferSizeComponent = new BufferSizeComponent(0, searchLonelyPointsParams.minLonelyPointDistance, searchLonelyPointsParams.divideEdgeMaxPoints, searchLonelyPointsParams.minNormalizedArea, searchLonelyPointsEntity);
            entityManager.AddComponentData<BufferSizeComponent>(searchLonelyPointsEntity,bufferSizeComponent);
            pointsArray.Dispose();
            edgesArray.Dispose();
            trianglesArray.Dispose();
            lonelyPointsArray.Dispose();
            searchLonelyPointsEntitys.Add(teamName,searchLonelyPointsEntity);
        }
        private void teamsSetuped()
        {
            foreach (var team in Teams.teamsList)
            {
                teamNames.Add(team.TeamName);
                createEntities(team.TeamName);
            }
            MatchEvents.footballFieldLoaded.AddListenerConsiderInvoked(footballFieldLoaded);
        }
        public void UpdatePoints(string teamName)
        {
            Entity searchLonelyPointsEntity = searchLonelyPointsEntitys[teamName];
            BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
            DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);
            if (testingMode)
            {
                for (int i = 4; i < pointTransforms.Count; i++)
                {
                    PointElement pointElement = points[i];
                    pointElement.index = i;
                    pointElement.position.x = pointTransforms[i].position.x;
                    pointElement.position.y = pointTransforms[i].position.z;
                    points[i] = pointElement;
                }
                bufferSizeComponent.pointSize = pointTransforms.Count;
                bufferSizeComponent.edgesResultSize = 0;
                bufferSizeComponent.trianglesResultSize = 0;
                bufferSizeComponent.lonelyPointsResultSize = 0;
                entityManager.SetComponentData<BufferSizeComponent>(searchLonelyPointsEntity, bufferSizeComponent);
            }
            else
            {
                if (Teams.teamsList.Exists(x => x.TeamName.Equals(teamName)))
                {
                    Team team = Teams.teamsList.Find(x => x.TeamName.Equals(teamName));
                    for (int i = 0; i < team.publicPlayerDatas.Count ; i++)
                    {
                        PointElement pointElement = points[i + extraPoints];
                        pointElement.index = i + extraPoints;
                        pointElement.position.x = team.publicPlayerDatas[i].position.x;
                        pointElement.position.y = team.publicPlayerDatas[i].position.z;
                        points[i + extraPoints] = pointElement;
                    }
                    bufferSizeComponent.pointSize = team.publicPlayerDatas.Count + extraPoints;
                    bufferSizeComponent.edgesResultSize = 0;
                    bufferSizeComponent.trianglesResultSize = 0;
                    bufferSizeComponent.lonelyPointsResultSize = 0;
                    entityManager.SetComponentData<BufferSizeComponent>(searchLonelyPointsEntity, bufferSizeComponent);
                }
            }
        }
        public List<Point> getCurrentTeamPoints(string teamName)
        {
            
            return new List<Point>();
        }
        private void footballFieldLoaded()
        {
            if (!testingMode)
            {
                foreach (var searchLonelyPointsEntity in searchLonelyPointsEntitys.Values)
                {
                    int i = 0;
                    DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);
                    foreach (var corner in MatchComponents.footballField.cornersComponents)
                    {
                        Transform cornerTransform = corner.cornerPoint;
                        Vector3 pos = cornerTransform.position + cornerTransform.TransformDirection(new Vector3(searchLonelyPointsParams.fieldOffset, 0, searchLonelyPointsParams.fieldOffset));
                        PointElement point = new PointElement(new Vector2(pos.x, pos.z), false, i);
                        points[i] = point;
                        i++;
                    }
                }
            }
            /*
            foreach (var sideOfField in MatchComponents.footballField.sideOfFields)
            {
                foreach (var teamData in teamDatas)
                {
                    Vector3 position = sideOfField.goalComponents.centerMatchStoppedState.position;
                    PointElement point = new PointElement(new Vector2(position.x, position.z), false, i);
                    points[i] = point;
                }
                i++;
            }*/
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (Application.isPlaying && enabled)
            {
                int k = 0;
                foreach (var team in Teams.teamsList)
                {
                    TeamDebug teamDebug = teamDebugs[k];
                    k++;
                    if (!teamDebug.debug) continue;
                    
                    Entity searchLonelyPointsEntity = searchLonelyPointsEntitys[team.TeamName];
                    Color color = team.Color;
                    Gizmos.color = color;
                    GUIStyle style = new GUIStyle();
                    style.normal.textColor = Color.red;
                    DynamicBuffer<EdgeElement> edges = entityManager.GetBuffer<EdgeElement>(searchLonelyPointsEntity);
                    DynamicBuffer<TriangleElement> triangles = entityManager.GetBuffer<TriangleElement>(searchLonelyPointsEntity);
                    DynamicBuffer<PointElement> points = entityManager.GetBuffer<PointElement>(searchLonelyPointsEntity);
                    BufferSizeComponent bufferSizeComponent = entityManager.GetComponentData<BufferSizeComponent>(searchLonelyPointsEntity);
                    if (teamDebug.debugPoints)
                    {
                        for (int i = 0; i < bufferSizeComponent.pointSize; i++)
                        {
                            Vector3 pos = getVector3(points[i].position, 1);
                            Handles.Label(pos, "point " + points[i].index, style);
                            Gizmos.DrawSphere(getVector3(points[i].position, 1.0f), 0.25f);
                        }
                    }
                    if (teamDebug.debugTriangles)
                    {
                        for (int i = 0; i < bufferSizeComponent.trianglesResultSize; i++)
                        {

                            TriangleElement triangle = triangles[i];
                            if (triangle.thereIsTrianglesInside) continue;
                            PointElement p1 = points[triangle.pointIndex1];
                            PointElement p2 = points[triangle.pointIndex2];
                            PointElement p3 = points[triangle.pointIndex3];
                            Vector3 barycenter = getBarycenter(p1, p2, p3);
                            float area = SearchLonelyPointsJob.getArea(triangle, points);
                            float perimeter = SearchLonelyPointsJob.getPerimeter(triangle, points);
                            float normalizedArea = area / perimeter;
                            if (teamDebug.debugTriangleText)
                            {
                                Handles.Label(barycenter + Vector3.up * i * 0.1f, "triangle " + triangle.index.ToString() + " area=" + area + " perimeter=" + perimeter + " normalizedArea=" + normalizedArea, style);
                                //Handles.Label(barycenter + Vector3.up*0.1f, "triangle "+ triangle.index.ToString() + " nextTriangles="+triangle.triangleIndex1+"," + triangle.triangleIndex2 + "," + triangle.triangleIndex3 + " edges="+triangle.edgeIndex1 + "," + triangle.edgeIndex2 + "," + triangle.edgeIndex3);
                                //Handles.Label(barycenter + Vector3.up * 0.4f, "triangle " + triangle.index.ToString() + " edgeSign=" + triangle.edgeSign1 + "," + triangle.edgeSign2 + "," + triangle.edgeSign3 + " thereIsTriangleInside=" + triangle.thereIsTrianglesInside);
                            }


                            Gizmos.DrawLine(getVector3(p1.position, 1.0f), getVector3(p2.position, 1.0f));
                            Gizmos.DrawLine(getVector3(p1.position, 1.0f), getVector3(p3.position, 1.0f));
                            Gizmos.DrawLine(getVector3(p2.position, 1.0f), getVector3(p3.position, 1.0f));

                        }
                    }
                    if (teamDebug.debugEdges)
                    {
                        for (int i = 0; i < bufferSizeComponent.edgesResultSize; i++)
                        {
                            EdgeElement edge = edges[i];
                            PointElement p1 = points[edge.pointIndex1];
                            PointElement p2 = points[edge.pointIndex2];
                            Vector3 middle = getMiddlePoint(p1, p2);
                            if (teamDebug.debugText)
                                Handles.Label(middle + Vector3.up * 0.1f, "edge " + edge.index.ToString() + " points=" + edge.pointIndex1 + "," + edge.pointIndex2 + " isLimit=" + edge.isLimit + " limits=" + edge.edgeLimit1 + "," + edge.edgeLimit2 + " tIndex=" + edge.triangleLimitIndex + " tEdgeIndex=" + edge.nextTriangleEdgeIndex, style);
                            Gizmos.DrawLine(getVector3(p1.position, 1.0f), getVector3(p2.position, 1.0f));
                            //DrawArrow.ForGizmo(getVector3(p1.position, 1.0f), getVector3(p2.position, 1.0f) - getVector3(p1.position, 1.0f), 1);
                        }
                    }
                    if (teamDebug.debugLonelyPoints)
                    {
                        DynamicBuffer<LonelyPointElement> lonelyPointElements = entityManager.GetBuffer<LonelyPointElement>(searchLonelyPointsEntity);
                        for (int i = 0; i < bufferSizeComponent.lonelyPointsResultSize; i++)
                        {
                            LonelyPointElement lonelyPoint = lonelyPointElements[i];
                            Vector3 pos = getVector3(lonelyPoint.position, 1);
                            if (teamDebug.debugText)
                                Handles.Label(pos, "LonelyPoint " + lonelyPoint.index.ToString(), style);
                            
                            Gizmos.DrawSphere(pos, gizmoSphereRadio);

                        }
                        if (bufferSizeComponent.lonelyPointsResultSize > 0 && teamDebug.debugText)
                        {

                            print(team.TeamName + " Probes Generated = " + bufferSizeComponent.lonelyPointsResultSize);
                        }
                    }
                }
            }
        }
#endif
        Vector3 getVector3(Vector2 vector2,float y)
        {
            return new Vector3(vector2.x, y, vector2.y);
        }
        public Vector3 getMiddlePoint(PointElement point1, PointElement point2)
        {
            Vector3 pos1 = getVector3(point1.position,1);
            Vector3 pos2 = getVector3(point2.position, 1);
            float d = Vector2.Distance(point1.position, point2.position);
            d = d / 2;
            Vector3 dir = pos1 - pos2;
            dir.Normalize();
            return pos2 + dir * d;
        }
        public Vector3 getBarycenter(PointElement p1,PointElement p2, PointElement p3)
        {
            Vector3 pos1 = getVector3(p1.position, 1);
            Vector3 pos2 = getVector3(p2.position, 1);
            Vector3 pos3 = getVector3(p3.position, 1);
            Vector3 result = (pos1 + pos2 + pos3) / 3;
            return result ;
        }
    }
}
