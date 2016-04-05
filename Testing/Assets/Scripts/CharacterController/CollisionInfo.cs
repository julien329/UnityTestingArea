using UnityEngine;
using System.Collections;

public class CollisionInfo {

    public bool above, below;
    public bool left, right;

    public bool climbingSlope, descendingSlope;
    public float slopeAngle, slopeAngleOld;
    public Vector3 velocityOld;

    public void Reset() {
        above = below = false;
        left = right = false;
        climbingSlope = descendingSlope = false;

        slopeAngleOld = slopeAngle;
        slopeAngle = 0;
    }
}
