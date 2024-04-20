using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
public class GoalkeeperCtrl : MonoBehaviour,ILoad
{
    public GoalkeeperValues goalkeeperValues;
    private BallComponents componentsBall;
    public List<PublicPlayerData> enemyPlayerDataList = new List<PublicPlayerData>(), partnetPlayerDataList = new List<PublicPlayerData>();
    List<ChaserData> arriveBeforeOthersEnemyPlayerDataList = new List<ChaserData>();
    public Transform goalKeeperTransform;
    Transform goalkeeperBaseTrans { get => wirst1; }
    public Transform wirst1;
    public floatDelegate positionFunction;
    public Area Area { get=> sideOfField.bigArea; }
    public SideOfField sideOfField;
    public Vector3 targetPosition;
    Vector3 targetDirLook;
    Vector3 lastBallDirection;
    Vector3 advancedGoalkeeperCenterPosition;
    bool goToOptimalPoint;
    float minPositionY;
    Coroutine coroutine;
    public static int _loadLevel = MatchEvents.staticLoadLevel + 1;
    public int loadLevel { get => _loadLevel; set => _loadLevel=value; }

    public BallComponents ComponentsBall { get => componentsBall; set { componentsBall = value; checkEnable(); } }

    public PlaneWithLimits BallGoesInsidePlane { get => sideOfField.goalComponents.ballGoesInsidePlane; }
    GoalComponents GoalComponents { get => sideOfField.goalComponents; }
    public PublicPlayerData myPublicPlayerData { get; set; }
    ChaserData myChaserData{ get => getMyFirstChaserData(); }
    MyObsoleteBehaviour myBehaviour = new MyObsoleteBehaviour();
    Vector3 optimalPointReachBall,closestPoint;
    public bool drawGizmos;
    string currentOptimalPoint,currentClosestPoint;
    string currentState;
    string arriveOptimalPoint,arriveClosestPoint;
    bool changeDirection;
    Vector3 ballGoesInsideClosestPoint;
    Vector3 intersectionCenter;
    bool _ballGoesInside;
    float closestPointTime;
    public SimplePass pass;
    bool locked = false;
    public Transform test;
    public bool enableRandomPass;
    //OptimalPosition optimalPosition;
    public void Load(int level)
    {
        if (loadLevel == level)
        {
            //Load();
        }
    }
    ChaserData getMyFirstChaserData()
    {
        ChaserData chaserData;
        myPublicPlayerData.getFirstChaserData(out chaserData);
        return chaserData;
    }
    bool getFirstChaserData(out ChaserData firstChaserData)
    {
        List<ChaserData> enemysChaserData = ChaserData.getChaserDataOfPublicPlayerDatas(enemyPlayerDataList);
        enemysChaserData.Sort(ChaserData.CompareBySelectedOptimalTime);
        if (enemysChaserData.Count > 0)
        {
            firstChaserData = enemysChaserData[0];
            return true;
        }
        else
        {
            firstChaserData = null;
            return false;
        }
    }
    public void Load()
    {
        //Para que el guante no atraviese el suelo
        minPositionY = Mathf.Abs(goalKeeperTransform.position.y - wirst1.position.y);
        myPublicPlayerData.addedChaserDataEvent.addObserverAndExecuteIfValueNotIsNull(()=>myPublicPlayerData.addedChaserDataEvent.Value.AddListenerConsiderInvoked(checkEnable));
        MatchEvents.publicPlayerDataOfAddedPlayerToTeamIsAvailable.AddListener(playerAddedToTeam);
        MatchEvents.otherPlayerLeftRoom.AddListener(removePlayer);
        tag = Tags.GoalKeeper;
        enemyPlayerDataList.RemoveAll(item => !item.gameObject.activeInHierarchy);
        partnetPlayerDataList.RemoveAll(item => !item.gameObject.activeInHierarchy);
        //coroutine = StartCoroutine(calculateTargetsPoints());

        ObsoleteState _theMatchIsOnState = new ObsoleteState();
        _theMatchIsOnState.addEnableTrigger(MatchEvents.warmUp);
        //_theMatchIsOnState.addEnableTrigger(MatchEvents.endMatch);
        _theMatchIsOnState.addEnableTrigger(MatchEvents.continueMatch);
        _theMatchIsOnState.addDisableTrigger(MatchEvents.endPart);
        _theMatchIsOnState.addDisableTrigger(MatchEvents.stopMatch);
        _theMatchIsOnState.addDisableTrigger(MatchEvents.endMatch);
        _theMatchIsOnState.addFunction(theMatchIsOnState);
        ObsoleteState _theMatchIsStoppedState = new ObsoleteState();
        _theMatchIsStoppedState.addEnableTrigger(MatchEvents.endPart);
        _theMatchIsStoppedState.addEnableTrigger(MatchEvents.endMatch);
        _theMatchIsStoppedState.addEnableTrigger(MatchEvents.stopMatch);
        _theMatchIsStoppedState.addDisableTrigger(MatchEvents.continueMatch);
        _theMatchIsStoppedState.addFunction(theMatchIsStoppedState);
        //Calculo calculateDirCenterPositionCalculo = new Calculo(calculateDirCenterPosition);
        //calculateDirCenterPositionCalculo.addListener(_theMatchIsOnState);
        //myBehaviour.addCalculo(calculateDirCenterPositionCalculo);
        bool theMatchIsOn = MatchData.matchState == MatchState.WarmUp || MatchData.isStarted;
        myBehaviour.addState(_theMatchIsOnState, theMatchIsOn);
        myBehaviour.addState(_theMatchIsStoppedState,!theMatchIsOn);
        addGoalkeeperToGoalkeeperList();
        //BallGoesInsidePlane.buildPlanes(-Vector3.forward*ComponentsBall.radio);
        checkEnable();
        
        //optimalPosition = new OptimalPosition();
    }
    void Update()
    {
        
        //goalKeeperTransform.position += Vector3.up * 2 * Time.fixedDeltaTime;
        positionFunction(Time.deltaTime);
        //print("ball velocity = "+componentsBall.rigBall.velocity.magnitude);
    }
    public void checkEnable()
    {
        if (sideOfField != null && ComponentsBall != null && Area != null && BallGoesInsidePlane != null && Area.checkAreaLoaded() && BallGoesInsidePlane.checkAreaLoaded() && GoalComponents !=null && myPublicPlayerData!=null && myPublicPlayerData.ChaserDataList.Count>0)
        {
            MyEnable();
        }
        else
        {
            MyDisable();
        }
    }
    public void Lock()
    {
        locked = true;
        MyDisable();
    }
    public void Unlock()
    {
        locked = false;
        checkEnable();
    }
    public void setSideOfField(SideOfField sideOfField)
    {
        this.sideOfField = sideOfField;
        targetPosition = GoalComponents.centerMatchStoppedState.position;
        sideOfField.isLoadedEvent.AddListenerConsiderInvoked(checkEnable);
        //pass.direction = sideOfField.goalComponents.forward;
    }
    public void MyDisable()
    {
        if (!locked)
        {
            enabled = false;
        }
    }
    public void MyEnable()
    {
        if (!locked)
        {
            enabled = true;
        }
    }
    public void addGoalkeeperToGoalkeeperList()
    {
        GoalkeeperList.addGoalkeeper(this);
    }
   
