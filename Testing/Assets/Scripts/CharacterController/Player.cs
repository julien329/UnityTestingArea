using UnityEngine;
using System.Collections;

[RequireComponent (typeof(Collider2D))]
public class Player : MonoBehaviour {

    public float moveSpeed = 6;
    public float timeToJumpApex = 0.25f;
    public float jumpHeight = 4f;
    public float accelerationTimeAirborne;
    public float accelerationTimeGrounded;

    float gravity;
    float jumpVelocity;
    float targetVelocityX;
    float velocityXSmoothing;

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

        // Get player input in raw states
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Get jump input (space) to jump if touching ground below
        if(Input.GetKeyDown(KeyCode.Space) && controller.collisions.below) {
            velocity.y = jumpVelocity;
        }

        // Set target velocity according to user input
        float targetVelocityX = input.x * moveSpeed;
        // Smooth velocity (use acceleration). Change smoothing value if grounded or airborne
        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        // Add gravity force downward to Y velocity
        velocity.y += gravity * Time.deltaTime;
        // Call move to check collisions and translate the player
        controller.Move(velocity * Time.deltaTime);
    }
}
