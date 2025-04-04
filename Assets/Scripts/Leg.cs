using UnityEngine;

public class Leg : MonoBehaviour
{
    public GameObject CreatureBody;
    public float LegSegmentLength = 1.0f;
    public float StepLength = 1.0f;

    public Vector3 FootTargetPosition { get; private set; }
    public Vector3 FootPosition { get; private set; }

    private Vector3 HipPosition => CreatureBody.transform.position + new Vector3(CreatureBody.transform.localScale.x / 2, CreatureBody.transform.localScale.y / 2, 0);
    private Vector3 KneePosition { get; set; }

    private GameObject UpperLegSegment;
    private const float GizmosJointSize = 0.1f;

    void Start()
    {
        InitializeFootPositions();
        RecalculateIK();
        UpperLegSegment = CreateLegSegment("Upper_Leg_1");
    }

    void Update()
    {
        RecalculateIK();
        UpdateLegVisuals();

        float legSegmentLength = Vector3.Distance(HipPosition, KneePosition) / 2;
        print(legSegmentLength);
    }

    private void InitializeFootPositions()
    {
        FootPosition = CreatureBody.transform.position + new Vector3(2f, 0f, 0f);
        FootTargetPosition = FootPosition + new Vector3(0f, 0f, 0.75f);
        RecalculateIK();
    }

    private void RecalculateIK()
    {
        Vector3 midPoint = GetMidPoint(HipPosition, FootPosition);
        float baseLength = Vector3.Distance(HipPosition, FootPosition);
        KneePosition = midPoint + Vector3.up * GetHeightOfIsoscelesTriangle(LegSegmentLength, baseLength);

        UpdateFootTargetPosition();
        MoveFootTowardsTarget();
    }

    private void UpdateFootTargetPosition()
    {
        Vector3 targetOffset = new Vector3(2f, 0f, 0.75f);
        Vector3 raycastStart = Quaternion.AngleAxis(CreatureBody.transform.eulerAngles.y, Vector3.up) * targetOffset + CreatureBody.transform.position + Vector3.up * 5f;

        if (Physics.Raycast(raycastStart, Vector3.down, out RaycastHit hit))
        {
            FootTargetPosition = raycastStart + Vector3.down * hit.distance;
        }
    }

    private void MoveFootTowardsTarget()
    {
        if (Vector3.Distance(FootPosition, FootTargetPosition) > StepLength)
        {
            FootPosition = Vector3.MoveTowards(FootPosition, FootTargetPosition, StepLength);
        }
    }

    private void UpdateLegVisuals()
    {
        Vector3 upperLegPosition = GetMidPoint(HipPosition, KneePosition);
        UpperLegSegment.transform.position = upperLegPosition;
        UpperLegSegment.transform.LookAt(HipPosition);
        UpperLegSegment.transform.Rotate(90f, 0f, 0f);
    }

    private GameObject CreateLegSegment(string name)
    {

        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        segment.name = name;
        Destroy(segment.GetComponent<Collider>());

        float legSegmentLength = Vector3.Distance(HipPosition, KneePosition) / 2; // Divide in half, because the leg segment object is symetrically expanding to the both directions
        segment.transform.localScale = new Vector3(0.1f, legSegmentLength, 0.1f);
        return segment;
    }

    private Vector3 GetMidPoint(Vector3 firstPoint, Vector3 secondPoint) 
        => (firstPoint + secondPoint) / 2;

    // h = sqrt(a^2 � (b^2/4))
    // h - height
    // a - equal sides length
    // b - base length
    private float GetHeightOfIsoscelesTriangle(float sideLength, float baseLength) 
        => Mathf.Sqrt(sideLength * sideLength - (baseLength * baseLength / 4));

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