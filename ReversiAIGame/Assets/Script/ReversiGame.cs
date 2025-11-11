using UnityEngine;
using System.Collections.Generic;

public class ReversiGame : MonoBehaviour
{
    private const int BoardSize = 8;
    private int[,] board = new int[BoardSize, BoardSize]; // 0:空, 1:黒, 2:白
    private int currentPlayer = 1;

    private readonly int[] dx = { -1, -1, -1, 0, 1, 1, 1, 0 };
    private readonly int[] dy = { -1, 0, 1, 1, 1, 0, -1, -1 };

    [SerializeField] private GameObject boardPrehab;
    [SerializeField] private bool aiEnabled = true;
    [SerializeField] private float aiThinkTime = 1.0f;

    private List<BoardScript> boardCells = new List<BoardScript>();
    private ReversiAIScirpt aiScript;        // 白AI
    private ReversiAIScirpt playerAIScript;  // 黒AI（有効時のみ）

    void Start()
    {
        InitializeBoard();
        SpawnBoard();
        UpdateCells();
        PrintBoard();
        if(aiEnabled) HandleAITurn();
    }

    //=========================//
    //  初期化・生成関連
    //=========================//
    private void InitializeBoard()
    {
        board[3, 3] = 2;
        board[4, 4] = 2;
        board[3, 4] = 1;
        board[4, 3] = 1;
    }

    private void SpawnBoard()
    {
        if (!boardPrehab)
        {
            Debug.LogError("Board prefab is not assigned!");
            return;
        }

        Vector3 spawnPosition = new Vector3(-BoardSize / 2, -BoardSize / 2, 0);
        for (int x = 0; x < BoardSize; x++)
        {
            for (int y = 0; y < BoardSize; y++)
            {
                GameObject cellObj = Instantiate(boardPrehab, spawnPosition, Quaternion.identity);
                var cellScript = cellObj.GetComponent<BoardScript>();

                if (cellScript)
                {
                    cellScript.myBoardX = x;
                    cellScript.myBoardY = y;
                    cellScript.id = x + y * BoardSize;
                    cellScript.reversiGame = this;
                    boardCells.Add(cellScript);
                }
                spawnPosition.x += 1;
            }
            spawnPosition.x = -BoardSize / 2;
            spawnPosition.y += 1;
        }
    }

    //=========================//
    //  AI関連
    //=========================//
    private ReversiAIScirpt FindAIScript(string tag, int player)
    {
        GameObject obj = GameObject.FindGameObjectWithTag(tag);
        if (!obj) return null;

        var script = obj.GetComponent<ReversiAIScirpt>();
        if (!script)
        {
            Debug.LogError($"ReversiAIScirpt not found on tag: {tag}");
            return null;
        }

        script.InitializeAI(player);
        return script;
    }

    private void HandleAITurn()
    {
        if (currentPlayer == 2)
        {
            aiScript ??= FindAIScript("ReversiAI", 2);
            aiScript?.RunAISearch();
        }
        else if (currentPlayer == 1 && aiEnabled)
        {
            playerAIScript ??= FindAIScript("PlayerReversiAI", 1);
            playerAIScript?.RunAISearch();
        }
    }

    //=========================//
    //  手の処理
    //=========================//
    public void MakeMove(int x, int y)
    {
        if (!IsValidMove(x, y, currentPlayer))
        {
            Debug.Log($"{(currentPlayer == 1 ? "黒" : "白")}：無効な手です");
            return;
        }

        ApplyMove(x, y, currentPlayer);
        currentPlayer = 3 - currentPlayer;
      //  PrintBoard();

        // パス判定
        if (!HasValidMove(currentPlayer))
        {
            Debug.Log($"{(currentPlayer == 1 ? "黒" : "白")}はパスしました。");
            currentPlayer = 3 - currentPlayer;

            if (!HasValidMove(currentPlayer))
            {
                Debug.Log("両者とも打てる手がありません。ゲーム終了。");
                PrintResult();
                return;
            }
        }
        if(aiEnabled)
        {
            Debug.Log($"{(currentPlayer == 1 ? "黒" : "白")}の番です。");
            Invoke(nameof(HandleAITurn), aiThinkTime); // 1秒後にAIの手を処理
        }
        else
        {
            // プレイヤー対戦時はここで次の手を待つ
            Debug.Log($"{(currentPlayer == 1 ? "黒" : "白")}の番です。");
            if(currentPlayer == 2)
            {
                Debug.Log("白はAIが担当します。");
                HandleAITurn();
            }
        }
        //  HandleAITurn();
    }

    private bool IsValidMove(int x, int y, int player)
    {
        if (board[x, y] != 0) return false;
        int opponent = 3 - player;

        for (int dir = 0; dir < 8; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];
            bool hasOpponent = false;

            while (InBounds(nx, ny))
            {
                if (board[nx, ny] == opponent)
                {
                    hasOpponent = true;
                    nx += dx[dir];
                    ny += dy[dir];
                }
                else if (board[nx, ny] == player && hasOpponent)
                    return true;
                else break;
            }
        }
        return false;
    }

    private void ApplyMove(int x, int y, int player)
    {
        board[x, y] = player;
        int opponent = 3 - player;

        for (int dir = 0; dir < 8; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];
            var toFlip = new List<Vector2Int>();

            while (InBounds(nx, ny))
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
                else break;
            }
        }

        UpdateCells();
    }

    private bool HasValidMove(int player)
    {
        for (int x = 0; x < BoardSize; x++)
            for (int y = 0; y < BoardSize; y++)
                if (IsValidMove(x, y, player))
                    return true;
        return false;
    }

    //=========================//
    //  補助関数
    //=========================//
    private bool InBounds(int x, int y) => x >= 0 && x < BoardSize && y >= 0 && y < BoardSize;

    private void UpdateCells()
    {
        foreach (var cell in boardCells)
            cell.UpdateBoardVisual(board[cell.myBoardX, cell.myBoardY]);
    }

    public int GetFlippableCount(int x, int y, int player)
    {
        if (board[x, y] != 0) return 0;
        int opponent = 3 - player;
        int total = 0;

        for (int dir = 0; dir < 8; dir++)
        {
            int nx = x + dx[dir], ny = y + dy[dir], count = 0;
            while (InBounds(nx, ny))
            {
                if (board[nx, ny] == opponent)
                {
                    count++; nx += dx[dir]; ny += dy[dir];
                }
                else if (board[nx, ny] == player && count > 0)
                {
                    total += count; break;
                }
                else break;
            }
        }
        return total;
    }

    //=========================//
    //  デバッグ表示
    //=========================//
    private void PrintBoard()
    {
        string s = "";
        for (int y = BoardSize - 1; y >= 0; y--)
        {
            for (int x = 0; x < BoardSize; x++)
                s += board[x, y] switch
                {
                    0 => ". ",
                    1 => "B ",
                    2 => "W ",
                    _ => "? "
                };
            s += "\n";
        }
        Debug.Log(s);
    }

    private void PrintResult()
    {
        int black = 0, white = 0;
        foreach (var c in board)
        {
            if (c == 1) black++;
            else if (c == 2) white++;
        }

        Debug.Log($"黒: {black} 白: {white}");
        Debug.Log(black > white ? "黒の勝ち！" : white > black ? "白の勝ち！" : "引き分け！");
    }
}
