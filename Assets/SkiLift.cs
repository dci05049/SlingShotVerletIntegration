using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkiLift : MonoBehaviour
{

    public Transform StartPoint;
    public Transform EndPoint;

    private LineRenderer lineRenderer;
    private List<RopeSegment> ropeSegments = new List<RopeSegment>();
    private float ropeSegLen = 0.55f;
    private int segmentLength = 35;
    private float lineWidth = 0.1f;

    //Sling shot 
    private bool moveToMouse = false;
    private Vector3 mousePositionWorld;
    private int indexMousePos;
    [SerializeField]
    private GameObject followTarget;

    // Use this for initialization
    void Start()
    {
        this.lineRenderer = this.GetComponent<LineRenderer>();
        Vector3 ropeStartPoint = StartPoint.position;

        for (int i = 0; i < segmentLength; i++)
        {
            this.ropeSegments.Add(new RopeSegment(ropeStartPoint));
            ropeStartPoint.y -= ropeSegLen;
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.DrawRope();
        if (Input.GetMouseButtonDown(0))
        {
            this.moveToMouse = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            this.moveToMouse = false;
        }

        Vector3 screenMousePos = Input.mousePosition;
        float xStart = StartPoint.position.x;
        float xEnd = EndPoint.position.x;
        float currX = this.followTarget.transform.position.x;

        float ratio = (currX - xStart) / (xEnd - xStart);
        Debug.Log(ratio);
        if (ratio > 0)
        {
            this.indexMousePos = (int)(this.segmentLength * ratio);
        }
    }

    private void FixedUpdate()
    {
        this.Simulate();
    }

    private void Simulate()
    {
        // SIMULATION
        Vector2 forceGravity = new Vector2(0f, -1f);

        for (int i = 1; i < this.segmentLength; i++)
        {
            RopeSegment firstSegment = this.ropeSegments[i];
            Vector2 velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.fixedDeltaTime;
            this.ropeSegments[i] = firstSegment;
        }

        //CONSTRAINTS
        for (int i = 0; i < 50; i++)
        {
            this.ApplyConstraint();
        }
    }

    private void ApplyConstraint()
    {
        //Constrant to First Point 
        RopeSegment firstSegment = this.ropeSegments[0];
        firstSegment.posNow = this.StartPoint.position;
        this.ropeSegments[0] = firstSegment;


        //Constrant to Second Point 
        RopeSegment endSegment = this.ropeSegments[this.ropeSegments.Count - 1];
        endSegment.posNow = this.EndPoint.position;
        this.ropeSegments[this.ropeSegments.Count - 1] = endSegment;

        for (int i = 0; i < this.segmentLength - 1; i++)
        {
            RopeSegment firstSeg = this.ropeSegments[i];
            RopeSegment secondSeg = this.ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = Mathf.Abs(dist - this.ropeSegLen);
            Vector2 changeDir = Vector2.zero;

            if (dist > ropeSegLen)
            {
                changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;
            }
            else if (dist < ropeSegLen)
            {
                changeDir = (secondSeg.posNow - firstSeg.posNow).normalized;
            }

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                this.ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                this.ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                this.ropeSegments[i + 1] = secondSeg;
            }

            if (indexMousePos > 0 && indexMousePos < this.segmentLength - 1 && i == indexMousePos)
            {
                RopeSegment segment = this.ropeSegments[i];
                RopeSegment segment2 = this.ropeSegments[i + 1];
                segment.posNow = new Vector2(this.followTarget.transform.position.x, this.followTarget.transform.position.y);
                segment2.posNow = new Vector2(this.followTarget.transform.position.x, this.followTarget.transform.position.y);
                this.ropeSegments[i] = segment;
                this.ropeSegments[i + 1] = segment2;
            }
        }
    }

    private void DrawRope()
    {
        float lineWidth = this.lineWidth;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[this.segmentLength];
        for (int i = 0; i < this.segmentLength; i++)
        {
            ropePositions[i] = this.ropeSegments[i].posNow;
        }

        lineRenderer.positionCount = ropePositions.Length;
        lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            this.posNow = pos;
            this.posOld = pos;
        }
    }
}