    bool checkIfGoalkeeperArrivesBeforeOthers()
    {
        //List<ChaserData> enemyChaserDataList = ChaserData.getChaserDataOfPublicPlayerDatas(enemyPlayerDataList);
        
        if(ChaserData.checkFirstChaserDatas(myChaserData, arriveBeforeOthersEnemyPlayerDataList, 0.1f))
        {
            if (arriveBeforeOthersEnemyPlayerDataList.Count > 0)
            {
                //enemyChaserDataList.Sort(ChaserData.CompareBySlectedOptimalTime);
                float enemyMinTimeToGetOptimalPoint = arriveBeforeOthersEnemyPlayerDataList[0].getGlobalOptimalTime();
                foreach (var partnetPlayerData in partnetPlayerDataList)
                {
                    ChaserData partnerChaserData;
                    if (partnetPlayerData.getFirstChaserData(out partnerChaserData))
                    {
                        if (enemyMinTimeToGetOptimalPoint - partnerChaserData.getGlobalOptimalTime() > 0.5f)
                        {
                            arriveOptimalPoint = "you first but partner";
                            return false;
                        }
                    }
                }
            }
            else
            {
                if(partnetPlayerDataList.Count > 0)
                {
                    arriveOptimalPoint = "No enemys-partner";
                    return false;
                }
                else
                {
                    arriveOptimalPoint = "No enemys-No partner";
                    return true;
                }
            }
        }
        else
        {
            arriveOptimalPoint = "enemys";
            return false;
        }
        arriveOptimalPoint = "you first";
        return true;
    }
    bool getOptimalPointToReachBall(bool ballGoesInside, out Vector3 result,out float timeResult)
    {
        Vector3 optimalPoint = myChaserData.getOptimalPointWithOffset();
        bool resultIsValid = myChaserData.ReachTheTarget;
        timeResult = 0;
        result = Vector3.zero;
        goToOptimalPoint = false;
        if (resultIsValid)
        {
            //El portero será capaz de alcanzar algún punto en la futura trayectoria del balón
            result = optimalPoint;
            timeResult = myChaserData.OptimalTime;
            result = ClampGoalKeeper(result);
            
            float timeToGetBallOptimalPoint = myChaserData.OptimalTargetTime;
            float timeToGetGoalkeeperOptimalPoint = myChaserData.OptimalTime;
            float distanceOptimalPointOfLine = MyFunctions.DistancePointAndFiniteLine(optimalPoint, MyFunctions.setYToVector3WithClamp(GoalComponents.right.position, optimalPoint.y, 0, goalkeeperValues.maxHeightInArea), MyFunctions.setYToVector3WithClamp(GoalComponents.left.position, optimalPoint.y, 0, goalkeeperValues.maxHeightInArea));
            //print(ballGoesInside + " " + (timeToGetGoalkeeperOptimalPoint - timeToGetBallOptimalPoint));
            if (ballGoesInside && timeToGetGoalkeeperOptimalPoint < 0.5f || false&&distanceOptimalPointOfLine < 4)
            {
                currentOptimalPoint = "optimalPoint 1";
                return true;
            }
            else
            {
                currentOptimalPoint = "optimalPoint 2";
                return false;
            }
        }
        currentOptimalPoint = "No optimalPoint";
        return false;
    }
    bool getClosestPoint(bool ballGoesInside,out Vector3 resultPoint,out float timeResult)
    {
        //El portero NO será capaz de alcanzar algún punto en la futura trayectoria del balón delante de la portería
        bool resultBool = false;
        resultPoint = Vector3.positiveInfinity;
        timeResult = Mathf.Infinity;
        if (myChaserData.thereIsClosestPoint)
        {
            Vector3 closestPoint = ClampGoalKeeper(myChaserData.ClosestPoint);
            Vector3 closestPointInArea;
            Vector3 direction = closestPoint - myChaserData.position;
            Ray ray = new Ray(myChaserData.position, direction);
            if (MatchComponents.footballField.fullFieldArea.GetPoint(closestPoint, ray, out closestPointInArea))
            {
                closestPoint = closestPointInArea;
                float distanceGoalkeeperClosestPoint = Vector3.Distance(closestPoint, myChaserData.position);
                float timeGoalkeeper = distanceGoalkeeperClosestPoint / goalkeeperValues.maxSpeed;
                float distanceClosestPointOfLine = MyFunctions.DistancePointAndFiniteLine(closestPointInArea, MyFunctions.setYToVector3WithClamp(GoalComponents.right.position, closestPointInArea.y, 0, goalkeeperValues.maxHeightInArea), MyFunctions.setYToVector3WithClamp(GoalComponents.left.position, closestPointInArea.y, 0, goalkeeperValues.maxHeightInArea));
                if (ballGoesInside && myChaserData.differenceClosestTime < 0.2f && myChaserData.differenceClosestTime > -0.2f || false&&distanceClosestPointOfLine < 4)
                {
                    currentClosestPoint = "True A";
                    resultPoint = closestPointInArea;
                    resultPoint = ClampGoalKeeper(resultPoint);
                    timeResult = myChaserData.ClosestChaserTime;
                    return true;
                }
                if (ballGoesInside)
                {
                    //El balón va hacia la portería
                    float angle = Vector3.Angle(ComponentsBall.rigBall.velocity, myChaserData.position - ComponentsBall.rigBall.position);
                    if (angle < 70)
                    {
                        float timeBall = myChaserData.ClosestTargetTime;
                        float difference = timeGoalkeeper - timeBall;
                        if (difference < 0.3f)
                        {
                            //El portero alcanza el closestPoint antes de que pasen 0.3 segundos desde que lo alcanzo el balón.
                            resultPoint = closestPoint;
                            resultPoint = ClampGoalKeeper(resultPoint);
                            //DisableBehaviourForAWhile(goalkeeperValues.timeJumping);
                            timeResult = timeGoalkeeper;
                            currentClosestPoint = "True B";
                            return false;
                        }
                        else if (true)
                        {
                            resultPoint = closestPoint;
                            resultPoint = ClampGoalKeeper(resultPoint);
                            timeResult = myChaserData.ClosestTargetTime;
                            currentClosestPoint = "False";
                            return false;
                        }
                    }
                }
            }
            else
            {
                resultPoint = closestPointInArea;
            }
        }
        currentClosestPoint = "False";
        return resultBool;
    }
    Vector3 getGoalkeeperOptimalPosition(Vector3 centerPosition, Vector3 dirCenterPosition)
    {
        Vector3 result;
        float ballGoalkeeperDistance = Vector3.Distance(myChaserData.position, ComponentsBall.rigBall.position);

        if (ballGoalkeeperDistance < goalkeeperValues.minDistanceFollowBall)
        {
            result = ComponentsBall.rigBall.position;
        }
        else
        {
            float ballDistance = Vector3.Distance(centerPosition, ComponentsBall.rigBall.position);
            float maxDistance = ballDistance;
            Vector3 optimalPoint=ComponentsBall.rigBall.position;
            ChaserData firstChaserData;
            if (getFirstChaserData(out firstChaserData)&&!firstChaserData.getGlobalOptimalPoint(out optimalPoint))
            {
                optimalPoint = ComponentsBall.rigBall.position;
            }
            Vector3 closestPointOfLine = MyFunctions.GetClosestPointOnFiniteLine(MyFunctions.setYToVector3(optimalPoint, 0), MyFunctions.setYToVector3(GoalComponents.right.position, 0), MyFunctions.setYToVector3(GoalComponents.left.position, 0));
            float distanceClosestPointAndBall = Vector3.Distance(closestPointOfLine, MyFunctions.setYToVector3(optimalPoint, 0));

            float minDistanceBetweenGoalkeeperBall = Mathf.Lerp(0, goalkeeperValues.minDistanceToBallWhitOwner, (distanceClosestPointAndBall - goalkeeperValues.adjustDistanceBallGoalkeeper1) / goalkeeperValues.adjustDistanceBallGoalkeeper2);
            //print(distanceClosestPointAndBall+" | " + (distanceClosestPointAndBall - a) / 5);
            foreach (var enemyData in enemyPlayerDataList)
            {
                Vector3 dirEnemyGoal = GoalComponents.centerOptimalPosition.position - enemyData.bodyTransform.position;
                ChaserData enemyChaserData;
                if(!enemyData.getFirstChaserData(out enemyChaserData))
                {
                    continue;
                }
                Vector3 enemyOptimalPoint;
                if(goalkeeperValues.useRivalsAdvandedPosition && enemyChaserData.getGlobalOptimalPoint(out enemyOptimalPoint))
                {
                    float timeToGetTheBallToTheEnemy = Vector3.Distance(enemyOptimalPoint, enemyData.bodyTransform.position) / goalkeeperValues.optimalPositionPassVelocity;
                    OptimalPosition optimalPosition = new OptimalPosition(-dirCenterPosition.normalized, dirEnemyGoal.normalized, centerPosition, enemyData.bodyTransform.position, timeToGetTheBallToTheEnemy, goalkeeperValues.maxSpeed, goalkeeperValues.optimalPositionSpeedKick, 0, maxDistance);
                    maxDistance = optimalPosition.calculate();
                    //print(maxDistance);
                }
                //print(maxDistance);
                //print((ballDistance - goalkeeperValues.minDistanceToBallWhitOwner) + " | " + maxDistance + " | "+ timeToGetTheBallToTheEnemy);

            }

            float distance1 = Mathf.Clamp(ballDistance - minDistanceBetweenGoalkeeperBall, 0, maxDistance);
            float angleGoalForward_Ball = Vector3.Angle(GoalComponents.centerOptimalPosition.forward, optimalPoint - GoalComponents.centerOptimalPosition.position);
            float distanceAngle = Mathf.Lerp(distance1, 0, goalkeeperValues.optimalPositionEscoradoBallInfluence.Evaluate(angleGoalForward_Ball / 90));
            float farDistance = goalkeeperValues.adjustBallIsFarCurve.Evaluate((distanceClosestPointAndBall - goalkeeperValues.adjustBallIsFar1) / goalkeeperValues.adjustBallIsFar2);
            
            float farAngleDistance = Mathf.Lerp(distanceAngle, 0, farDistance);

            Vector3 closestGoalkeeperCenterLinePoint = MyFunctions.GetClosestPointOnFiniteLine(goalKeeperTransform.position, centerPosition, optimalPoint);
            float distanceClosestPointGoalkeeper = Vector3.Distance(goalKeeperTransform.position, closestGoalkeeperCenterLinePoint);
            Vector3 point1 = centerPosition - dirCenterPosition.normalized * farAngleDistance;
            //print((farAngleDistance) + " | "+maxDistance);
            result = Vector3.Lerp(point1, closestGoalkeeperCenterLinePoint, distanceClosestPointGoalkeeper / 2);
            result = cornerAdjust(result);
            Area.GetPoint(result, new Ray(centerPosition, -dirCenterPosition), out result);
        }

        //Debug.DrawLine(componentsBall.rigBall.position, result);
        result = ClampGoalKeeper(result);
        return result;
    }
    void theMatchIsOnState()
    {
        

        if (checkBallDirection())
        {
            return;
        }
        Vector3 dirCenterPosition = calculateDirCenterPosition();
        
        _ballGoesInside = ballGoesInside(out ballGoesInsideClosestPoint);
        float optimalPointTime;
        
        
        if (ballIsInMyRight())
        {
            intersectionCenter = getCenterPosition(GoalComponents.right.position);
        }
        else
        {
            intersectionCenter = getCenterPosition(GoalComponents.left.position);
        }
        targetDirLook = getDirLookBallPosition();
        advancedGoalkeeperCenterPosition = getGoalkeeperOptimalPosition(intersectionCenter, dirCenterPosition);
        
        goToOptimalPoint = getOptimalPointToReachBall(_ballGoesInside, out optimalPointReachBall, out optimalPointTime);

        bool resultClosestPoint = getClosestPoint(_ballGoesInside, out closestPoint, out closestPointTime);
        bool resultCheckIfGoalkeeperArrivesBeforeOthers = checkIfGoalkeeperArrivesBeforeOthers();
        if (goToOptimalPoint && resultCheckIfGoalkeeperArrivesBeforeOthers)
        {
            targetPosition = optimalPointReachBall;
            currentState = "optimalPoint";
        }
        else
        {
            
            if (myChaserData.ReachTheTarget && resultCheckIfGoalkeeperArrivesBeforeOthers)
            {
                targetPosition = optimalPointReachBall;
                currentState = "optimalPoint ArrivesBeforeOthers";
            }
            else
            {
                if (resultClosestPoint && resultCheckIfGoalkeeperArrivesBeforeOthers)
                {
                    targetPosition = closestPoint;
                    currentState = "closestPoint";
                }
                else
                {
                    if (false&&resultCheckIfGoalkeeperArrivesBeforeOthers)
                    {
                        targetPosition = closestPoint;
                        currentState = "closestPoint2";
                    }
                    else
                    {
                        targetPosition = advancedGoalkeeperCenterPosition;
                        currentState = "advancedPosition1";
                    }
                }
            }
        }
        if(enableRandomPass)
                checkPass();
    }
    void checkPass()
    {
        Vector3 heightPos = MyFunctions.setYToVector3(goalKeeperTransform.position, goalkeeperValues.height + goalKeeperTransform.position.y);
        float distance = MyFunctions.DistancePointAndFiniteLine(ComponentsBall.transCenterBall.position, goalkeeperBaseTrans.position, heightPos);
        if(distance<goalkeeperValues.passScope && ComponentsBall.rigBall.velocity.magnitude < 10)
        {
            pass.randomPass(sideOfField.goalComponents.forward.forward,myPublicPlayerData.playerID);
            //pass.pass(enemyPlayerDataList,partnetPlayerDataList,myPublicPlayerData);
        }
    }
    bool ballGoesInside(out Vector3 closestPoint)
    {
        closestPoint = Vector3.zero;
        //print(myChaserData.thereIsIntercession);
        //print("thereIsIntercession="+ myChaserData.thereIsIntercession);
        if (myChaserData.thereIsIntercession)
        {
            bool isForward;
            Vector3 intercession = myChaserData.Intercession;
            //print("a "+BallGoesInsidePlane.GetPoint(intercession, out closestPoint, out isForward));
            return BallGoesInsidePlane.GetPoint(intercession, out closestPoint, out isForward);
        }
        else
        {
            return false;
        }

    }
    Vector3 getCenterPosition(Vector3 pointPlano)
    {
        Vector3 intercessionBallGoal;
        float aux = GoalComponents.centerOptimalPosition.InverseTransformPoint(pointPlano).x;
        float angle;
        if (aux > 0)
        {
            angle = goalkeeperValues.angleCenterPositionAdjust;
        }
        else
        {
            angle = -goalkeeperValues.angleCenterPositionAdjust;
        }
        //Vector3 centerLine = ComponentsBall.transBall.position - GoalComponents.centerOptimalPosition.position;
        ChaserData firstChaserData;
        Vector3 centerLine;
        Vector3 optimalPoint;
        if (getFirstChaserData(out firstChaserData) && firstChaserData.getGlobalOptimalPoint(out optimalPoint))
        {
            //Vector3 centerLine = ComponentsBall.transBall.position - GoalComponents.centerOptimalPosition.position;
            
            centerLine = optimalPoint - GoalComponents.centerOptimalPosition.position;
        }
        else
        {
            centerLine = ComponentsBall.transBall.position - GoalComponents.centerOptimalPosition.position;
        }
        centerLine = Quaternion.AngleAxis(angle, Vector3.up) * centerLine;
        intercessionBallGoal = MyFunctions.GetClosestPointOnFiniteLine(pointPlano, GoalComponents.centerOptimalPosition.position, GoalComponents.centerOptimalPosition.position + centerLine);
        Plane plane = new Plane(GoalComponents.centerOptimalPosition.forward,GoalComponents.centerMatchStoppedState.position);
        if (!plane.GetSide(intercessionBallGoal))
        {
            intercessionBallGoal = plane.ClosestPointOnPlane(intercessionBallGoal);
        }
        return ClampGoalKeeper(intercessionBallGoal);
    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (drawGizmos && Application.isPlaying && enabled)
        {
            Gizmos.color = Color.blue;
            Vector3 point = optimalPointReachBall + Vector3.up * 0.5f;
            GUIStyle style = new GUIStyle();
            style.fontSize = 12;
            style.normal.textColor = Color.blue;

            Handles.Label(point, name + " optimalPointReachBall",style);
            

            Gizmos.DrawSphere(optimalPointReachBall, 0.05f);
            DrawArrow.ForGizmo(point, Vector3.down*0.5f, Color.blue, 0.25f);

            point = advancedGoalkeeperCenterPosition + Vector3.up * 0.3f;
            Handles.Label(point, name + " advancedGoalkeeperCenterPosition", style);
            Gizmos.DrawSphere(advancedGoalkeeperCenterPosition, 0.05f);
            DrawArrow.ForGizmo(point, Vector3.down * 0.3f, Color.blue, 0.25f);

            Gizmos.color = Color.green;

            style.normal.textColor = Color.green;
                Vector3 point2 = targetPosition + Vector3.up * 0.5f+ Vector3.forward * 0.3f;
                Handles.Label(point2, name + " targetPosition", style);
                DrawArrow.ForGizmo(point2, targetPosition- point2, Color.green, 0.25f);
                //Gizmos.DrawSphere(targetPosition, 0.1f);
            style.normal.textColor = Color.yellow;
            style.fontSize = 10;

            if (changeDirection)
            {
            Handles.Label(goalKeeperTransform.position, "OptimalPoint=" + currentOptimalPoint + " | ClosestPoint=" + currentClosestPoint + " | State=" + currentState + " | ArrivesOptimal=" + arriveOptimalPoint + " | ArrivesClosest=" + arriveClosestPoint + " | changeDirection", style);
            }
            else
            {
               Handles.Label(goalKeeperTransform.position, "OptimalPoint=" + currentOptimalPoint + " | ClosestPoint=" + currentClosestPoint + " | State=" + currentState + " | ArrivesOptimal=" + arriveOptimalPoint + " | ArrivesClosest=" + arriveClosestPoint, style);
            } 
            style.fontSize = 12;
                //Handles.Label(goalKeeperTransform.position - Vector3.up * 0.2f, "currentVelocity=" + myPublicPlayerData.velocity.ToString("f2"), style);
            style.normal.textColor = Color.white;
                Handles.Label(ballGoesInsideClosestPoint + Vector3.up*0.7f, "ballGoesInside="+_ballGoesInside, style);
            Gizmos.color = Color.white;
            DrawArrow.ForGizmo(ballGoesInsideClosestPoint + Vector3.up*0.7f, Vector3.down * 0.5f, Color.white, 0.25f);
            Gizmos.DrawSphere(ballGoesInsideClosestPoint, 0.1f);
            drawIntercession();
        }

    }
#endif
    void drawIntercession()
    {
        Color color = new Color(0.7f, 0.1f, 0.0f,1);
        GUIStyle style = new GUIStyle();
        style.fontSize = 12;
        style.normal.textColor = color;

#if UNITY_EDITOR
         Handles.Label(intersectionCenter + Vector3.up * 0.5f, "intersectionCenter", style);
#endif
        Gizmos.color = color;
        DrawArrow.ForGizmo(intersectionCenter + Vector3.up * 0.5f, Vector3.down * 0.5f, color, 0.25f);
        Gizmos.DrawSphere(intersectionCenter, 0.05f);
    }
    void theMatchIsStoppedState()
    {
        targetPosition = GoalComponents.centerMatchStoppedState.position;
        currentOptimalPoint = "GoalCenterPosition";
        currentState = "theMatchIsStoppedState";
        //targetDirLook = getDirLookCenterPosition(dirCenterPosition);
        targetDirLook = getDirLookBallPosition();
        
    }
    Vector3 calculateDirCenterPosition()
    {
        ChaserData firstChaserData;
        Vector3 optimalPoint; 
        if (getFirstChaserData(out firstChaserData) && firstChaserData.getGlobalOptimalPoint(out optimalPoint))
        {
            return GoalComponents.centerOptimalPosition.position - optimalPoint;
        }
        else
        {
            return GoalComponents.centerOptimalPosition.position - ComponentsBall.rigBall.position;
        }
    }
    IEnumerator calculateTargetsPoints()
    {
        //Necesario que haya una coroutina que calcule el angulo de cambio de direción del balón. El resto también tiene que estar en esta coroutina (y no en update()) para que si cambia el angulo no se cambie el targetPosition
        while (true)
        {
            myBehaviour.Execute();
         //no tocar 0.1 en WaitForSeconds
         yield return new WaitForSeconds(0);
         //yield return null;
        }
    }
    protected virtual void goTo(float deltaTime)
    {
        //goalKeeperTransform.position = Vector3.Lerp(goalKeeperTransform.position, targetPosition, deltaTime * speedTranslation);
        //Para que el portero se mueva a 5 m/s constantemente, no asi con la funcion Vector3.Lerp
        float distance = Vector3.Distance(goalKeeperTransform.position,targetPosition);
        Vector3 dir = targetPosition - goalKeeperTransform.position;
        //Para que no tiemble la mano cuando esta muy cerca de targetPosition hay que utilizar Mathf.Lerp en la velocidad
        float speed = Mathf.Lerp(0, goalkeeperValues.maxSpeed, distance/ goalkeeperValues.distanceLerpCloseTarget);
        // print(goalkeeperValues.maxSpeed);
        //float speed = goalkeeperValues.maxSpeed;
        if (!MyFunctions.Vector3IsNan(dir))
        {
            float deltaSpeed = deltaTime * speed;
            //myPublicPlayerData.velocity = speed;
            
            goalKeeperTransform.position += dir.normalized * deltaSpeed;
            myPublicPlayerData.velocity = dir.normalized * speed;

        }
        if(targetDirLook != Vector3.zero)
        {
            goalKeeperTransform.rotation = Quaternion.Lerp(goalKeeperTransform.rotation, Quaternion.LookRotation(targetDirLook), deltaTime * goalkeeperValues.speedRotation);
        }
        //Debug.DrawLine(componentsBall.rigBall.position, targetPosition, Color.red);
        //Debug.DrawLine(goalKeeperTransform.position, targetPosition, Color.blue);
    }
    bool checkBallDirection()
    {
        bool result = false;
        float angle = Vector3.Angle(MyFunctions.setYToVector3(lastBallDirection,0), MyFunctions.setYToVector3(ComponentsBall.rigBall.velocity,0));
        if (angle > 10 || (lastBallDirection == Vector3.zero && ComponentsBall.rigBall.velocity.magnitude > 0.1f))
        {
            if (myBehaviour.enabled)
            {
                //Para evitar cortar el estado jump()
                changeDirection = true;
                float reflexes = Random.Range(goalkeeperValues.minRandomReflexes, goalkeeperValues.maxRandomReflexes);
                DisableBehaviourForAWhile(reflexes);
                result = true;
            }
        }
        lastBallDirection = ComponentsBall.rigBall.velocity;
        return result;
    }
    Vector3 cornerAdjust(Vector3 input)
    {
        //Si el balón está escorado a la banda el portero no necesita estar muy adelantado
        float angle = Vector3.Angle(GoalComponents.centerMatchStoppedState.forward, ComponentsBall.transBall.position - GoalComponents.centerMatchStoppedState.position);
        float angleLerp = (angle - goalkeeperValues.cornerAngleAdjust1) / (goalkeeperValues.cornerAngleAdjust2 - goalkeeperValues.cornerAngleAdjust1);
        Vector3 angleAdjust = Vector3.Lerp(input, GoalComponents.centerMatchStoppedState.position, angleLerp);
        float distance = Vector3.Distance(GoalComponents.centerMatchStoppedState.position, ComponentsBall.transBall.position);
        float distanceLerp = (distance - goalkeeperValues.cornerDistanceAdjust1) / (goalkeeperValues.cornerDistanceAdjust2 - goalkeeperValues.cornerDistanceAdjust1);
        return Vector3.Lerp(input, angleAdjust, distanceLerp);

    }
    public void DisableBehaviourForAWhile(float period)
    {
        //goalKeeperIslied = true;
        myBehaviour.Disable();
        //Invoke(nameof(disactivateGoalKeeperIsLied), goalkeeperValues.timeJumping);

        Invoke(nameof(EnableBehaviour), period);
    }
    void EnableBehaviour()
    {
        changeDirection = false;
        //goalKeeperIslied = false;
        myBehaviour.Enable();
    }
    public void playerAddedToTeam(PlayerAddedToTeamEventArgs args)
    {

        if (!args.PlayerID.Equals(myPublicPlayerData.playerIDMono.getStringID()))
        {
            PublicPlayerData publicPlayerData = PublicPlayerDataList.all[args.PlayerID];

            if (enemyPlayerDataList.Contains(publicPlayerData))
            {
                enemyPlayerDataList.Remove(publicPlayerData);
                ChaserData chaserData;
                publicPlayerData.getFirstChaserData(out chaserData);
                arriveBeforeOthersEnemyPlayerDataList.Add(chaserData);
            }
            if (partnetPlayerDataList.Contains(publicPlayerData))
            {
                partnetPlayerDataList.Remove(publicPlayerData);
            }
            Team teamGoalKeeper;
            if (Teams.getTeamFromPlayer(myPublicPlayerData.playerIDMono.getStringID(), out teamGoalKeeper))
            {
                if (teamGoalKeeper.TeamName.Equals(args.TeamName))
                {
                    //print("add partnet "+publicPlayerData.name);
                    partnetPlayerDataList.Add(publicPlayerData);
                }
                else
                {
                    //print("add enemy " + publicPlayerData.name);
                    enemyPlayerDataList.Add(publicPlayerData);
                    ChaserData chaserData;
                    publicPlayerData.getFirstChaserData(out chaserData);
                    arriveBeforeOthersEnemyPlayerDataList.Add(chaserData);
                }
            }
        }
        else
        {
            arriveBeforeOthersEnemyPlayerDataList.Add(myChaserData);
        }
    }

