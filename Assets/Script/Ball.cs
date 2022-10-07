using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

public class Ball : MonoBehaviour
{
    [SerializeField, Header("Ball Push Power")]
    private float Force = 100;
    [SerializeField, Header("Ball Prediction Prefab")]
    private GameObject BallPrediction;
    [SerializeField, Header("Ball Trajectory Iteration")]
    private int maxtrajectoryIteration = 50;
    [SerializeField, Header("Game Manager Score Event")]
    private UnityEvent ScoreEvent;
    [SerializeField, Header("Game Manager Random Spawn Area")]
    private UnityEvent<Transform> GroundEvent;
    // var
    private Vector2 startPosition;
    private Vector2 defaultballPosition;
    private float ballscorePosition;


    //component
    private Rigidbody2D rb2d;


    //scene control
    private Scene sceneMain;
    private PhysicsScene2D scenemainPhysics;
    private Scene scenePrediction;
    private PhysicsScene2D scenepredictionPhysics;


    private void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

    }

    void Start()
    {
        Physics2D.simulationMode = SimulationMode2D.Script;


        GetComponent<Rigidbody2D>().isKinematic = true;
        defaultballPosition = transform.position;


        createsceneMain();
        createscenePrediction();


    }

    private void createsceneMain()
    {
        sceneMain = SceneManager.CreateScene("MainScene");
        scenemainPhysics = sceneMain.GetPhysicsScene2D();

    }
    private void createscenePrediction()
    {
        CreateSceneParameters sceneParameters = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        scenePrediction = SceneManager.CreateScene("PredictionScene", sceneParameters);
        scenepredictionPhysics = scenePrediction.GetPhysicsScene2D();

    }

    // Update is called once per frame
    void Update()
    {

        //Debug.Log(rigidbody2D);

        if (Input.GetMouseButtonDown(0))
        {
            startPosition = getmousePosition();
        }
        if (Input.GetMouseButton(0))
        {

            GameObject newballPrediction = spawnballPrediction();
            throwBall(newballPrediction.GetComponent<Rigidbody2D>());

            //--------------------------
            createTrajectory(newballPrediction);

            Destroy(newballPrediction);
        }
        if (Input.GetMouseButtonUp(0))
        {
            GetComponent<LineRenderer>().positionCount = 0;
            GetComponent<Rigidbody2D>().isKinematic = false;
            throwBall(GetComponent<Rigidbody2D>());
        }
    }

    private void createTrajectory(GameObject newballPrediction)
    {
        LineRenderer ballLine = GetComponent<LineRenderer>();
        ballLine.positionCount = maxtrajectoryIteration;

        for (int i = 0; i < maxtrajectoryIteration; i++)
        {
            scenepredictionPhysics.Simulate(Time.fixedDeltaTime);
            ballLine.SetPosition(i, new Vector3(newballPrediction.transform.position.x, newballPrediction.transform.position.y, 0));
        }
    }

    private void throwBall(Rigidbody2D rb)
    {
        rb.AddForce(getthrowPower(startPosition, getmousePosition()), ForceMode2D.Force);
    }


    private GameObject spawnballPrediction()
    {
        GameObject newballPrediction = GameObject.Instantiate(BallPrediction);
        SceneManager.MoveGameObjectToScene(newballPrediction, scenePrediction);
        newballPrediction.transform.position = transform.position;

        return newballPrediction;
    }

    private Vector2 getthrowPower(Vector2 startPosition, Vector2 endPosition)
    {
        return (startPosition - endPosition) * Force;
    }


    private void FixedUpdate()
    {
        if (!scenemainPhysics.IsValid()) return;
        scenemainPhysics.Simulate(Time.fixedDeltaTime);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log(collision.gameObject.tag);

        if (collision.gameObject.tag.Equals("Ground"))
        {
            StartCoroutine(reset());
        }
    }
    IEnumerator reset()
    {
        yield return new WaitForSeconds(.3f);
        GetComponent<Rigidbody2D>().isKinematic = true;
        GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        GetComponent<Rigidbody2D>().angularVelocity = 0f;
        GroundEvent.Invoke(transform);
    }


    //************score trigger
    private void OnTriggerEnter2D(Collider2D collision)
    {
        ballscorePosition = transform.position.y;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (transform.position.y < ballscorePosition)
        {
            Debug.Log("Scored");
            ScoreEvent.Invoke();
        }
    }




    private Vector2 getmousePosition()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }
}
