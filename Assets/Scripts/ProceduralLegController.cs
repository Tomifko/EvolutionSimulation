using UnityEngine;
using System.Collections.Generic; // Required for using Lists

public class ProceduralLegController : MonoBehaviour
{
    [Header("Body")]
    public Transform body; // Assign the CreatureBody transform here in the Inspector

    [Header("Leg Generation")]
    public int numberOfLegs = 2;
    public float legLength = 2.0f; // Total length from hip to foot when fully extended
    public float upperLegRatio = 0.5f; // Proportion of total length for the upper leg (0.5 = knee in middle)
    public Vector3 hipOffset = new Vector3(0.5f, -0.5f, 0f); // Offset from body center to hip (adjust x for spread)
    public Material legMaterial; // Optional material for the legs

    [Header("Walking Animation")]
    public float stepHeight = 0.5f;
    public float stepLength = 1.0f;
    public float stepSpeed = 2.0f;
    public float bodyTargetHeight = 1.5f; // Desired height of the body center above the foot's resting plane

    // --- Private Variables ---
    private List<LegInfo> legs = new List<LegInfo>();
    private float upperLegLength;
    private float lowerLegLength;

    // Structure to hold information for each leg
    private class LegInfo
    {
        public Transform hipAttachment; // Point on the body where the leg starts
        public Vector3 restingFootPositionWorld; // Neutral position when standing still
        public Vector3 targetFootPositionWorld; // Where the foot should aim for
        public Vector3 currentFootPositionWorld; // Current actual position (smoothed)
        public Vector3 kneePositionWorld; // Calculated knee position
        public float stepPhaseOffset; // To alternate leg movements

        // Visuals
        public GameObject upperLegVisual;
        public GameObject lowerLegVisual;
    }

    void Start()
    {
        if (body == null) body = transform; // Default to the object this script is on

        upperLegLength = legLength * upperLegRatio;
        lowerLegLength = legLength * (1.0f - upperLegRatio);

        GenerateLegs();
    }

    void GenerateLegs()
    {
        legs.Clear(); // Clear any existing legs if regenerated

        for (int i = 0; i < numberOfLegs; i++)
        {
            LegInfo newLeg = new LegInfo();

            // --- Create Hip Attachment Point ---
            GameObject hipGO = new GameObject($"Leg_{i}_Hip");
            newLeg.hipAttachment = hipGO.transform;
            newLeg.hipAttachment.SetParent(body);

            // Position hips based on index (simple alternating side offset)
            float sideSign = (i % 2 == 0) ? 1f : -1f;
            Vector3 legHipOffset = new Vector3(hipOffset.x * sideSign, hipOffset.y, hipOffset.z);
            newLeg.hipAttachment.localPosition = legHipOffset;

            // --- Calculate Resting Foot Position ---
            // Start directly below the hip at the target body height distance
            newLeg.restingFootPositionWorld = newLeg.hipAttachment.position - (Vector3.up * bodyTargetHeight);
            newLeg.targetFootPositionWorld = newLeg.restingFootPositionWorld;
            newLeg.currentFootPositionWorld = newLeg.restingFootPositionWorld;

            // --- Create Leg Visuals (Capsules) ---
            newLeg.upperLegVisual = CreateLegSegment($"Leg_{i}_Upper", legMaterial);
            newLeg.lowerLegVisual = CreateLegSegment($"Leg_{i}_Lower", legMaterial);

            // --- Set Step Phase Offset ---
            // Distribute phases evenly over a full cycle (2 * PI)
            newLeg.stepPhaseOffset = (2 * Mathf.PI / numberOfLegs) * i;

            legs.Add(newLeg);
        }
        // Initial update of visuals based on resting pose
        UpdateLegVisuals();
    }

