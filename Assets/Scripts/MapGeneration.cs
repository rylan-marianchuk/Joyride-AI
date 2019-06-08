using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGeneration : MonoBehaviour {

    public GameObject wall;
    public static MapGeneration mapGeneration;

    private const float BOTTOM_PLAYER_POS = -4.22f;
    private const float TOP_PLAYER_POS = 5f;

    // Increasing this will make the generation of path length smaller and thus more difficult.
    private const float DIFFICULTY_PATH_LENGTH = 0;
    // The max angle of increase the player route can take.
    private const float MAX_INCLINE = Mathf.PI / 5;
    // The gap between the farthest a wall can reach to the generated proceeding path.
    private const float OFFSET_THRESHOLD = 2.25f;

    // The point of the path that was last generated. Next chunk generation needs to start at this point
    private Vector2 LastPathPoint;

    private List<Chunk> Chunks = new List<Chunk>();



	void Start () {
        if (mapGeneration == null)
            mapGeneration = this;
        else
            Destroy(this);
		//// Use Object pooling for the wall generation. Start with 20 walls.
  //      for (int i = 0; i < 20; i++)
  //      {
  //          GameManager.instance.addWall(wall);
  //      }

        // Create the first one at the edge of screen
        LastPathPoint = new Vector2(10, UnityEngine.Random.Range(BOTTOM_PLAYER_POS, TOP_PLAYER_POS));

        // Generate first buffer, will always keep a buffer of 40 units
        Chunks.Add(GenerateChunk());

        List<Vector2> firstChunkPoints = Chunks[0].getProceedingPathPoints();
        List<Vector2> firstChunkPointsOFFUP = Chunks[0].getProceedingPathPointsUP();
        List<Vector2> firstChunkPointsOFFDOWN = Chunks[0].getProceedingPathPointsDOWN();
        for (int i = 0; i < firstChunkPoints.Count - 1; i++)
        {
            Debug.DrawLine(firstChunkPoints[i], firstChunkPoints[i + 1], Color.red, 10000f);
            Debug.DrawLine(firstChunkPointsOFFUP[i], firstChunkPointsOFFUP[i + 1], Color.red, 10000f);
            Debug.DrawLine(firstChunkPointsOFFDOWN[i], firstChunkPointsOFFDOWN[i + 1], Color.red, 10000f);
        }
	}


    private Chunk GenerateChunk()
    {
        Chunk newChunk = new Chunk(OFFSET_THRESHOLD, DIFFICULTY_PATH_LENGTH, MAX_INCLINE, LastPathPoint);
        LastPathPoint = newChunk.getEndPoint();
        return newChunk;
    }
}
