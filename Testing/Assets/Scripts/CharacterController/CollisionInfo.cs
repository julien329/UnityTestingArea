using UnityEngine;
using System.Collections;

public class CollisionInfo {

    public bool above, below;
    public bool left, right;
    public bool climbingSlope, descendingSlope;

    public float slopeAngle, slopeAngleOld;
    public Vector3 velocityOld;


    public void Reset(Vector3 velocity) {
        above = below = false;
        left = right = false;
        climbingSlope = descendingSlope = false;

        velocityOld = velocity;
        slopeAngleOld = slopeAngle;
        slopeAngle = 0;
    }
}
