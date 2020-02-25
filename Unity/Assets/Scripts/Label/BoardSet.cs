using System;
public enum BoardSet
{
    LowRes,
    Staunton,
    Abstract,
    Carved,
    Design,
    Dual,
    Medieval,
    ModernArt
}

static class BoardSetMethods
{
    public static GameObject GetPrefab(this PieceSet ps, Piece piece)
    {
        return ps.GetPieceSetImpl().GetPrefab(piece);
    }

    public class BoardSet
{
    public static readonly BoardSet LowRes = new BoardSet("LowRes", new Vector3(1, 1, 1), new Vector3(0, 90, 0), 1.15f);
    public static readonly BoardSet Staunton = new BoardSet("Staunton", new Vector3(49, 49, 49), new Vector3(270, 90, 0), 0.7f);
    public static readonly BoardSet Abstract = new BoardSet("Abstract", new Vector3(0.037f, 0.037f, 0.037f), new Vector3(270, 90, 0), 0.7f);
    public static readonly BoardSet Carved = new BoardSet("Carved", new Vector3(49, 49, 49), new Vector3(270, 90, 0), 0.7f);
    public static readonly BoardSet Design = new BoardSet("Design", new Vector3(49, 49, 49), new Vector3(270, 0, 0), 0);
    public static readonly BoardSet Dual = new BoardSet("Dual", new Vector3(0.037f, 0.037f, 0.037f), new Vector3(270, 0, 0), 0);
    public static readonly BoardSet Medieval = new BoardSet("Medieval", new Vector3(49, 49, 49), new Vector3(270, 90, 0), 0);
    public static readonly BoardSet ModernArt = new BoardSet("ModernArt", new Vector3(0.037f, 0.037f, 0.037f), new Vector3(270, 90, 0), 0);

    public string name;
    public Vector3 scale;
    public Vector3 rotation;
    public float elevation;

    public BoardSet(string name, Vector3 scale, Vector3 rotation, float elevation)
    {
        this.name = name;
        this.scale = scale;
        this.rotation = rotation;
        this.elevation = elevation;
    }

    public GameObject GetPrefab()
    {
        string prefabPath = String.Format("Assets/Prefabs/Boards/{0}.prefab", name);
        return AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
    }
}

public struct BoundingBox
{
    public readonly Vector2 min;
    public readonly Vector2 max;

    public BoundingBox(Vector2 min, Vector2 max)
    {
        this.min = min;
        this.max = max;
    }
}
