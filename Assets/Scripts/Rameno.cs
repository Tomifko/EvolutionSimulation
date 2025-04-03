using UnityEngine;

public class Rameno : MonoBehaviour
{
    public Vector3 BodyJointPosition = new Vector3(0,1f,0);
    public Vector3 ArmJointPosition = new Vector3(1f,1.5f,0);
    public Vector3 ArmEndPosition = new Vector3(2f,1f,0);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
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
        
    }

    void OnDrawGizmos()
    {
        // Draw static joint attached to the creature body
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(BodyJointPosition, 0.1f);

        // Draw joint with red
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(ArmJointPosition, 0.1f);

        // Draw end of arm with black
        Gizmos.color = Color.black;
        Gizmos.DrawSphere(ArmEndPosition, 0.1f);


        Gizmos.DrawLine(BodyJointPosition, ArmJointPosition);
        Gizmos.DrawLine(ArmJointPosition, ArmEndPosition);
    }
}