    void removePlayer(string playerID)
    {
        enemyPlayerDataList.RemoveAll(item => item == null);
        partnetPlayerDataList.RemoveAll(item => item == null);
        PublicPlayerData publicPlayerData = enemyPlayerDataList.Find(item => item.playerIDMono.getStringID().Equals(playerID));
        if (publicPlayerData != null)
        {
            enemyPlayerDataList.Remove(publicPlayerData);
        }
        publicPlayerData = partnetPlayerDataList.Find(item => item.playerIDMono.getStringID().Equals(playerID));
        if (publicPlayerData != null)
        {
            partnetPlayerDataList.Remove(publicPlayerData);
        }
    }
    void printPartnetEnemys()
    {
        print("GoalkeeperName=" + name);
        print("Enemies");
        foreach (var enemyPublicPlayerData in enemyPlayerDataList)
        {
            print("Name=" + enemyPublicPlayerData.name+" | chaserData="+enemyPublicPlayerData.ChaserDataList.ToString());
        }
        print("Partnets");
        foreach (var partnetPublicPlayerData in partnetPlayerDataList)
        {
            print("Name" + partnetPublicPlayerData.name + " | chaserData=" + partnetPublicPlayerData.ChaserDataList.ToString());
        }
    }
    Vector3 ClampGoalKeeper(Vector3 value)
    {
        //Para que el guante no atraviese el suelo
        float targetPositionY = Mathf.Clamp(value.y, minPositionY, Mathf.Infinity);
        return new Vector3(value.x, targetPositionY, value.z);
    }
#region Miscellaneous

    
    Vector3 getDirLookBallPosition()
    {
        //Para que no rote en el eje X
        Vector3 dirLook = ComponentsBall.transBall.position - putBallY(goalKeeperTransform.position);
        return dirLook;
    }
    bool pointIsInFront(Vector3 vector1, Vector3 vector2)
    {
        float angle = Vector3.Cross(vector1, vector2).y;
        //float angle = FindAngle(-center.right,vector1-center.position);
        if (angle >= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    bool ballIsInMyRight()
    {
        return Vector3.Cross(GoalComponents.centerOptimalPosition.forward, ComponentsBall.transBall.position - GoalComponents.centerOptimalPosition.position).y >=0;
    }
    //Para que los vectores no tengan rotacion en el eje X
    Vector3 putBallY(Vector3 value)
    {
        return new Vector3(value.x, ComponentsBall.transBall.position.y, value.z);
    }
    private void OnEnable()
    {
        coroutine = StartCoroutine(calculateTargetsPoints());
    }
    private void OnDisable()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }
    public void setGoTo()
    {
        positionFunction = goTo;
    }
#endregion
    
    /*
    bool getClosestPoint(bool ballGoesInside, out Vector3 resultPoint, out float timeResult)
    {
        //El portero NO será capaz de alcanzar algún punto en la futura trayectoria del balón delante de la portería
        bool resultBool = false;
        if (myChaserData.thereIsClosestPoint)
        {
            Vector3 closestPoint = ClampGoalKeeper(myChaserData.ClosestPoint);
            Vector3 closestPointInArea;
            Vector3 direction = closestPoint - myChaserData.position;
            Ray ray = new Ray(myChaserData.position, direction);
            if (MatchComponents.footballField.fullFieldArea.GetPoint(closestPoint, ray, out closestPointInArea))
            {
                if (ballGoesInside)
                {
                    //El balón va hacia la portería
                    float angle = Vector3.Angle(ComponentsBall.rigBall.velocity, myChaserData.position - ComponentsBall.rigBall.position);
                    if ((angle < 70 || false))
                    {
                        float timeBall = myChaserData.ClosestTargetTime;
                        float distanceGoalkeeperClosestPoint = Vector3.Distance(closestPoint, myChaserData.position);
                        float timeGoalkeeper = distanceGoalkeeperClosestPoint / goalkeeperValues.maxSpeed;
                        //print(timeGoalkeeper - timeBall);
                        if (timeGoalkeeper - timeBall < 0.3f)
                        {
                            //El portero alcanza el closestPoint antes de que pasen 0.3 segundos desde que lo alcanzo el balón.
                            resultPoint = closestPoint;
                            resultPoint = ClampGoalKeeper(resultPoint);
                            DisableBehaviourForAWhile(goalkeeperValues.timeJumping);
                            timeResult = myChaserData.ClosestTargetTime;
                            currentTargetPoint = "closestPoint";
                        }
                        else if (Vector3.Distance(goalKeeperTransform.position, GoalComponents.centerMatchStoppedState.position) < 4)
                        {
                            //El portero está cerca de la portería
                            DisableBehaviourForAWhile(0.25f);
                            resultPoint = goalKeeperTransform.position;
                            resultPoint = ClampGoalKeeper(resultPoint);
                            currentTargetPoint = "goalKeeper.position";
                            timeResult = 0;
                        }
                        else
                        {
                            resultPoint = closestPoint;
                            resultPoint = ClampGoalKeeper(resultPoint);
                            timeResult = myChaserData.ClosestTargetTime;
                            currentTargetPoint = "closestPoint";
                        }
                        resultBool = true;
                    }
                    else
                    {
                        //el portero esta detras del balón respecto a la portería
                        if (myChaserData.thereIsIntercession)
                        {
                            resultPoint = myChaserData.Intercession;
                        }
                        else
                        {
                            resultPoint = ballGoesInsideClosestPoint;
                        }
                        resultPoint = ClampGoalKeeper(resultPoint);
                        currentTargetPoint = "Intercession";
                        timeResult = Vector3.Distance(myChaserData.Intercession, myChaserData.position) / goalkeeperValues.maxSpeed;
                        resultBool = false;
                    }

                }
                else
                {
                    //print("b");
                    resultPoint = GoalComponents.centerMatchStoppedState.position;
                    resultPoint = ClampGoalKeeper(resultPoint);
                    currentTargetPoint = "closestPointInsideFullArea - No ballGoesInside";
                    timeResult = Vector3.Distance(GoalComponents.centerMatchStoppedState.position, myChaserData.position) / goalkeeperValues.maxSpeed;
                    resultBool = false;
                }
            }
            else
            {
                //print("c");
                if (ballGoesInside)
                {
                    currentTargetPoint = "closestPointOutsideFullArea - ballGoesInside";
                    resultPoint = closestPointInArea;
                    resultPoint = ClampGoalKeeper(resultPoint);
                    timeResult = Vector3.Distance(closestPointInArea, myChaserData.position) / goalkeeperValues.maxSpeed;
                    resultBool = true;
                }
                else
                {
                    currentTargetPoint = "closestPointOutsideFullArea - No ballGoesInside";
                    resultPoint = GoalComponents.centerMatchStoppedState.position;
                    resultPoint = ClampGoalKeeper(resultPoint);
                    timeResult = Vector3.Distance(GoalComponents.centerMatchStoppedState.position, myChaserData.position) / goalkeeperValues.maxSpeed;
                    resultBool = false;
                }
            }
        }
        else
        {
            if (ballGoesInside)
            {
                resultPoint = myChaserData.Intercession;
                resultPoint = ClampGoalKeeper(resultPoint);
                currentTargetPoint = "NO thereIsClosestPoint-ballGoesInside-Intercession";
                timeResult = Vector3.Distance(myChaserData.Intercession, myChaserData.position) / goalkeeperValues.maxSpeed;
                resultBool = true;
            }
            else
            {
                resultPoint = GoalComponents.centerMatchStoppedState.position;
                resultPoint = ClampGoalKeeper(resultPoint);
                currentTargetPoint = "NO thereIsClosestPoint-NO ballGoesInside";
                timeResult = Vector3.Distance(GoalComponents.centerMatchStoppedState.position, myChaserData.position) / goalkeeperValues.maxSpeed;
                resultBool = false;
            }
        }
        return resultBool;
    }*/

    /*bool checkIfGoalkeeperArrivesBeforeOthers(float optimalPointTime,List<float> enemyTimeList, List<float> partnerTimeList)
    {
        float enemyMinTimeToGetOptimalPoint = Mathf.Infinity;
        foreach (var enemyTime in enemyTimeList)
        {
            if (enemyTime < optimalPointTime)
            {
                enemyMinTimeToGetOptimalPoint = enemyTime;
                return false;
            }else if (enemyTime < enemyMinTimeToGetOptimalPoint)
            {
                enemyMinTimeToGetOptimalPoint = enemyTime;
            }
        }
        foreach (var partnerTime in partnerTimeList)
        {
            if (partnerTime < enemyMinTimeToGetOptimalPoint)
            {
                return false;
            }
        }
        return true;
    }*/
}
public class Recta
{
    public float x0, y0, z0;
    public float tx, ty, tz;
    public Recta(Vector3 point, Vector3 dir)
    {
        x0 = point.x;
        y0 = point.y;
        z0 = point.z;
        tx = dir.x;
        ty = dir.y;
        tz = dir.z;
    }

}
public class Plano
{
    float A, B, C, D;

