using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    // Storing all walls that will be moved across screen.
    private List<GameObject> Walls = new List<GameObject>();

    private float scrollRate = 0.05f; // In units per frame

    // If specified, every ten seconds the scroll rate will increase by this amount.
    private float rateOfscrollIncrease = 0.01f;

    public static GameManager instance;

    public float getScrollRate()
    {
        return scrollRate;
    }

    public void addWall(GameObject wall)
    {
        Walls.Add(wall);
    }

	// Use this for initialization
	void Awake () {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
	}

    	
	// Update is called once per frame
	void FixedUpdate () {
		foreach (GameObject wall in Walls)
        {
            Vector3 currentPos = wall.transform.position;
            currentPos.x -= scrollRate;
            wall.transform.position = currentPos;
        }
	}
}
