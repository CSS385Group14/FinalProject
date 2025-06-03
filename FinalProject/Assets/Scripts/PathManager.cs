using UnityEngine;

public class PathManager : MonoBehaviour
{
    public static PathManager main;

    public Transform[][] pathGroups;
    [SerializeField] private Transform[] startPoints;

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

    }

    public Transform[] GetRandomPath()
    {
        return pathGroups[Random.Range(0, pathGroups.Length)];
    }
    public Transform GetRandomStartPoint()
    {
        if (startPoints == null || startPoints.Length == 0)
        {
            Debug.LogError("No start points assigned in PathManager");
            return transform;
        }
        return startPoints[Random.Range(0, startPoints.Length)];
    }
}
