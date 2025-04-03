using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Rameno : MonoBehaviour
{
    public GameObject CreatureBody;

    public Vector3 BodyJointPosition => new (CreatureBody.transform.position.x + (CreatureBody.transform.localScale.x / 2),
                                             CreatureBody.transform.position.y + (CreatureBody.transform.localScale.y / 2), 
                                             CreatureBody.transform.position.z );
    public Vector3 ArmJointPosition;
    public Vector3 ArmEndPosition;

    private Vector3 bodyJointArmEndMidPoint;

    public float LegSegmentLength = 1.0f;
    public float MaxReachRange = 3.0f;

    private float gizmosJointSize = 0.1f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ArmEndPosition = new(CreatureBody.transform.position.x + 2f,
                                         0f,
                                         CreatureBody.transform.position.z);

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
        ArmJointPosition = new Vector3(bodyJointArmEndMidPoint.x,
                                        bodyJointArmEndMidPoint.y + GetHeightOfIsoscelesTriangle(LegSegmentLength, baseLength),
                                        bodyJointArmEndMidPoint.z);
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
        Gizmos.DrawSphere(ArmJointPosition, gizmosJointSize); // Draw arm joint

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(ArmEndPosition, gizmosJointSize); // Draw end of arm with black
        Gizmos.DrawSphere(BodyJointPosition, gizmosJointSize); // Draw static joint attached to the creature body

        // Connect the joints with lines
        Gizmos.DrawLine(BodyJointPosition, ArmJointPosition);
        Gizmos.DrawLine(ArmJointPosition, ArmEndPosition);
    }
}