    public Plano(Vector3 point, Vector3 dir1, Vector3 dir2)
    {
        float u1, u2, u3;
        float v1, v2, v3;
        float x0, y0, z0;
        x0 = point.x;
        y0 = point.y;
        z0 = point.z;
        u1 = dir1.x;
        u2 = dir1.y;
        u3 = dir1.z;
        v1 = dir2.x;
        v2 = dir2.y;
        v3 = dir2.z;
        A = u2 * v3 - u3 * v2;
        B = -(u1 * v3 - u3 * v1);
        C = u1 * v2 + u2 * v1;
        D = -A * x0 - B * y0 - C * z0;
    }
    public Vector3 findInterseccion(Recta recta)
    {
        Vector3 interseccion;
        float t;
        t = A * recta.tx + B * recta.ty + C * recta.tz;
        t = -(A * recta.x0 + B * recta.y0 + C * recta.z0 + D) / t;
        interseccion = new Vector3(recta.x0 + recta.tx * t, recta.y0 + recta.ty * t, recta.z0 + recta.tz * t);
        return interseccion;
    }
}
/*
protected virtual void goTo(float deltaTime)
{
    Vector3 targetPosition;
    Vector3 targetDirLook;
    Vector3 dirCenterPosition = getDirCenterPosition(pointIsInFront(-center.right, componentsBall.transBall.position - center.position));

    Vector3 pointPrueba = GetClosestPointOnFiniteLine(componentsBall.transBall.position, putBallY(right.position), putBallY(left.position));
    //Debug.DrawLine(componentsBall.transBall.position, pointPrueba, Color.blue);
    if (ballIsInside)
    {
        targetPosition = getFollowBall();
        targetDirLook = getDirLookBallPosition();
    }
    else if (pointIsInFront(-center.right, componentsBall.transBall.position - center.position))
    {
#region getPoints

        Vector3 followBallPoint = Vector3.zero;
        bool interseccionVelocityIsValid = false;
        Vector3 interseccionCenter;
        Vector3 interseccionVelocity;

        Vector3 horizontalPlaneDir = getHorizontalPlaneDir(dirCenterPosition);
        Vector3 optimalPoint;
        bool optimalPointIsValid = getOptimalIntersection(goalKeeperTransform.position, porteroValues.speed, componentsBall.rigBall.position, componentsBall.rigBall.velocity, out optimalPoint);

        if (ballIsInMyRight())
        {
            interseccionCenter = getIntercessionCenterBallGoal(right, dirCenterPosition, horizontalPlaneDir);
            interseccionVelocity = getIntercessionVelocityBallGoal(right, componentsBall.rigBall.velocity.normalized, horizontalPlaneDir, ref interseccionVelocityIsValid);

        }
        else
        {
            interseccionCenter = getIntercessionCenterBallGoal(left, dirCenterPosition, horizontalPlaneDir);
            interseccionVelocity = getIntercessionVelocityBallGoal(left, componentsBall.rigBall.velocity.normalized, horizontalPlaneDir, ref interseccionVelocityIsValid);
        }
        followBallPoint = getFollowBall();
#endregion

        targetDirLook = getDirLookCenterPosition(dirCenterPosition);
#region getTargetPosition
        //Cuando el balón está quieto no hay intersección con el plano y da un Vector3 NaN
        if (Vector3HaveNaN(interseccionVelocity))
        {
            targetPosition = getLerp(interseccionCenter, followBallPoint);
        }
        else
        {
            if (ballGoesInside())
            {
                //La intersección esta en la dirección del balón?
                if (!interseccionVelocityIsValid)
                {
                    targetPosition = getLerp(interseccionCenter, followBallPoint);
                }
                else
                {
                    targetPosition = getLerpWithVelocity(interseccionVelocity, interseccionCenter, followBallPoint);
                }

            }
            else
            {
                targetPosition = getLerp(interseccionCenter, followBallPoint);
            }
        }
#endregion
    }
    else
    {
        targetPosition = goalKeeperTransform.position;
        targetDirLook = getDirLookCenterPosition(dirCenterPosition);
    }

    float speedTranslation = getSpeed();
    goalKeeperTransform.position = Vector3.Lerp(goalKeeperTransform.position, targetPosition, deltaTime * speedTranslation);
    goalKeeperTransform.rotation = Quaternion.Lerp(goalKeeperTransform.rotation, Quaternion.LookRotation(targetDirLook), deltaTime * porteroValues.speedRotation);
}*/
/*
Vector3 getGoalkeeperOptimalPosition(Vector3 centerPosition, Vector3 dirCenterPosition)
{
    Vector3 result;
    float ballGoalkeeperDistance = Vector3.Distance(myChaserData.position, ComponentsBall.rigBall.position);

    if (ballGoalkeeperDistance < goalkeeperValues.minDistanceFollowBall)
    {
        result = ComponentsBall.rigBall.position;
    }
    else
    {
        float ballDistance = Vector3.Distance(centerPosition, ComponentsBall.rigBall.position);
        float maxDistance = ballDistance;
        Vector3 closestPointOfLine = MyFunctions.GetClosestPointOnFiniteLine(MyFunctions.setYToVector3(ComponentsBall.rigBall.position, 0), MyFunctions.setYToVector3(GoalComponents.right.position, 0), MyFunctions.setYToVector3(GoalComponents.left.position, 0));
        float distanceClosestPointAndBall = Vector3.Distance(closestPointOfLine, MyFunctions.setYToVector3(ComponentsBall.rigBall.position, 0));

        float minDistanceBetweenGoalkeeperBall = Mathf.Lerp(0, goalkeeperValues.minDistanceToBallWhitOwner, (distanceClosestPointAndBall - goalkeeperValues.adjustDistanceBallGoalkeeper1) / goalkeeperValues.adjustDistanceBallGoalkeeper2);
        //print(distanceClosestPointAndBall+" | " + (distanceClosestPointAndBall - a) / 5);
        foreach (var enemyData in enemyPlayerDataList)
        {

            //Vector3 dirEnemyGoal = MyFunctions.setYToVector3(goalComponents.center.position - enemyData.bodyTransform.position, 0);
            Vector3 dirEnemyGoal = GoalComponents.centerOptimalPosition.position - enemyData.bodyTransform.position;
            float timeToGetTheBallToTheEnemy = Vector3.Distance(ComponentsBall.rigBall.position, enemyData.bodyTransform.position) / goalkeeperValues.optimalPositionPassVelocity;
            OptimalPosition optimalPosition = new OptimalPosition(-dirCenterPosition.normalized, dirEnemyGoal.normalized, centerPosition, enemyData.bodyTransform.position, timeToGetTheBallToTheEnemy, goalkeeperValues.maxSpeed, goalkeeperValues.optimalPositionSpeedKick, 0, maxDistance);
            maxDistance = optimalPosition.calculate();
            //print(maxDistance);
            //print((ballDistance - goalkeeperValues.minDistanceToBallWhitOwner) + " | " + maxDistance + " | "+ timeToGetTheBallToTheEnemy);

        }

        float distance1 = Mathf.Clamp(ballDistance - minDistanceBetweenGoalkeeperBall, 0, maxDistance);
        float angleGoalForward_Ball = Vector3.Angle(GoalComponents.centerOptimalPosition.forward, ComponentsBall.transBall.position - GoalComponents.centerOptimalPosition.position);
        float distanceAngle = Mathf.Lerp(distance1, 0, goalkeeperValues.optimalPositionEscoradoBallInfluence.Evaluate(angleGoalForward_Ball / 90));
        float farDistance = goalkeeperValues.adjustBallIsFarCurve.Evaluate((distanceClosestPointAndBall - goalkeeperValues.adjustBallIsFar1) / goalkeeperValues.adjustBallIsFar2);
        float farAngleDistance = Mathf.Lerp(distanceAngle, 0, farDistance);

        Vector3 closestGoalkeeperCenterLinePoint = MyFunctions.GetClosestPointOnFiniteLine(goalKeeperTransform.position, centerPosition, ComponentsBall.rigBall.position);
        float distanceClosestPointGoalkeeper = Vector3.Distance(goalKeeperTransform.position, closestGoalkeeperCenterLinePoint);
        Vector3 point1 = centerPosition - dirCenterPosition.normalized * farAngleDistance;
        //print((farAngleDistance) + " | "+maxDistance);
        result = Vector3.Lerp(point1, closestGoalkeeperCenterLinePoint, distanceClosestPointGoalkeeper / 2);
        result = cornerAdjust(result);
        Area.GetPoint(result, new Ray(centerPosition, -dirCenterPosition), out result);
    }

    //Debug.DrawLine(componentsBall.rigBall.position, result);
    result = ClampGoalKeeper(result);
    return result;
}*/