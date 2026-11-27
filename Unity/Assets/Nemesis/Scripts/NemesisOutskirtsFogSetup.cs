using UnityEngine;

public class NemesisOutskirtsFogSetup : MonoBehaviour
{
    [Range(3, 16)] public int visionRadius = 8;
    void Start()
    {
        bool[,] ground = new bool[33, 25];
        for (int x = 0; x < 33; x++)
            for (int y = 0; y < 25; y++) ground[x, y] = true;
        var fog = GetComponent<NemesisFogOfWar>();
        if (fog == null) fog = gameObject.AddComponent<NemesisFogOfWar>();
        fog.Configure(ground, 0.72f, 0.36f, visionRadius, new Vector2Int(-16, -12));
    }
}
