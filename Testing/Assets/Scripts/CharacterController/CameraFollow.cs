using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

    public Vector3 cameraOffset;
    public float followSpeed = 7f;

    Transform player;
    Vector3 targetPosition;


	void Start () {
        player = GameObject.Find("Player").transform;
	}
	

	void Update () {
        targetPosition = player.position + cameraOffset;

        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
	}
}
