using System.Numerics;
using UnityEngine;

using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class Leg : MonoBehaviour
{
    public GameObject CreatureBody;
    public GameObject LegTarget;
    public Vector3 BodyJointPosition => new ( CreatureBody.transform.position.x + (CreatureBody.transform.localScale.x / 2),
                                              CreatureBody.transform.position.y + (CreatureBody.transform.localScale.y / 2), 
                                              CreatureBody.transform.position.z );
    public Vector3 ArmEndPosition;
    public float LegSegmentLength = 1.0f;

    private Vector3 armJointPosition;
    private Vector3 bodyJointArmEndMidPoint;
    public Vector3 legTargetPoint;
    private float gizmosJointSize = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ArmEndPosition = new(CreatureBody.transform.position.x + 2f,
                             0f,
                             CreatureBody.transform.position.z);

        // Calculate position of the LegTargetPoint
        legTargetPoint = new Vector3(CreatureBody.transform.position.x + 2f, 0, CreatureBody.transform.position.z + 0.75f);

        //var leg = CreateLegSegment("Leg1");
        //GameObject.Instantiate(leg);
    }

    //GameObject CreateLegSegment(string name)
    //{
    //    GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
    //    segment.name = name;
    //    Destroy(segment.GetComponent<Collider>()); // Remove collider for simplicity
        
    //    // We will scale and position these later
    //    segment.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f); // Default thickness, height set later
    //    return segment;
    //}

    // Update is called once per frame
    void Update()
    {
        // Get midpoint on triangle base
        bodyJointArmEndMidPoint = GetMidPoint(BodyJointPosition, ArmEndPosition);

        // Midpoint + HeightOfIsoscelesTriangle = position of the arm joint
        var baseLength = GetDistance(BodyJointPosition, ArmEndPosition);
        armJointPosition = new Vector3(bodyJointArmEndMidPoint.x,
                                       bodyJointArmEndMidPoint.y + GetHeightOfIsoscelesTriangle(LegSegmentLength, baseLength),
                                       bodyJointArmEndMidPoint.z);

        var targetOffsetVector = new Vector3(2f, 0f, 0.75f);
        legTargetPoint = Quaternion.AngleAxis(CreatureBody.transform.eulerAngles.y, Vector3.up) 
            * targetOffsetVector
            + new Vector3(CreatureBody.transform.position.x, 0, CreatureBody.transform.position.z);


    }

    Vector3 GetMidPoint(Vector3 firstPoint, Vector3 secondPoint)
    {
        var x = (firstPoint.x + secondPoint.x) / 2; 
        var y = (firstPoint.y + secondPoint.y) / 2; 
        var z = (firstPoint.z + secondPoint.z) / 2;

        return new Vector3(x,y,z);
    }

    // h = sqrt(a^2 – (b^2/4))
    // h - height
    // a - equal sides length
    // b - base length
    float GetHeightOfIsoscelesTriangle(float sideLength, float baseLength)
    {
        return Mathf.Sqrt(sideLength * sideLength - (baseLength * baseLength / 4));
    }

    // d = ((x2 - x1)2 + (y2 - y1)2 + (z2 - z1)2)1/2
    float GetDistance(Vector3 firstPoint, Vector3 secondPoint)
    {
        var dstX = Mathf.Abs(firstPoint.x - secondPoint.x);
        var dstY = Mathf.Abs(firstPoint.y - secondPoint.y);
        var dstZ = Mathf.Abs(firstPoint.z - secondPoint.z);

        var sum = dstX * dstX + dstY * dstY + dstZ * dstZ;
        var d = Mathf.Pow(sum, 1/2);
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
        Gizmos.DrawSphere(ArmEndPosition, gizmosJointSize); // Draw end of arm with black
        Gizmos.DrawSphere(BodyJointPosition, gizmosJointSize); // Draw static joint attached to the creature body

        // Connect the joints with lines
        Gizmos.DrawLine(BodyJointPosition, armJointPosition);
        Gizmos.DrawLine(armJointPosition, ArmEndPosition);

        // Draw legs target
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(legTargetPoint, gizmosJointSize);
    }
}
