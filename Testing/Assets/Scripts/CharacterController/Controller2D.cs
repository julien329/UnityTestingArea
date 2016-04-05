using UnityEngine;
using System.Collections;

[RequireComponent (typeof(BoxCollider2D))]
public class Controller2D : MonoBehaviour {

    public LayerMask collisionMask;
    public float skinWidth = 0.015f;
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public float maxClimbAngle = 60f;
    public float maxDescendAngle = 60f;

    float horizontalRaySpacing;
    float verticalRaySpacing;

    BoxCollider2D playerCollider;
    RaycastOrigins raycastOrigins;
    public CollisionInfo collisions;


    void Start () {
        playerCollider = GetComponent<BoxCollider2D>();
        collisions = new CollisionInfo();
        CalculateRaySpacing();
    }
	

    // Move player with requested velocity
    public void Move(Vector3 velocity) {
        // Get new Raycast origins, reset collision infos and save old velocity
        UpdateRaycastOrigins();
        collisions.Reset();
        collisions.velocityOld = velocity;

        // If player going down and moving horizontaly, check for descending slopes
        if(velocity.y < 0 && velocity.x != 0)
            DescendSlope(ref velocity);

        // If player moving horizontally, check horizontal collisions
        if (velocity.x != 0)
            HorizontalCollisions(ref velocity);
        // If player moving horizontally, check vertical collisions
        if (velocity.y != 0)
            VerticalCollisions(ref velocity);

        // Translate player by the amount specified by the final velocity value
        transform.Translate(velocity);
    }

    
    // Check for horizontal collisions, and modifies velocity accordingly
    void HorizontalCollisions(ref Vector3 velocity) {
        // Get direction sign in x axis
        float directionX = Mathf.Sign(velocity.x);
        // Calculate needed rayLength with requested velocity (distance) and skinWitdh
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        // For every horizontal ray...
        for (int i = 0; i < horizontalRayCount; i++) {
            // Find starting point according to direction
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            // Add distance offset beween each ray
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            // Cast ray with collisonMask looking or specific layer
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            // If the Raycast hit something...
            if (hit) {
                // Calculate slope angle using normal vector
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                // If it is the first ray (in front on player) and slope is climbable...
                if (i == 0 && slopeAngle <= maxClimbAngle && slopeAngle != 0) {
                    // If previously was on a descending slope...
                    if (collisions.descendingSlope) {
                        // Reset old velocity (remove slowdowns) and set bool to false
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    float distanceToSlopeStart = 0;
                    // If climbing a new slope...
                    if (slopeAngle != collisions.slopeAngleOld) {
                        // Save initial offset distance of detection
                        distanceToSlopeStart = hit.distance - skinWidth;
                        // Substract velocity with offset (stick the player to the slope during calculations)
                        velocity.x -= distanceToSlopeStart * directionX;
                    }
                    // Start climbin the slope
                    ClimbSlope(ref velocity, slopeAngle);
                    // Add back the offset
                    velocity.x += distanceToSlopeStart * directionX;
                }

                // If not climbing or of slope angle to big...
                if (!collisions.climbingSlope || slopeAngle > maxClimbAngle) {
                    // Stop the player from going further than the hit distance
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    // Set current hit distance as max length for following rays (prevent higher velocity)
                    rayLength = hit.distance;

                    // If climbing a slope...
                    if (collisions.climbingSlope) {
                        // Set y axis velocity according to the new x axis velocity (prevent clipping in object above the player)
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }
                
                    // Set collisions bool for left and right
                    collisions.left = (directionX == -1);
                    collisions.right = (directionX == 1);
                }
            }
        }
    }


    // Check for vertical collisions, and modifies velocity accordingly
    void VerticalCollisions(ref Vector3 velocity) {
        // Get direction sign in y axis
        float directionY = Mathf.Sign (velocity.y);
        // Calculate needed rayLength with requested velocity (distance) and skinWitdh
        float rayLength = Mathf.Abs (velocity.y) + skinWidth;

        // For every vertical ray...
		for (int i = 0; i < verticalRayCount; i ++) {
            // Find starting point according to direction
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            // Add distance offset beween each ray
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);
            // Cast ray with collisonMask looking or specific layer
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            // If the Raycast hit something...
            if (hit) {
                // Stop the player from going further than the hit distance
                velocity.y = (hit.distance - skinWidth) * directionY;
                // Set current hit distance as max length for following rays (prevent higher velocity)
                rayLength = hit.distance;

                // If climbing a slope...
                if(collisions.climbingSlope) {
                    // Set x axis velocity according to the new y axis velocity (prevent clipping in object in front of the player)
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }

                // Set collisions bool for above and below
                collisions.below = (directionY == -1);
                collisions.above = (directionY == 1);
            }
        }

        // If climbing a slope... (we look for this in order to smooth transition between two different slopes)
        if (collisions.climbingSlope) {
            // Get direction sign in X axis
            float directionX = Mathf.Sign(velocity.x);
            // Calculate needed rayLength with requested velocity (distance) and skinWitdh
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            // Find first ray origin in the direction the player is moving
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            // Cast ray with collisonMask looking or specific layer
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            // If the Raycast hit something...
            if (hit) {
                // Calculate slope angle using hit normal angle
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                // If the angle is different than the last angle (this is a transition)...
                if(slopeAngle != collisions.slopeAngle) {
                    // Set reset velocity in x axis to fit the new slope (prevent brief clipping during transition)
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    // Save new slope angle
                    collisions.slopeAngle = slopeAngle;
                }
            }
        }
    }


    // Climb the slope
    void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
        // Get move distance and calculate climb velocity in Y axis using trigonometry
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        // In Y axis, if velocity is less than climb velocity (not application when jumping)...
        if(velocity.y <= climbVelocityY) {
            // Set y velocity
            velocity.y = climbVelocityY;
            // Calculate climb velocity in X axis using trigonometry
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            // Set collisions bool and save slope angle
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }


    // Descend the slope
    void DescendSlope(ref Vector3 velocity) {
        // Get direction sign in X axis
        float directionX = Mathf.Sign(velocity.x);
        // Find last ray origin in the direction the player is moving
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        // Cast ray with collisonMask looking or specific layer
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        // If the Raycast hit something...
        if (hit) {
            // Calculate slope angle using hit normal angle
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            // If the slope angle is not too big or null...
            if(slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
                // If the slope is descending the same direction the player is moving...
                if(Mathf.Sign(hit.normal.x) == directionX) {
                    // If the player is close enough to the slope (not just falling down)...
                    if(hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
                        // Get move distance and calculate descend velocity in Y axis using trigonometry
                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        // Calculate descend velocity in X axis using trigonometry
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        // Decrement y velocity to stick on the surface
                        velocity.y -= descendVelocityY;

                        // Save slope angle and set collisions bool
                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                    }
                }
            }
        }
    }


    // Update the rayCast origin points (world coord)
    void UpdateRaycastOrigins() {
        // Get player collider bounds and substract skinWidth on each side
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(-2 * skinWidth);

        // Set min/max origin points
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
    }


    // Calculate need spacing between rays according to the number of ray. 
    void CalculateRaySpacing() {
        // Get player collider bounds and substract skinWidth on each side
        Bounds bounds = playerCollider.bounds;
        bounds.Expand(-2 * skinWidth);

        // Clamp the rayCount with a minimal value
        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calculate required spacing between rays
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }


    // Saved raycast origin points
    struct RaycastOrigins {
        public Vector2 topLeft, topRight;
        public Vector2 bottomLeft, bottomRight;
    }
}
