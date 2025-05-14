using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager main;

    public Transform[][] pathGroups;
    public Transform startPoint;  // Add this field to represent the starting point

    void Awake()
    {
        main = this;

        int numPaths = transform.childCount;
        pathGroups = new Transform[numPaths][];

        for (int i = 0; i < numPaths; i++)
        {
            Transform pathParent = transform.GetChild(i);
            int count = pathParent.childCount;
            pathGroups[i] = new Transform[count];

            for (int j = 0; j < count; j++)
            {
                pathGroups[i][j] = pathParent.GetChild(j);
            }
        }

        // Ensure a startPoint is set for spawning enemies
        if (startPoint == null)
        {
            Debug.LogWarning("Start point not set in PathManager. Please assign a start point in the inspector.");
        }
    }

    public Transform[] GetRandomPath()
    {
        return pathGroups[Random.Range(0, pathGroups.Length)];
    }
}
