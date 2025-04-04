using UnityEngine;

public class Leg : MonoBehaviour
{
    public GameObject CreatureBody;
    public Vector3 BodyJointPosition => new(CreatureBody.transform.position.x + (CreatureBody.transform.localScale.x / 2),
                                              CreatureBody.transform.position.y + (CreatureBody.transform.localScale.y / 2),
                                              CreatureBody.transform.position.z);
    public Vector3 FootPosition;
    public Vector3 FootTargetPosition;
    public float LegSegmentLength = 1.0f;
    public float StepLength = 1.0f;

    private Vector3 armJointPosition;
    private Vector3 bodyJointArmEndMidPoint;
    private float gizmosJointSize = 0.1f;

    private Vector3 UpperLegSegmentPosition;
    private Vector3 LowerLegSegmentPosition;
    private GameObject UpperLegSegment;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FootPosition = new(CreatureBody.transform.position.x + 2f,
                             0f,
                             CreatureBody.transform.position.z);

        // Calculate position of the LegTargetPoint
        FootTargetPosition = new Vector3(CreatureBody.transform.position.x + 2f, 0, CreatureBody.transform.position.z + 0.75f);

        RecalculateIK();
        UpperLegSegment = CreateLegSegment("Upper_Leg_1");
    }

    // Update is called once per frame
    void Update()
    {
        RecalculateIK();
        UpdateLegVisuals();
    }

    void RecalculateIK()
    {
        // Get midpoint on triangle base
        bodyJointArmEndMidPoint = GetMidPoint(BodyJointPosition, FootPosition);

        // Midpoint + HeightOfIsoscelesTriangle = position of the arm joint
        var baseLength = GetDistance(BodyJointPosition, FootPosition);
        armJointPosition = new Vector3(bodyJointArmEndMidPoint.x,
                                       bodyJointArmEndMidPoint.y + GetHeightOfIsoscelesTriangle(LegSegmentLength, baseLength),
                                       bodyJointArmEndMidPoint.z);

        var targetOffsetVector = new Vector3(2f, 0f, 0.75f);

        // Raycast from from legTargetPoint, to move it up/down
        var raycastStartPoint = Quaternion.AngleAxis(CreatureBody.transform.eulerAngles.y, Vector3.up)
            * targetOffsetVector
            + new Vector3(CreatureBody.transform.position.x, 0, CreatureBody.transform.position.z)
            + Vector3.up * 5f;

        if (Physics.Raycast(raycastStartPoint, Vector3.down, out var hit))
        {
            FootTargetPosition = raycastStartPoint + Vector3.down * hit.distance;
        }

        // Do step if distance between foot and target gets too big.
        var footToFootTargetDistance = GetDistance(FootPosition, FootTargetPosition);
        if (footToFootTargetDistance > StepLength)
        {
            FootPosition = Vector3.MoveTowards(FootPosition, FootTargetPosition, StepLength);
        }
    }

    private void UpdateLegVisuals()
    {
        // Update the leg visuals
        UpperLegSegment.transform.position = UpperLegSegmentPosition;
        UpperLegSegment.transform.LookAt(BodyJointPosition);
        UpperLegSegment.transform.Rotate(90f, 0f, 0f);
    }
    GameObject CreateLegSegment(string name)
    {
        print($"{nameof(Leg)}_{nameof(CreateLegSegment)}");

        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        segment.name = name;
        Destroy(segment.GetComponent<Collider>()); // Remove collider for simplicity

        // We will scale and position these later
        var legSegmentLength = GetDistance(BodyJointPosition, armJointPosition);
        print(legSegmentLength);
        segment.transform.localScale = new Vector3(0.1f, legSegmentLength, 0.1f); // Default thickness, height set later
        return segment;
    }

    Vector3 GetMidPoint(Vector3 firstPoint, Vector3 secondPoint)
    {
        var x = (firstPoint.x + secondPoint.x) / 2;
        var y = (firstPoint.y + secondPoint.y) / 2;
        var z = (firstPoint.z + secondPoint.z) / 2;

        return new Vector3(x, y, z);
    }

    // h = sqrt(a^2 – (b^2/4))
    // h - height
    // a - equal sides length
    // b - base length
    float GetHeightOfIsoscelesTriangle(float sideLength, float baseLength)
    {
        return Mathf.Sqrt(sideLength * sideLength - (baseLength * baseLength / 4));
    }

    // d = Sqrt((x2 - x1)2 + (y2 - y1)2 + (z2 - z1)2)
    float GetDistance(Vector3 firstPoint, Vector3 secondPoint)
    {
        var dstX = Mathf.Abs(firstPoint.x - secondPoint.x);
        var dstY = Mathf.Abs(firstPoint.y - secondPoint.y);
        var dstZ = Mathf.Abs(firstPoint.z - secondPoint.z);

        var sum = dstX * dstX + dstY * dstY + dstZ * dstZ;
        var d = Mathf.Sqrt(sum);
        return d;
    }

    void OnDrawGizmos()
    {
        // Draw midpoint on the base of the triangle
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(bodyJointArmEndMidPoint, gizmosJointSize);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(armJointPosition, gizmosJointSize); // Draw arm joint

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(FootPosition, gizmosJointSize); // Draw end of arm with black
        Gizmos.DrawSphere(BodyJointPosition, gizmosJointSize); // Draw static joint attached to the creature body
        Gizmos.DrawLine(FootPosition, FootTargetPosition); // Draw line between foot and its target

        // Connect the joints with lines
        Gizmos.DrawLine(BodyJointPosition, armJointPosition);
        Gizmos.DrawLine(armJointPosition, FootPosition);

        // Draw legs target
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(FootTargetPosition, gizmosJointSize);

        // Get midpoints between joins, so we can position the leg segments
        Gizmos.color = Color.yellow;
        UpperLegSegmentPosition = GetMidPoint(BodyJointPosition, armJointPosition);
        Gizmos.DrawSphere(UpperLegSegmentPosition, gizmosJointSize);
        LowerLegSegmentPosition = GetMidPoint(armJointPosition, FootPosition);
        Gizmos.DrawSphere(LowerLegSegmentPosition, gizmosJointSize);
    }
}