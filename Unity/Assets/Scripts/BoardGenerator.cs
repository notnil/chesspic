using UnityEngine;
using System.Collections.Generic;
using Chess;
using System;
using System.IO;

public class BoardGenerator : MonoBehaviour
{
    public Camera generatorCamera;
    public Light generatorLight;
    public string generatorPath;
    public int generatorCount;
    public bool writeToDisk;
    public List<Label.PieceSet> pieceSetTypes;

    private static readonly Vector3 PositionA1 = new Vector3(-9.0f, 1.1f, 9.0f);
    private static readonly Vector3 PositionA8 = new Vector3(9.0f, 1.1f, 9.0f);
    private static readonly Vector3 PositionH1 = new Vector3(-9.0f, 1.1f, -9.0f);
    private static readonly Vector3 PositionH8 = new Vector3(9.0f, 1.1f, -9.0f);
    private static readonly Vector3 BoardEdgeA1 = new Vector3(-10.0f, 1.1f, 10.0f);
    private static readonly Vector3 BoardEdgeA8 = new Vector3(10.0f, 1.1f, 10.0f);
    private static readonly Vector3 BoardEdgeH1 = new Vector3(-10.0f, 1.1f, -10.0f);
    private static readonly Vector3 BoardEdgeH8 = new Vector3(10.0f, 1.1f, -10.0f);
    private static readonly List<Board> FensFileBoards = new List<Board>();

    private Label label = null;
    private int screenshotsTaken = 0;
    private float drawTime = 0.01f;
    private float timeLeft;
    private bool done;

    void Start()
    {
        timeLeft = drawTime;
        InitFenBoards();

        if (writeToDisk)
        {
            string filePath = generatorPath + "/_labels.csv";
            if (File.Exists(filePath) == false)
            {
                string s = string.Join(",", Label.CSVHeader()) + "\n";
                System.IO.File.WriteAllText(filePath, s);
            }
        }
    }
    void InitFenBoards()
    {
        // from https://www.kingbase-chess.net/
        string FensFilePath = Application.dataPath + "/fens.txt";
        var lines = File.ReadLines(FensFilePath);
        var linesList = new List<string>(lines);
        Shuffle(linesList);
        foreach (string line in linesList.GetRange(0, generatorCount))
        {
            Board b = Board.ParseFen(line);
            FensFileBoards.Add(b);
        }
        Board.CompetitionBoards = FensFileBoards;
        Debug.Log("Loaded Fen Boards " + Board.CompetitionBoards.Count.ToString());
    }

    void FixedUpdate()
    {
        if (done)
        {
            return;
        }
        if (label == null)
        {
            label = new Label();
            DrawLabel();
            timeLeft = drawTime;
            return;
        }
        timeLeft -= Time.deltaTime;
        if (timeLeft < 0)
        {
            if (IsValidLabel())
            {
                if (screenshotsTaken < generatorCount)
                {
                    if (writeToDisk)
                    {
                        byte[] b = TakeScreenshot(generatorCamera);
                        string fname = generatorPath + "/" + label.fileName;
                        System.IO.File.WriteAllBytes(fname, b);
                    }

                    screenshotsTaken++;
                    // the board must rotate or some other issue so I have to use H1 instead of A1 
                    Vector3 coordsA1 = generatorCamera.WorldToViewportPoint(positionForSquare(Square.H1));
                    label.referencePoint = new Vector2(coordsA1.x, coordsA1.y);
                    label.boundingBox = BoundingBox();
                    if (writeToDisk)
                    {
                        WriteCSV();
                    }
                }
                else
                {
                    done = true;
                    return;
                }
            }
            LabelCleanUp();
        }
    }