    GameObject CreateLegSegment(string name, Material material)
    {
        GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        segment.name = name;
        Destroy(segment.GetComponent<Collider>()); // Remove collider for simplicity
        if (material != null)
        {
            segment.GetComponent<Renderer>().material = material;
        }
        // We will scale and position these later
        segment.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f); // Default thickness, height set later
        return segment;
    }


    void Update()
    {
        UpdateTargetFootPositions();
        SolveIKAndPositionLegs();
        UpdateLegVisuals();
    }

    void UpdateTargetFootPositions()
    {
        float time = Time.time * stepSpeed;

        for (int i = 0; i < legs.Count; i++)
        {
            LegInfo leg = legs[i];

            // Calculate the base resting position relative to the *current* hip position
            Vector3 currentRestingPos = leg.hipAttachment.position - (body.up * bodyTargetHeight);

            // Calculate step cycle phase for this leg
            float phase = time + leg.stepPhaseOffset;

            // Calculate forward/backward movement (Sine wave)
            float horizontalDisp = Mathf.Sin(phase) * stepLength * 0.5f; // 0.5 because range is -1 to 1

            // Calculate vertical movement (Lift foot during half the cycle)
            // Use Cosine so peak is at phase 0, PI, 2PI etc. We want lift when Sin is positive (0 to PI)
            float verticalDisp = Mathf.Max(0, Mathf.Cos(phase - (Mathf.PI / 2.0f))) * stepHeight; // Shifted Cosine

            // Combine displacements relative to body's forward direction
            Vector3 displacement = (body.forward * horizontalDisp) + (body.up * verticalDisp);

            // Update the target position
            leg.targetFootPositionWorld = currentRestingPos + displacement;

            // Simple smoothing / interpolation towards target (optional, but looks better)
            leg.currentFootPositionWorld = Vector3.Lerp(leg.currentFootPositionWorld, leg.targetFootPositionWorld, Time.deltaTime * 15.0f);
        }
    }


    void SolveIKAndPositionLegs()
    {
        foreach (LegInfo leg in legs)
        {
            Vector3 hipPos = leg.hipAttachment.position;
            Vector3 footPos = leg.currentFootPositionWorld; // Use the smoothed position
            Vector3 hipToFoot = footPos - hipPos;
            float distance = hipToFoot.magnitude;

            // Check reachability
            if (distance > upperLegLength + lowerLegLength)
            {
                // Target too far: Stretch leg fully towards the target
                leg.kneePositionWorld = hipPos + hipToFoot.normalized * upperLegLength;
                // Clamp foot position to max reach
                leg.currentFootPositionWorld = hipPos + hipToFoot.normalized * (upperLegLength + lowerLegLength);
                footPos = leg.currentFootPositionWorld; // Update footPos for visual calculation below
                distance = upperLegLength + lowerLegLength; // Update distance
            }
            else if (distance < Mathf.Abs(upperLegLength - lowerLegLength))
            {
                // Target too close: Place knee along the hip->foot line based on lengths
                // This case is less common in walking but good to handle.
                // A simple approach is to extend the knee slightly outwards from the hip-foot line.
                // For now, let's just place it on the line, slightly offset from hip
                leg.kneePositionWorld = hipPos + hipToFoot.normalized * upperLegLength;
            }
            else
            {
                // Target is reachable - Use Law of Cosines to find knee position
                float angle1 = Mathf.Acos((distance * distance + upperLegLength * upperLegLength - lowerLegLength * lowerLegLength) / (2 * distance * upperLegLength));
                //float angle2 = Mathf.Acos((lowerLegLength*lowerLegLength + upperLegLength*upperLegLength - distance*distance) / (2 * lowerLegLength * upperLegLength)); // Angle at Knee


                // Determine the plane of bending. We need a vector perpendicular to hipToFoot.
                // A common choice is based on the body's orientation. Let's bend outwards using body.right relative to hip.
                Vector3 sideVector = Vector3.Cross(hipToFoot, body.up).normalized; // Default bend plane based on body up
                if (leg.hipAttachment.localPosition.x < 0) sideVector *= -1; // Bend left leg left, right leg right

                // If hipToFoot is aligned with body.up, Cross product is zero. Choose a default side like body.right.
                if (sideVector == Vector3.zero)
                {
                    sideVector = leg.hipAttachment.right; // Use hip's local right
                }


                // Calculate the knee position by rotating a point upperLegLength away from the hip
                // around the 'sideVector' axis by 'angle1' radians.
                leg.kneePositionWorld = hipPos + Quaternion.AngleAxis(angle1 * Mathf.Rad2Deg, sideVector) * (hipToFoot.normalized * upperLegLength);

                // Alternative knee bend direction (e.g., always bend "forward" relative to body)
                // Vector3 forwardBendAxis = Vector3.Cross(hipToFoot, body.right);
                // leg.kneePositionWorld = hipPos + Quaternion.AngleAxis(angle1 * Mathf.Rad2Deg, forwardBendAxis) * (hipToFoot.normalized * upperLegLength);
            }
        }
    }

    void UpdateLegVisuals()
    {
        foreach (LegInfo leg in legs)
        {
            // --- Position and Orient Upper Leg ---
            Vector3 hipPos = leg.hipAttachment.position;
            Vector3 kneePos = leg.kneePositionWorld;
            Vector3 upperLegVector = kneePos - hipPos;
            float upperSegmentActualLength = upperLegVector.magnitude;

            if (upperSegmentActualLength > 0.01f) // Avoid zero vector issues
            {
                leg.upperLegVisual.transform.position = hipPos + (upperLegVector * 0.5f); // Position at midpoint
                leg.upperLegVisual.transform.up = upperLegVector.normalized; // Align capsule's Y-axis (up) with the segment vector
                // Scale the capsule height (Y-axis for default Capsule primitive)
                // Capsule height includes the half-sphere caps, so total height = cylinder height + radius*2
                // Assuming radius is fixed (e.g. based on x/z scale 0.1), height needs adjustment.
                // For simplicity, let's approximate: Scale Y to match length. Default capsule height is 2 units.
                leg.upperLegVisual.transform.localScale = new Vector3(
                    leg.upperLegVisual.transform.localScale.x,
                    upperSegmentActualLength / 2.0f, // Divide by 2 because default capsule height is 2
                    leg.upperLegVisual.transform.localScale.z);
            }

            // --- Position and Orient Lower Leg ---
            Vector3 footPos = leg.currentFootPositionWorld;
            Vector3 lowerLegVector = footPos - kneePos;
            float lowerSegmentActualLength = lowerLegVector.magnitude;

            if (lowerSegmentActualLength > 0.01f)
            {
                leg.lowerLegVisual.transform.position = kneePos + (lowerLegVector * 0.5f); // Position at midpoint
                leg.lowerLegVisual.transform.up = lowerLegVector.normalized; // Align capsule's Y-axis (up)
                leg.lowerLegVisual.transform.localScale = new Vector3(
                    leg.lowerLegVisual.transform.localScale.x,
                    lowerSegmentActualLength / 2.0f, // Divide by 2 because default capsule height is 2
                    leg.lowerLegVisual.transform.localScale.z);
            }
        }
    }

    // Optional: Draw Gizmos in the editor to visualize points
    void OnDrawGizmos()
    {
        if (legs == null || legs.Count == 0) return;

        foreach (LegInfo leg in legs)
        {
            if (leg.hipAttachment != null)
            {
                Gizmos.color = Color.yellow; // Target Foot Position
                Gizmos.DrawSphere(leg.targetFootPositionWorld, 0.1f);

                Gizmos.color = Color.cyan; // Smoothed/Current Foot Position
                Gizmos.DrawSphere(leg.currentFootPositionWorld, 0.1f);

                Gizmos.color = Color.red; // Knee Position
                Gizmos.DrawSphere(leg.kneePositionWorld, 0.08f);

                Gizmos.color = Color.green; // Hip Position
                Gizmos.DrawSphere(leg.hipAttachment.position, 0.08f);

                // Draw lines for legs if visuals aren't generated yet or for clarity
                if (leg.upperLegVisual == null || leg.lowerLegVisual == null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(leg.hipAttachment.position, leg.kneePositionWorld);
                    Gizmos.DrawLine(leg.kneePositionWorld, leg.currentFootPositionWorld);
                }
            }
        }
    }
}