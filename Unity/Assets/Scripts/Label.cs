using System;
using System.Collections.Generic;
using Chess;
using UnityEditor;
using UnityEngine;
using Color = Chess.Color;
using Random = UnityEngine.Random;

public class Label
{
    private static readonly Vector3 LightRotationMin = new Vector3(0, 0, 0);
    private static readonly Vector3 LightRotationMax = new Vector3(180, 180, 180);
    private static readonly Vector3 LookAtPointMin = new Vector3(-3, 0, -3);
    private static readonly Vector3 LookAtPointMax = new Vector3(3, 0, 3);
    private static readonly Vector3 CameraPointMin = new Vector3(-30, 10, -30);
    private static readonly Vector3 CameraPointMax = new Vector3(30, 30, 30);

    private static readonly List<BoardSet> BoardSets = new List<BoardSet>(
        new BoardSet[]{BoardSet.LowRes, BoardSet.Staunton, BoardSet.Abstract, BoardSet.Carved, BoardSet.Design, BoardSet.Dual, BoardSet.Medieval, BoardSet.ModernArt }
    );
    private static readonly List<PieceSet> PieceSets = new List<PieceSet>(
        new PieceSet[]{PieceSet.LowRes, PieceSet.HighRes, PieceSet.Staunton, PieceSet.Carved}
    );

    public string id;
    public Board board;
    public Vector3 lightRotation;
    public Vector3 lookAtPoint;
    public Vector3 cameraPoint;
    public string fileName;
    public Vector2 referencePoint;
    public BoardSet boardSet;
    public PieceSet pieceSet;
    public Board.RandomStrategy randomStrategy;
    public BoundingBox boundingBox;

    public Label()
    {
		System.Random rnd = new System.Random();
		this.id = Guid.NewGuid().ToString();
		this.randomStrategy = (Board.RandomStrategy)rnd.Next(1);
		this.lightRotation = RandomVector(LightRotationMin, LightRotationMax);
        this.lookAtPoint = RandomVector(LookAtPointMin, LookAtPointMax);
        this.cameraPoint = RandomVector(CameraPointMin, CameraPointMax);
        this.board = Board.Random(this.randomStrategy);
        this.fileName = id + ".jpg";
        this.referencePoint = new Vector2();
        this.boardSet = BoardSets[rnd.Next(BoardSets.Count)];
        this.pieceSet = PieceSets[rnd.Next(PieceSets.Count)];
        this.boundingBox = new BoundingBox();
	}

    public static string[] CSVHeader()
    {
        return new string[]{"id","fen","fileName","boardName","pieceSet","randomStrategy",
                "lightRotationX","lightRotationY","lightRotationZ",
                "lookAtPointX", "lookAtPointY", "lookAtPointZ",
                "cameraPointX", "cameraPointY", "cameraPointZ",
                "referencePointX", "referencePointY",
                "boundingBoxMinX", "boundingBoxMinY", "boundingBoxMaxX", "boundingBoxMaxY"};
    }

    public string[] CSVRow()
    {
        return new string[]{id,board.ToString(),fileName,boardSet.name,pieceSet.name,randomStrategy.ToString(),
                lightRotation.x.ToString(),lightRotation.y.ToString(),lightRotation.z.ToString(),
                lookAtPoint.x.ToString(),lookAtPoint.y.ToString(),lookAtPoint.z.ToString(),
                cameraPoint.x.ToString(),cameraPoint.y.ToString(),cameraPoint.z.ToString(),
                referencePoint.x.ToString(), referencePoint.y.ToString(),
                boundingBox.min.x.ToString(),boundingBox.min.y.ToString(), boundingBox.max.x.ToString(), boundingBox.max.y.ToString()
        };
    }

    private static Vector3 RandomVector(Vector3 min, Vector3 max)
    {
        float x = Random.Range(min.x, max.x);
        float y = Random.Range(min.y, max.y);
        float z = Random.Range(min.z, max.z);
        return new Vector3(x, y, z);
    }

    public class PieceSet
    {
        public static readonly PieceSet LowRes = new PieceSet("LowRes", new Vector3(1,1,1), new Vector3());
        public static readonly PieceSet HighRes = new PieceSet("HighRes", new Vector3(180, 180, 180), new Vector3());
        public static readonly PieceSet Staunton = new PieceSet("Staunton", new Vector3(45, 45, 45), new Vector3(270, 0, 0));
        public static readonly PieceSet Carved = new PieceSet("Carved", new Vector3(45, 45, 45), new Vector3(180, 0, 0));
        public static readonly PieceSet Dual = new PieceSet("Dual", new Vector3(45, 45, 45), new Vector3(270, 0, 0));

        public string name;
        public Vector3 scale;
        public Vector3 rotation;

        public PieceSet(string name, Vector3 scale, Vector3 rotation)
        {
            this.name = name;
            this.scale = scale;
            this.rotation = rotation;
        }

        public GameObject GetPrefab(Piece piece)
        {
            string typeName = PieceTypeName(piece.pieceType);
            string colorName = ColorName(piece.color);
            string prefabPath = String.Format("Assets/Prefabs/Pieces/{0}/Models/{1}_{2}.prefab", name, colorName, typeName);
            return AssetDatabase.LoadAssetAtPath(prefabPath, typeof(GameObject)) as GameObject;
        }

        private static string PieceTypeName(PieceType p)
        {
            switch (p)
            {
                case PieceType.King:
                    return "king";
                case PieceType.Queen:
                    return "queen";
                case PieceType.Rook:
                    return "rook";
                case PieceType.Bishop:
                    return "bishop";
                case PieceType.Knight:
                    return "knight";
                case PieceType.Pawn:
                    return "pawn";
            }
            return "";
        }

        private static string ColorName(Color c)
        {
            switch (c)
            {
                case Color.Black:
                    return "black";
                case Color.White:
                    return "white";
            }
            return "";
        }
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

    public struct BoundingBox {
        public readonly Vector2 min;
        public readonly Vector2 max;

        public BoundingBox(Vector2 min, Vector2 max)
        {
            this.min = min;
            this.max = max;
        }
    }
}