    public void DrawLabel()
    {
        GameObject boardPrefab = label.boardSet.GetPrefab();
        GameObject boardObj = Instantiate(boardPrefab, new Vector3(0, 0, 0), Quaternion.Euler(label.boardSet.rotation));
        boardObj.transform.localScale = label.boardSet.scale;
        boardObj.tag = "Piece";
        foreach (KeyValuePair<Square, Piece> entry in label.board.pieceMap)
        {
            if (entry.Value == null)
            {
                continue;
            }
            GameObject prefab = label.pieceSet.GetPrefab(entry.Value);
            Vector3 position = positionForSquare(entry.Key);
            Vector3 rotation = label.pieceSet.rotation;
            rotation.y = UnityEngine.Random.Range(0, 360);
            GameObject obj = Instantiate(prefab, position, Quaternion.Euler(rotation));
            obj.tag = "Piece";
            obj.transform.localScale = label.pieceSet.scale;
        }
        generatorCamera.transform.position = label.cameraPoint;
        generatorCamera.transform.LookAt(label.lookAtPoint);
        generatorLight.transform.rotation = Quaternion.Euler(label.lightRotation);
    }
    public void WriteCSV()
    {
        string filePath = generatorPath + "/_labels.csv";
        using (System.IO.StreamWriter file = new System.IO.StreamWriter(filePath, true))
        {
            file.WriteLine(string.Join(",", label.CSVRow()));
        }
    }
    public Label.BoundingBox BoundingBox()
    {
        List<Vector3> squarePositions = new List<Vector3> { BoardEdgeA1, BoardEdgeA8, BoardEdgeH1, BoardEdgeH8 };
        Vector3 min = new Vector3(1, 1, 0);
        Vector3 max = new Vector3(0, 0, 0);
        foreach (Vector3 sqPos in squarePositions)
        {
            Vector3 low = new Vector3(sqPos.x, label.boardSet.elevation, sqPos.z);
            Vector3 high = new Vector3(sqPos.x, label.boardSet.elevation + 5.0f, sqPos.z);
            foreach (Vector3 pos in new List<Vector3> { low, high })
            {
                Vector3 pixel = generatorCamera.WorldToViewportPoint(pos);
                min = MinVector(min, pixel);
                max = MaxVector(max, pixel);
            }
        }
        return new Label.BoundingBox(new Vector2(min.x, min.y), new Vector2(max.x, max.y));
    }
    public bool IsValidLabel()
    {
        Vector3 coordsA1 = generatorCamera.WorldToViewportPoint(BoardEdgeA1);
        Vector3 coordsA8 = generatorCamera.WorldToViewportPoint(BoardEdgeA8);
        Vector3 coordsH1 = generatorCamera.WorldToViewportPoint(BoardEdgeH1);
        Vector3 coordsH8 = generatorCamera.WorldToViewportPoint(BoardEdgeH8);
        bool valid = InBounds(coordsA1) && InBounds(coordsA8) && InBounds(coordsH1) && InBounds(coordsH8);
        // Debug.LogFormat("FEN: {0} {1} {2} {3} {4} {5}", label.board.ToString(), coordsA1, coordsA8, coordsH1, coordsH8, valid);
        return valid;
    }
    private const int resWidth = 1600;
    private const int resHeight = 1000;
    private bool InBounds(Vector3 point)
    {
        return point.x >= 0 && point.y >= 0 && point.x <= 1.0 && point.y <= 1.0;
    }
    public void LabelCleanUp()
    {
        label = null;
        GameObject[] gameObjects = GameObject.FindGameObjectsWithTag("Piece");
        for (var i = 0; i < gameObjects.Length; i++)
        {
            Destroy(gameObjects[i]);
        }
    }
    public static byte[] TakeScreenshot(Camera camera)
    {
        RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        return screenShot.EncodeToJPG();
    }
    private Vector3 positionForSquare(Square sq)
    {
        int row = (int)sq / 8;
        float dx = Math.Abs(PositionA1.x - PositionH8.x);
        float x = PositionA1.x + (((float)row * dx) / 7f);

        int file = (int)sq % 8;
        float dz = Math.Abs(PositionA1.z - PositionH8.z);
        float z = PositionH8.z + (((float)file * dz) / 7f);
        return new Vector3(x, label.boardSet.elevation, z);
    }

    private static System.Random rng = new System.Random();
    public static void Shuffle(List<string> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            string value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Vector3 MaxVector(Vector3 v1, Vector3 v2)
    {
        float x = Math.Max(v1.x, v2.x);
        float y = Math.Max(v1.y, v2.y);
        float z = Math.Max(v1.z, v2.z);
        return new Vector3(x, y, z);
    }

    public static Vector3 MinVector(Vector3 v1, Vector3 v2)
    {
        float x = Math.Min(v1.x, v2.x);
        float y = Math.Min(v1.y, v2.y);
        float z = Math.Min(v1.z, v2.z);
        return new Vector3(x, y, z);
    }
}
