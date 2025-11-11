using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;

public class ReversiGame : MonoBehaviour
{
    private const int BoardSize = 8;
    private int[,] board = new int[BoardSize, BoardSize]; // 0:空, 1:黒, 2:白
    private int currentPlayer = 1; // 1:黒, 2:白

    private readonly int[] dx = { -1, -1, -1, 0, 1, 1, 1, 0 };
    private readonly int[] dy = { -1, 0, 1, 1, 1, 0, -1, -1 };

    [SerializeField] private GameObject boardPrehab;
    private List<BoardScript> boardCells = new List<BoardScript>();
    void Start()
    {
        InitializeBoard();
        PrintBoard();
        
        SpawnBoard();
        UpdateCells();
    }

    // 盤面のセルを生成する関数
    void SpawnBoard()
    {
        if(!boardPrehab) 
        {
            Debug.LogError("Board prefab is not assigned!");
            return;
        }
        // 盤面のセルを生成
        Vector3 spawnPosition = new Vector3(-BoardSize / 2, -BoardSize / 2, 0);
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                GameObject boardCell = Instantiate(boardPrehab, spawnPosition, Quaternion.identity);
                BoardScript boardScript = boardCell.GetComponent<BoardScript>();
                boardCells.Add(boardScript);
                if (boardScript != null)
                {
                    boardScript.myBoardX = x;
                    boardScript.myBoardY = y;
                    boardScript.id = x + y * BoardSize;
                    boardScript.reversiGame = this;
                }
                else
                {
                    Debug.LogError("BoardScript component not found on the prefab!");
                }
                spawnPosition.x += 1;
            }
            spawnPosition.x = -BoardSize / 2;
            spawnPosition.y += 1;
        }
    }
    void InitializeBoard()
    {
        // 初期盤面
        board[3, 3] = 2;
        board[4, 4] = 2;
        board[3, 4] = 1;
        board[4, 3] = 1;
    }

    // ユーザーからの入力を受ける関数
    public void MakeMove(int x, int y)
    {
        if (!IsValidMove(x, y, currentPlayer))
        {
            Debug.Log("無効な手です");
            return;
        }

        ApplyMove(x, y, currentPlayer);
        currentPlayer = 3 - currentPlayer; // プレイヤー交代
        PrintBoard();

        if (!HasValidMove(currentPlayer))
        {
            currentPlayer = 3 - currentPlayer;
            if (!HasValidMove(currentPlayer))
            {
                Debug.Log("ゲーム終了");
                PrintResult();
            }
        }
    }
    // 指定された位置に石を置けるかどうかをチェック
    bool IsValidMove(int x, int y, int player)
    {
        if (board[x, y] != 0) return false;

        int opponent = 3 - player;
        for (int dir = 0; dir < 8; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];
            bool hasOpponentBetween = false;

            while (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize)
            {
                if (board[nx, ny] == opponent)
                {
                    hasOpponentBetween = true;
                    nx += dx[dir];
                    ny += dy[dir];
                }
                else if (board[nx, ny] == player && hasOpponentBetween)
                {
                    return true;
                }
                else
                {
                    break;
                }
            }
        }
        return false;
    }

    void ApplyMove(int x, int y, int player)
    {
        board[x, y] = player;
        int opponent = 3 - player;

        for (int dir = 0; dir < 8; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];
            System.Collections.Generic.List<Vector2Int> toFlip = new System.Collections.Generic.List<Vector2Int>();

            while (nx >= 0 && nx < BoardSize && ny >= 0 && ny < BoardSize)
            {
                if (board[nx, ny] == opponent)
                {
                    toFlip.Add(new Vector2Int(nx, ny));
                    nx += dx[dir];
                    ny += dy[dir];
                }
                else if (board[nx, ny] == player)
                {
                    foreach (var pos in toFlip)
                        board[pos.x, pos.y] = player;
                    break;
                }
                else
                {
                    break;
                }
            }
        }
        UpdateCells();
    }

    bool HasValidMove(int player)
    {
        for (int x = 0; x < BoardSize; x++)
            for (int y = 0; y < BoardSize; y++)
                if (IsValidMove(x, y, player))
                    return true;
        return false;
    }

    void PrintBoard()
    {
        string s = "";
        for (int y = BoardSize - 1; y >= 0; y--)
        {
            for (int x = 0; x < BoardSize; x++)
            {
                s += board[x, y] switch
                {
                    0 => ".",
                    1 => "B",
                    2 => "W",
                    _ => "?"
                };
                s += " ";
            }
            s += "\n";
        }
        Debug.Log(s);
    }

    void PrintResult()
    {
        int black = 0, white = 0;
        foreach (var cell in board)
        {
            if (cell == 1) black++;
            if (cell == 2) white++;
        }
        Debug.Log($"黒: {black} 白: {white}");
        if (black > white) Debug.Log("黒の勝ち");
        else if (white > black) Debug.Log("白の勝ち");
        else Debug.Log("引き分け");
    }

    // ボードの状態を外部から取得する関数
    public int GetCell(int x, int y)
    {
        return board[x, y];
    }

    void UpdateCells()
    {
        foreach (var cell in boardCells)
        {
            int state = GetCell(cell.myBoardX, cell.myBoardY);
            cell.UpdateBoardVisual(state);
        }
    }
}
