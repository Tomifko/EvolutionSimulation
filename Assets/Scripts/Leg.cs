using UnityEngine;

public class Leg : MonoBehaviour
{
    public GameObject CreatureBody;
    public float LegSegmentLength = 1.0f;
    public float StepLength = 1.0f;
    public float TargetDistance = 0.5f;
    public float HipJointZOffset = 0.0f;
    public Side LegSide = Side.Right;

    public Vector3 FootTargetPosition { get; private set; }
    public Vector3 FootPosition { get; private set; }

    // Determine exact position of hip joint based on the side of the body the leg is attached to
    private Vector3 HipPosition => CreatureBody.transform.position 
                                    + new Vector3(CreatureBody.transform.localScale.x / 2 * DirectionModificator, // Determine if we're working with right or left leg
                                                  CreatureBody.transform.localScale.y / 2 * -1, // Attach the leg to the bottom of the creature
                                                  HipJointZOffset);
    private Vector3 KneePosition { get; set; }
    private int DirectionModificator => LegSide == Side.Right ? 1 : -1;

    private bool isMoving;
    private Vector3 stepStartPosition;
    private float stepProgress = 0f;
    public float StepHeight = 0.5f; // Maximum height of step arc
    public float StepDuration = 0.3f; // Duration of each step in seconds

    private GameObject UpperLegSegment;
    private GameObject LowerLegSegment;

    private const float GizmosJointSize = 0.1f;

    void Start()
    {
        InitializeFootPositions();

        // Create leg visuals
        //UpperLegSegment = CreateLegSegment($"Upper_{name}", 0.2f, HipPosition, KneePosition);
        //LowerLegSegment = CreateLegSegment($"Lower_{name}", 0.15f, KneePosition, FootPosition);

        //UpperLegSegment.transform.SetParent(transform);
        //LowerLegSegment.transform.SetParent(transform);
    }

    void Update()
    {
        RecalculateIK();
        //UpdateLegVisuals();
        MoveFootTowardsTarget(StepLength);
    }

    private void InitializeFootPositions()
    {
        FootTargetPosition = CreatureBody.transform.position + new Vector3(2f * DirectionModificator, 0f, 0f); ;
        FootPosition = FootTargetPosition;
        RecalculateIK();
        SetFootPosition(FootTargetPosition);
    }

    private void RecalculateIK()
    {
        Vector3 midPoint = GetMidPoint(HipPosition, FootPosition);
        float baseLength = Vector3.Distance(HipPosition, FootPosition);
        KneePosition = midPoint + Vector3.up * GetHeightOfIsoscelesTriangle(LegSegmentLength, baseLength);
        
        UpdateFootTargetPosition();
    }

    private void UpdateFootTargetPosition()
    {
        var targetOffsetScale = 4f;
        Vector3 targetOffset = new(2f * DirectionModificator, 0f, HipJointZOffset * targetOffsetScale);
        Vector3 raycastStart = Quaternion.AngleAxis(CreatureBody.transform.eulerAngles.y, Vector3.up) * targetOffset 
                                + CreatureBody.transform.position + Vector3.up * 5f;

        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hit))
        {
            FootTargetPosition = raycastStart + Vector3.down * hit.distance;
        }
    }

    private void SetFootPosition(Vector3 targetPosition)
    {
        FootPosition = targetPosition;
    }

    private void MoveFootTowardsTarget(float stepTriggerDistance)
    {
        var isStepNecessary = Vector3.Distance(FootPosition, FootTargetPosition) > stepTriggerDistance;

        if (isMoving)
        {
            // Increase step progress
            stepProgress += Time.deltaTime / StepDuration;

            if (stepProgress >= 1f)
            {
                // Step is complete
                SetFootPosition(FootTargetPosition);
                isMoving = false;
            }
            else
            {
                // Calculate position along the path using smooth interpolation
                float horizontalProgress = Mathf.SmoothStep(0f, 1f, stepProgress);

                Vector3 horizontalPosition = Vector3.Lerp(stepStartPosition, FootTargetPosition, horizontalProgress);
                float verticalOffset = Mathf.Sin(horizontalProgress * Mathf.PI) * StepHeight;

                // Set the new foot position with the vertical arc added
                SetFootPosition(horizontalPosition + new Vector3(0, verticalOffset, 0));
            }
        }
        else if (isStepNecessary)
        {
            // Start a new stepping motion
            isMoving = true;
            stepStartPosition = FootPosition;
            stepProgress = 0f;
        }
    }

    #region Visual stuff
    private void UpdateLegVisuals()
    {
        // Update upper leg segment 
        UpdateLegSegmentVisual(UpperLegSegment, HipPosition, KneePosition);

        // Update lower leg segment 
        UpdateLegSegmentVisual(LowerLegSegment, KneePosition, FootPosition);
    }

    private void UpdateLegSegmentVisual(GameObject legSegment, Vector3 upperJoint, Vector3 lowerJoint)
    {
        Vector3 target = GetMidPoint(upperJoint, lowerJoint);
        var legPosition = Vector3.Slerp(legSegment.transform.position, target, Time.deltaTime * 10f);
        legSegment.transform.position = legPosition;
        legSegment.transform.LookAt(upperJoint);
        legSegment.transform.Rotate(90f, 0f, 0f);
    }

    private GameObject CreateLegSegment(string name, float thickness, Vector3 upperJoint, Vector3 lowerJoint)
    {

        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Cube);
        segment.name = name;
        Destroy(segment.GetComponent<Collider>());

        float legSegmentLength = Vector3.Distance(upperJoint, lowerJoint);
        segment.transform.localScale = new Vector3(thickness, legSegmentLength, thickness);
        return segment;
    }
    #endregion

    private Vector3 GetMidPoint(Vector3 firstPoint, Vector3 secondPoint) 
        => (firstPoint + secondPoint) / 2;

    // h = sqrt(a^2 – (b^2/4))
    // h - height
    // a - equal sides length
    // b - base length
    private float GetHeightOfIsoscelesTriangle(float sideLength, float baseLength) 
        => Mathf.Sqrt(sideLength * sideLength - (baseLength * baseLength / 4));

    public enum Side
    {
        Right,
        Left,
    }

    private void OnDrawGizmos()
    {
        // Draw midpoint on the base of the triangle
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(GetMidPoint(HipPosition, FootPosition), GizmosJointSize);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(KneePosition, GizmosJointSize); // Draw knee joint

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(FootPosition, GizmosJointSize); // Draw end of arm with black
        Gizmos.DrawSphere(HipPosition, GizmosJointSize); // Draw static joint attached to the creature body
        Gizmos.DrawLine(FootPosition, FootTargetPosition); // Draw line between foot and its target

        // Connect the joints with lines
        Gizmos.DrawLine(HipPosition, KneePosition);
        Gizmos.DrawLine(KneePosition, FootPosition);

        // Draw legs target
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(FootTargetPosition, GizmosJointSize);

        // Get midpoints between joins, so we can position the leg segments
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GetMidPoint(HipPosition, KneePosition), GizmosJointSize);
        Gizmos.DrawSphere(GetMidPoint(KneePosition, FootPosition), GizmosJointSize);
    }
}