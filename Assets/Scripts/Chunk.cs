using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk : MonoBehaviour {

    // The amount by which walls will be away from the generated path. (Larger value = easier)
    private float OffsetThreshold;

    // Units of the horizontal length of the chunk. Will depend on the length of all paths summed.
    private float Length;

    // How many walls per chunk. Units to be defined.
    private float Density;

    // Constant which when increased, will decrease the length of paths produced, thus higher values will mean harder gameplay.
    private float PathLengthDifficulty;

    // The max angle a path can proceed.
    private float MaxIncline;

    // The point which the path should start proceeding in this chunk.
    private Vector2 StartPoint;

    private const int PATHS_PER_CHUNK = 8;
    private const float MAX_WALL_HEIGHT = 3.5f;
    private const float BOTTOM_PLAYER_POS = -4.22f;
    private const float TOP_PLAYER_POS = 5f;

    private List<Vector2> ProceedingPathPoints;
    private List<Vector2> OffsetPathPointsUp;
    private List<Vector2> OffsetPathPointsDown;
    private List<List<Vector2>> WallPoints;
    private List<GameObject> Walls = new List<GameObject>();


    public Vector2 getEndPoint()
    {
        Vector2 lastPoint = ProceedingPathPoints[ProceedingPathPoints.Count - 1];
        return new Vector2(lastPoint.x, lastPoint.y);
    }


    public List<Vector2> getProceedingPathPoints()
    {
        return new List<Vector2>(ProceedingPathPoints);
    }
    public List<Vector2> getProceedingPathPointsUP()
    {
        return new List<Vector2>(OffsetPathPointsUp);
    }
    public List<Vector2> getProceedingPathPointsDOWN()
    {
        return new List<Vector2>(OffsetPathPointsDown);
    }


    public Chunk(float offsetThreshold, float pathLengthDifficulty, float maxIncline, Vector2 startPoint)
    {
        this.OffsetThreshold = offsetThreshold;
        this.PathLengthDifficulty = pathLengthDifficulty;
        this.MaxIncline = maxIncline;
        this.StartPoint = new Vector2(startPoint.x, startPoint.y);

        ProceedingPathPoints = GenerateProceedingPath();
        OffsetPathPointsUp = getOffsetPoints(OffsetThreshold);
        OffsetPathPointsDown = getOffsetPoints(-OffsetThreshold);
        WallPoints = generateWalls();
    }


    /*
     Returns a non-linear random function that selects more upper-middle ranged values. randomX should be chosen between zero and one.
     Polynomial was generated with 20 as the maximum length of a path.
    */
    private float getRandomPathLength(float randomX)
    {
        return (-460.0f / 21) * randomX * randomX * randomX + (174.0f / 7) * randomX * randomX + (190.0f / 21) * randomX + 8 - PathLengthDifficulty;
    }


    /**
     * Given the minAngle and max angle, return an angle for the path to lead from the last point generated
     */
    private float getRandomAngle(float minAngle, float maxAngle)
    {
        Random.InitState((int)System.DateTime.Now.Ticks);
        return UnityEngine.Random.Range(minAngle, maxAngle);
    }


    /**
     * Returns the minimum angle possible with the chosen path length
     */
    private float getMinAngle(float pathLength, float lastPointy)
    {
        float theta = Mathf.Atan((lastPointy - BOTTOM_PLAYER_POS) / pathLength);

        return Mathf.Max(-theta, -MaxIncline);
    }


    /**
     * Returns the maximum angle possible with the chosen path length
     */
    private float getMaxAngle(float pathLength, float lastPointy)
    {
        float theta = Mathf.Atan((TOP_PLAYER_POS - lastPointy) / pathLength);

        return Mathf.Min(theta, MaxIncline);
    }


    /**
     * Return a list of the generated points the player should proceed on.
     */
    private List<Vector2> GenerateProceedingPath()
    {
        List<Vector2> paths = new List<Vector2>();
        paths.Add(StartPoint);
        Vector2 lastPoint = StartPoint;

        for (int i = 0; i < PATHS_PER_CHUNK; i++)
        {
            float pathLength = getRandomPathLength(UnityEngine.Random.Range(0, 1));
            float randomTheta = getRandomAngle(getMinAngle(pathLength, lastPoint.y), getMaxAngle(pathLength, lastPoint.y));

            // Get the values from polar coordinates. x = rcos@ and y = rsin@
            Vector2 nextSpot = new Vector2(lastPoint.x + pathLength * Mathf.Cos(randomTheta), lastPoint.y + pathLength * Mathf.Sin(randomTheta));

            paths.Add(nextSpot);
            lastPoint = nextSpot;
        }

        return paths;
    }


    /*
     * Returns the path points but offset by a value. 
     */
    private List<Vector2> getOffsetPoints(float offsetAmount)
    {
        List<Vector2> offsetPoints = new List<Vector2>();

        if (this.ProceedingPathPoints == null || this.ProceedingPathPoints.Count == 0)
            throw new System.ArgumentNullException("Cannot generate an offset path if main path is not generated.");

        foreach (Vector2 p in ProceedingPathPoints)
        {
            offsetPoints.Add(new Vector2(p.x, p.y + offsetAmount));
        }

        return offsetPoints;
    }

    /*
     * Given two points, align a wall to fit them. Instantiate a game object and scale and set rotation properly.
     */
    private void spawnWall(Vector2 p1, Vector2 p2)
    {
        Vector2 position = new Vector2((p1.x + p2.x) / 2, (p1.y + p2.y) / 2);
        Vector2 diff = p1 - p2;
        // Angle is inverse tangent of the vector between the points.
        float rotation = Mathf.Atan(diff.y / diff.x);

        GameObject wall = Instantiate(MapGeneration.mapGeneration.wall, position, Quaternion.identity) as GameObject;
        wall.transform.localScale = new Vector3(Vector2.Distance(p1, p2), 0.35f, 1);
        wall.transform.Rotate(Vector3.forward, rotation*Mathf.Rad2Deg);
        Walls.Add(wall);
    }


    /*
     * Generate walls that attach to the offset paths but do not cross the main path. Also instantiate all wall game objects that will be used.
     */
    private List<List<Vector2>> generateWalls()
    {
        List<List<Vector2>> wallsCreated = new List<List<Vector2>>();

        if (this.ProceedingPathPoints == null || this.ProceedingPathPoints.Count == 0)
            throw new System.ArgumentNullException("Cannot generate walls if no path is created");

        // For starting, iterate through paths twice at a time and add a wall on both upper offset and lower offset
        // if the min of both wall points is less than the intersection of the path, add vertcal wall (in the case of the upper offset path)
        for (int i = 0; i < ProceedingPathPoints.Count - 1; i += 2)
        {
            // Get firsts vector multiplied by a random scalar
            System.Random ran = new System.Random();
            Vector2 upleft = OffsetPathPointsUp[i] - OffsetPathPointsUp[i + 1];
            upleft *= (ran.Next(1000) / 1000.0f);
            upleft += OffsetPathPointsUp[i + 1];
            Vector2 upRight = OffsetPathPointsUp[i + 1] - OffsetPathPointsUp[i + 2];
            upRight *= (ran.Next(1000) / 1000.0f);
            upRight += OffsetPathPointsUp[i + 2];
            spawnWall(upleft, upRight);
            List<Vector2> wall = new List<Vector2>();
            wall.AddRange(new Vector2[] { upleft, upRight });
            wallsCreated.Add(wall);
        }


        return wallsCreated;
    }
}
