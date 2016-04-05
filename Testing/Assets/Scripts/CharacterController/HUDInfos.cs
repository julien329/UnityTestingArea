using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HUDInfos : MonoBehaviour {

    Text below, above;
    Text left, right;
    Text climbing, descending;

    Controller2D playerController;


	void Start () {
        below = GameObject.Find("Below").GetComponent<Text>();
        above = GameObject.Find("Above").GetComponent<Text>();
        left = GameObject.Find("Left").GetComponent<Text>();
        right = GameObject.Find("Right").GetComponent<Text>();
        climbing = GameObject.Find("Climbing").GetComponent<Text>();
        descending = GameObject.Find("Descending").GetComponent<Text>();

        playerController = GameObject.Find("Player").GetComponent<Controller2D>();
    }
	

	void Update () {
        below.text = (playerController.collisions.below) ? "Below: True" : "Below: False";
        above.text = (playerController.collisions.above) ? "Above: True" : "Above: False";
        left.text = (playerController.collisions.left) ? "Left: True" : "Left: False";
        right.text = (playerController.collisions.right) ? "Right: True" : "Right: False";

        climbing.text = (playerController.collisions.climbingSlope) ? "Climbing: True" : "Climbing: False";
        descending.text = (playerController.collisions.descendingSlope) ? "Descending: True" : "Descending: False";
    }
}
