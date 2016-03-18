using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour {

    private Vector3 move;
    private Rigidbody playerRigidbody;

    public float speed = 6.0f;

    // Use this for initialization
    void Start () {
        playerRigidbody = GetComponent<Rigidbody>();
    }
	
	// Update is called once per frame
	void Update () {
        Move();
    }

    private void Move() {
        // Get vertical/horizontal input value with wasd or arrows
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Set the movement vector based on the axis input.
        move.Set(h, 0f, v);

        // Move current position using ClampMagnitude to normalize the diagonal run speed.
        playerRigidbody.MovePosition(transform.position + Vector3.ClampMagnitude(move, 1.0f) * speed * Time.deltaTime);
    }
}
