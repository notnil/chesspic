using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chess
{
    public class Piece
    {
        public static readonly Piece WhiteKing = new Piece(Color.White, PieceType.King);
        public static readonly Piece WhiteQueen = new Piece(Color.White, PieceType.Queen);
        public static readonly Piece WhiteRook = new Piece(Color.White, PieceType.Rook);
        public static readonly Piece WhiteBishop = new Piece(Color.White, PieceType.Bishop);
        public static readonly Piece WhiteKnight = new Piece(Color.White, PieceType.Knight);
        public static readonly Piece WhitePawn = new Piece(Color.White, PieceType.Pawn);
        public static readonly Piece BlackKing = new Piece(Color.Black, PieceType.King);
        public static readonly Piece BlackQueen = new Piece(Color.Black, PieceType.Queen);
        public static readonly Piece BlackRook = new Piece(Color.Black, PieceType.Rook);
        public static readonly Piece BlackBishop = new Piece(Color.Black, PieceType.Bishop);
        public static readonly Piece BlackKnight = new Piece(Color.Black, PieceType.Knight);
        public static readonly Piece BlackPawn = new Piece(Color.Black, PieceType.Pawn);

        public readonly Color color;
        public readonly PieceType pieceType;

        public static List<Piece> All() {
            return new List<Piece>(AllPieces);
        }
        public static Piece FromChar(char c)
        {
            int idx = FenChars.IndexOf(c);
            if (idx == -1) {
                return null;
            }
            return AllPieces[idx];
        }
        public char FENChar()
        {
            int idx = AllPieces.IndexOf(this);
            return FenChars[idx];
        }
        private static readonly List<Piece> AllPieces = new List<Piece>(new Piece[] {
            WhiteKing , WhiteQueen, WhiteRook, WhiteBishop, WhiteKnight, WhitePawn,
            BlackKing , BlackQueen, BlackRook, BlackBishop, BlackKnight, BlackPawn
        });
        private const string FenChars = "KQRBNPkqrbnp";
        private Piece(Color color, PieceType pieceType)
        {
            this.color = color;
            this.pieceType = pieceType;
        }
    }
    public enum Color {
        Black,
        White
    }

    public enum PieceType {
        King,
        Queen,
        Rook,
        Bishop,
        Knight,
        Pawn
    }
    public enum Square
    {
        A1, B1, C1, D1, E1, F1, G1, H1,
        A2, B2, C2, D2, E2, F2, G2, H2,
        A3, B3, C3, D3, E3, F3, G3, H3,
        A4, B4, C4, D4, E4, F4, G4, H4,
        A5, B5, C5, D5, E5, F5, G5, H5,
        A6, B6, C6, D6, E6, F6, G6, H6,
        A7, B7, C7, D7, E7, F7, G7, H7,
        A8, B8, C8, D8, E8, F8, G8, H8,
    }
    public class Board
    {
        private static readonly Board StartingBoard = Board.ParseFen("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR");
        public static List<Board> CompetitionBoards = new List<Board>();

        public Dictionary<Square, Piece> pieceMap;

        public Board(Dictionary<Square, Piece> pieceMap)
        {
            this.pieceMap = new Dictionary<Square, Piece>(pieceMap);
        }
        public static Board Random(RandomStrategy strategy)
        {
            System.Random rnd = new System.Random();
            if (strategy == RandomStrategy.CompetitionDistibution) {
                int i = rnd.Next(CompetitionBoards.Count);
                return CompetitionBoards[i];
            }
            Dictionary<Square, Piece> pieceMap = new Dictionary<Square, Piece>();
            foreach (Square sq in System.Enum.GetValues(typeof(Square)))
            {
                Piece p = null;
                switch (strategy) {
                    case RandomStrategy.HalfEmptyRandomDistribution:
                        p = HalfEmptyRandomDistribution(rnd, sq);
                        break;
                    case RandomStrategy.StartingPositionDistribution:
                        p = StartingPositionDistribution(rnd, sq);
                        break;
                }
                pieceMap.Add(sq, p);
            }
            return new Board(pieceMap);
        }
        public static Board ParseFen(string fen)
        {
            string[] a = fen.Split(' ');
            string boardFen = a[0];
            string[] arr = boardFen.Split('/');
            List<string> list = new List<string>(arr);
            for (int i = 0; i < 8; i++)
            {
                String s = Util.FillInOnes(list[i]);
                list[i] = Util.Reverse(s);
            }
            list.Reverse();
            Dictionary<Square, Piece> pieceMap = new Dictionary<Square, Piece>();
            foreach (Square sq in System.Enum.GetValues(typeof(Square)))
            {
                int row = (int)sq / 8;
                int file = (int)sq % 8;
                char c = list[row][file];
                pieceMap[sq] = Piece.FromChar(c);
            }
            return new Board(pieceMap);
        }
        public override string ToString()
        {
            List<string> list = new List<string>();
            for (int i = 0; i < 8; i++)
            {
                list.Add("");
            }
            foreach (Square sq in System.Enum.GetValues(typeof(Square)))
            {
                int row = (int)sq / 8;
                Piece piece = this.pieceMap[sq];
                char c = '1';
                if (piece != null) {
                    c = piece.FENChar();
                }
                list[row] = list[row] + c;
            }
            for (int i = 0; i < 8; i++)
            {
                list[i] = Util.ReplaceOnes(list[i]);
                list[i] = Util.Reverse(list[i]);
            }
            list.Reverse();
            return string.Join("/", list.ToArray());
        }

        private static Piece HalfEmptyRandomDistribution(System.Random rnd, Square sq) {
            if (rnd.Next(0, 2) == 0)
            {
                return null;
            }
            int idx = rnd.Next(0, 12);
            return Piece.All()[idx];
        }

        private static Piece StartingPositionDistribution(System.Random rnd, Square sq)
        {
            int i = rnd.Next(0, 64);
            return StartingBoard.pieceMap[(Square)i];
        }

        public enum RandomStrategy {
            // squares have a 50% chance of being empty, %50 chance of being a random piece
            HalfEmptyRandomDistribution,
            // squares have the same odds of being empty or containing a piece as the starting position
            StartingPositionDistribution,
            // board randomly selected from a list of board positions from over 7k+ competition games elo > 2000
            CompetitionDistibution
        }
    }

    class Util {
        internal static string ReplaceOnes(string s)
        {
            int skip = 0;
            string result = "";
            foreach (char c in s)
            {
                if (c.Equals('1'))
                {
                    skip++;
                    continue;
                }
                if (skip > 0)
                {
                    result += skip.ToString();
                    skip = 0;
                }
                result += c.ToString();
            }
            if (skip > 0)
            {
                result += skip.ToString();
            }
            return result;
        }
        internal static string FillInOnes(string s)
        {
            string result = "";
            foreach (char c in s)
            {
                int v = (int)Char.GetNumericValue(c);
                if (v == -1)
                {
                    result += c;
                    continue;
                }
                result += new string('1', v);
            }
            return result;
        }
        internal static string Reverse(string s)
        {
            char[] charArray = s.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}
