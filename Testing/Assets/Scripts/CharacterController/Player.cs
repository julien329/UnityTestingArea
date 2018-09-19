using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Collider2D))]
public class Player : MonoBehaviour {

    public float moveSpeed = 7f;
    public float wallSlideSpeed = 3f;

    public float wallJumpForceXMultiplier = 0.6f;
    public float wallJumpForceYMultiplier = 1.2f;
    public float timeToJumpApex = 0.35f;
    public float jumpHeight = 3f;
    public float accelerationTimeAirborne = 0.2f;
    public float accelerationTimeGrounded = 0.1f;
    public uint jumpCountMax = 1;

    float gravity;
    float jumpVelocity;
    float targetVelocityX;
    float velocityXSmoothing;
    uint jumpCount = 0;

    Vector3 velocity;
    Controller2D controller;


    void Start () {
        controller = GetComponent<Controller2D>();

        // Formula : deltaMovement = velocityInitial * time + (acceleration * time^2) / 2  -->  where acceleration = gravity and velocityInitial is null
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        // Formula : velocityFinal = velocityInitial + acceleration * time  -->  where velocityFinal = jumpVelocity and velocityInitial is null
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
	}
	

	void Update () {
        // If there is a collision in Y axis, reset velocity
        if (controller.collisions.above || controller.collisions.below) {
            velocity.y = 0;
        }

        if (controller.collisions.below) {
            jumpCount = 0;
        }

        // Get player input in raw states
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Set target velocity according to user input
        float targetVelocityX = input.x * moveSpeed;
        // Smooth velocity (use acceleration). Change smoothing value if grounded or airborne
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);

        // If the jump key is pressed
        if (Input.GetKeyDown(KeyCode.Space)) {
            // If the input is facing the colliding wall, jump up and in the opposite direction 
            if (!controller.collisions.below && ((controller.collisions.right && input.x > 0) || (controller.collisions.left && input.x < 0))) {
                velocity.x = -Mathf.Sign(input.x) * wallJumpForceXMultiplier * jumpVelocity;
                velocity.y = wallJumpForceYMultiplier * jumpVelocity;
            }
            // Else jump if there is an available jump left
            else if (jumpCount < jumpCountMax) {
                velocity.y = jumpVelocity;
                jumpCount++;
            }
        }

        // Add gravity force downward to Y velocity
        velocity.y += gravity * Time.deltaTime;

        // If the input is facing the colliding wall and the player is going down, slide on the wall at clamped velocity.
        if (velocity.y < 0 && !controller.collisions.below && ((controller.collisions.right && input.x > 0) || (controller.collisions.left && input.x < 0))) {
            velocity.y = Mathf.Max(velocity.y, -wallSlideSpeed);
        }

        // If speed too small, set to null
        if (Mathf.Abs(velocity.x) < 0.1f) {
            velocity.x = 0f;
        }

        // Call move to check collisions and translate the player
        controller.Move(velocity * Time.deltaTime);
    }
}