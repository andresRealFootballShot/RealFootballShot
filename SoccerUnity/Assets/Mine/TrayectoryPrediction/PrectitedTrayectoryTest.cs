using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PrectitedTrayectoryTest : MonoBehaviour
{
    public Transform velocityTransform;
    public float speed=20;
    public Rigidbody ballRigidbody;
    Rigidbody ballGhost;
    private Scene _simulationScene;
    private PhysicsScene _physicsScene;

    [SerializeField] private LineRenderer _line;
    [SerializeField] private int _maxPhysicsFrameIterations = 100;
    [SerializeField] private float timeIncrement = 0.1f;
    [SerializeField] private Transform terrainTransform;
    private void Start()
    {
        CreatePhysicsScene();
    }

    private void CreatePhysicsScene()
    {
        
        _simulationScene = SceneManager.CreateScene("Simulation", new CreateSceneParameters(LocalPhysicsMode.Physics3D));
        _physicsScene = _simulationScene.GetPhysicsScene();
        var ghostObj = Instantiate(terrainTransform.gameObject, terrainTransform.position, terrainTransform.rotation);
        //ghostObj.GetComponent<Renderer>().enabled = false;
        SceneManager.MoveGameObjectToScene(ghostObj, _simulationScene);
        
        ballGhost = Instantiate(ballRigidbody, ballRigidbody.position, ballRigidbody.rotation);
        ballGhost.GetComponent<Renderer>().enabled = false;
        SceneManager.MoveGameObjectToScene(ballGhost.gameObject, _simulationScene);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ballRigidbody.position = velocityTransform.position;
            ballRigidbody.velocity = velocityTransform.forward * speed;
            ballGhost.position = velocityTransform.position;
            ballGhost.velocity = velocityTransform.forward * speed;

            
        }
        SimulateTrajectory();
    }
    public void SimulateTrajectory()
    {
        
        _line.positionCount = _maxPhysicsFrameIterations;

        _line.SetPosition(0, ballGhost.transform.position);
        float t = 0;
        for (var i = 0; i < _maxPhysicsFrameIterations; i++)
        {
            t += Time.fixedDeltaTime;
            _physicsScene.Simulate(Time.fixedDeltaTime);
            _line.SetPosition(i, ballGhost.transform.position);
            
        }
        print(t);
    }
}
