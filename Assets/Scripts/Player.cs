using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    private bool toMoveUp = false;

    // Fixed update called every 0.02 seconds.
    private int succesiveFramesTouched = 0;
    private int succesiveFramesUnTouched = 0;
    private const int FRAMES_TO_REACH_TOP = 70;
    private float velocityUpSlope;



	// Use this for initialization
	void Start () {
        // Get the velocity up slope by solving for the integral equal to total units the player has to travel vertically
        velocityUpSlope = 9.22f / ((Mathf.Pow(FRAMES_TO_REACH_TOP, 2) / 2.0f) - Mathf.Pow(FRAMES_TO_REACH_TOP, 2));
		
	}

	
	// Update is called once per frame
	void FixedUpdate () {
        if (Input.anyKey)
            toMoveUp = true;
        else
            toMoveUp = false;

        move(toMoveUp);
	}


    private float upMovementAmount(int succesive_frames_touched)
    {
        // Just the derivative of the position function (to travel 9.22 units over specified frames)
        // Slope solved by getting the integral to equal 9.22

        // Return the position in units
        if (succesiveFramesTouched > 50)
            return 0;

        return velocityUpSlope * (succesiveFramesTouched - FRAMES_TO_REACH_TOP);
    }


    private float downMovementAmount(int successive_frames_untouched)
    {
        // Get these values once succesive frames touched is zero. (Player falling)
        return 3.0f / 200 * (461.0f / 1250) * successive_frames_untouched;
    }


    /*
     * Called every frame to update the player position
     * bool pressed: if true, the play should move up 
     */
    void move(bool pressed)
    {
        if (pressed)
        {
            succesiveFramesUnTouched = 0;

            // Increase height
            Vector3 newPos = transform.position;
            newPos.y = Mathf.Min(newPos.y + upMovementAmount(succesiveFramesTouched), 5);
            transform.position = newPos;

            succesiveFramesTouched++;
        }
        else
        {
            succesiveFramesTouched = 0;

            // Decrease height
            Vector3 newPos = transform.position;
            newPos.y = Mathf.Max(newPos.y - downMovementAmount(succesiveFramesUnTouched), -4.22f);
            transform.position = newPos;

            succesiveFramesUnTouched++;
        }
    }



}
