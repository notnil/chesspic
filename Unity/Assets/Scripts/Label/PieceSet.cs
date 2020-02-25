using System;
using Chess;
using UnityEditor;
using UnityEngine;

public enum PieceSet
{
    LowRes,
    HighRes,
    Staunton,
    Carved,
    Dual
}

static class PieceSetMethods
{
    public static GameObject GetPrefab(this PieceSet ps, Piece piece)
    {
        return ps.GetPieceSetImpl().GetPrefab(piece);
    }

    public static Vector3 GetScale(this PieceSet ps)
    {
        return ps.GetPieceSetImpl().scale;
    }
    public static Vector3 GetRotation(this PieceSet ps)
    {
        return ps.GetPieceSetImpl().rotation;
    }
    private static PieceSetImpl GetPieceSetImpl(this PieceSet ps)
    {
        switch (ps) {
            case PieceSet.LowRes:
                return PieceSetImpl.LowRes;
            case PieceSet.HighRes:
                return PieceSetImpl.HighRes;
            case PieceSet.Staunton:
                return PieceSetImpl.Staunton;
            case PieceSet.Carved:
                return PieceSetImpl.Carved;
            case PieceSet.Dual:
                return PieceSetImpl.Dual;
        }
        return null;
    }

    private class PieceSetImpl
    {
        public static readonly PieceSetImpl LowRes = new PieceSetImpl("LowRes", new Vector3(1, 1, 1), new Vector3());
        public static readonly PieceSetImpl HighRes = new PieceSetImpl("HighRes", new Vector3(180, 180, 180), new Vector3());
        public static readonly PieceSetImpl Staunton = new PieceSetImpl("Staunton", new Vector3(45, 45, 45), new Vector3(270, 0, 0));
        public static readonly PieceSetImpl Carved = new PieceSetImpl("Carved", new Vector3(45, 45, 45), new Vector3(180, 0, 0));
        public static readonly PieceSetImpl Dual = new PieceSetImpl("Dual", new Vector3(45, 45, 45), new Vector3(270, 0, 0));

        public string name;
        public Vector3 scale;
        public Vector3 rotation;

        public PieceSetImpl(string name, Vector3 scale, Vector3 rotation)
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

        private static string ColorName(Chess.Color c)
        {
            switch (c)
            {
                case Chess.Color.Black:
                    return "black";
                case Chess.Color.White:
                    return "white";
            }
            return "";
        }
    }
}

