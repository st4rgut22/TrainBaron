using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour {

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    public LayerMask blockingLayer;
    public float moveTime;
    public float inverseMoveTime;
    float lerpTime = 1;
    float currentLerpTime = 0;
    protected bool move1 = true; //initialize true 
    protected bool move2 = true;
    public GameController gc;

    // Use this for initialization
    public virtual void Awake()
    {
        moveTime = 2.0f;
        inverseMoveTime = 1f / moveTime;
        boxCollider = transform.GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        gc = GameController.instance;
    }

    protected Vector3 GetBezierPosition(float t, Vector2 destination)
    {
        Vector2 A = transform.position;
        Vector2 B = A + new Vector2(0,.001f);
        Vector2 C = destination;
        float oneMinusT = 1f - t;
        Vector2 Q = oneMinusT * A + t * B;
        Vector2 R = oneMinusT * B + t * C;
        Vector2 P = oneMinusT * Q + t * R;
        return P;
    }

    protected bool CanMove(Vector2 destination, float rotation, out RaycastHit2D hit,string tag){
        //Store start position to move from, based on objects current transform position.
        Vector2 start = transform.position;
        //Disable the boxCollider so that linecast doesn't hit this object's own collider.
        transform.GetComponent<BoxCollider2D>().enabled = false;

        //Cast a line from start point to end point checking collision on blockingLayer.
        hit = Physics2D.Linecast(start, destination, blockingLayer);

        //Re-enable boxCollider after linecast
        transform.GetComponent<BoxCollider2D>().enabled = true;

        //Check if anything was hit
        if (hit.transform == null)
        {
            //If nothing was hit, start SmoothMovement co-routine passing in the Vector2 end as destination
            if (tag == "curve1" || tag  == "curve2" || tag == "curve3" || tag == "curve4")
            {

                //go straight
                //StartCoroutine(LinearMovement(transform.position+new Vector3(0,.5f)));
                //transform.rotation = transform.rotation * Quaternion.Euler(0, 0, 90);
                Debug.Log("start bezier movement");
                StartCoroutine(RotateMovement(rotation));
                StartCoroutine(BezierMovement(destination));
            }
            else if (tag == "Hor" || tag == "Vert" || tag=="industry")
            {
                Debug.Log("start linear movement");
                StartCoroutine(LinearMovement(destination));
            }
            return true;
        }
        //If something was hit, return false, Move was unsuccesful.
        return false;
    }

    protected IEnumerator RotateMovement(float rotate) {
        move1 = false;
        float speed = 160f;
        Quaternion testRotate = Quaternion.Euler(0, 0, rotate); //composing rotations
        //Debug.Log("test rotate " + testRotate.eulerAngles);
        Quaternion targetRotate = transform.rotation * Quaternion.AngleAxis(rotate, Vector3.forward);
        Debug.Log("target rotate " + targetRotate);
        Debug.Log("rotate " + transform.rotation);
        //Quaternion targetRotate = transform.rotation * Quaternion.Euler(0, 0, rotate); //composing rotations
        float angle = Quaternion.Angle(targetRotate, transform.rotation);
        Debug.Log("angle is " + angle);
        while (angle>.05f){
            Debug.Log("angle is " + angle);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotate, speed * Time.deltaTime);
            angle = Quaternion.Angle(transform.rotation, targetRotate);
            yield return null;
        }
        Debug.Log("rotate complete");
        move1 = true;
    }

    protected IEnumerator BezierMovement(Vector2 destination){
        move2 = false;
        float resolution = 0.025f; //the smaller, the higher porabola and the slower it is
        int loops = Mathf.FloorToInt(1f / resolution);
        for (int i = 0; i < loops;i++){
            float t = i * resolution;
            Vector2 newPos = GetBezierPosition(t, destination);
            rb2D.MovePosition(newPos);
            yield return null;
        }
        Debug.Log("bezier loop complete");
        move2 = true;
    }

    protected IEnumerator LinearMovement(Vector3 destination)
    {
        move1 = false;
        move2 = false;
        float sqrDistance = (transform.position - destination).sqrMagnitude;
        while (sqrDistance>float.Epsilon){
            //Find a new position proportionally closer to the end, based on the moveTime
            Vector2 newPos = Vector2.MoveTowards(rb2D.position, destination, inverseMoveTime*Time.deltaTime);
            //Call MovePosition on attached Rigidbody2D and move it to the calculated position.
            rb2D.MovePosition(newPos);
            //Recalculate the remaining distance after moving.
            sqrDistance = (transform.position - destination).sqrMagnitude;
            //Return and loop until sqrRemainingDistance is close enough to zero to end the function
            yield return null;
        }
        move1 = true;
        move2 = true;
    }
}
