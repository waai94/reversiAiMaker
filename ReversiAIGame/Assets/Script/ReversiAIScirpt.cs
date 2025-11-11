using UnityEngine;

public class ReversiAIScirpt : MonoBehaviour
{
    [SerializeField] private float stoneValueWeight = 1.0f; // 石の価値の重み
    [SerializeField] private float[,] positionWeight = new float[8, 8]
    {
        { 100, -20, 10, 5, 5, 10, -20, 100 },
        { -20, -50, -5, -5, -5, -5, -50, -20 },
        { 10, -5, 1, 1, 1, 1, -5, 10 },
        { 5, -5, 1, 0, 0, 1, -5, 5 },
        { 5, -5, 1, 0, 0, 1, -5, 5 },
        { 10, -5, 1, 1, 1, 1, -5, 10 },
        { -20, -50, -5, -5, -5, -5, -50, -20 },
        { 100, -20, 10, 5, 5, 10, -20, 100 }
    };
    [SerializeField] private float mobilityWeight = 1.0f; // 可動性の重み
    [SerializeField] private int choiceNumber = 3; //上位何手を候補にするか

    [SerializeField] private int myTurn = 1; // AIのターン（1:黒, 2:白）
    private ReversiGame reversiGame;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject reversiObject = GameObject.FindGameObjectWithTag("ReversiGame");
        if (!reversiObject) return;
        reversiGame = reversiObject.GetComponent<ReversiGame>();
        if(!reversiGame)
        {
            Debug.LogError("ReversiGame component not found on the GameObject with tag 'ReversiGame'!");
        }
    }

    public void InitializeAI(int aiTurn)
    {
        // AIの初期化ロジックをここに実装
        myTurn = aiTurn;

    }
    // Update is called once per frame
    void Update()
    {
        
    }

    public void RunAISearch()
    {
        Debug.Log("AI Search Running...");
        // ここにAIの探索ロジックを実装
        float[,] score = new float[8, 8];
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                // 石の価値
                score[x, y] += stoneValueWeight * GetStoneValue(x, y, myTurn);
                if(reversiGame.GetFlippableCount(x, y, myTurn)==0)
                {
                    score[x, y] = -99999999; //置けない場所は極端に低い値にする
                    continue;
                }

                // 可動性
                score[x, y] += mobilityWeight * GetMobilityValue(x, y, myTurn);

                // 位置の価値
                score[x, y] *= positionWeight[x, y];
            }
        }
        int[][] flipPositions = GetFlipPosition(score);
        Debug.Log("AI chooses to place at: " + flipPositions[0][0] + ", " + flipPositions[0][1]);
        reversiGame.MakeMove(flipPositions[0][0], flipPositions[0][1]);
    }

    int[][] GetFlipPosition(float[,] scores)
    {
        // スコアを基に上位choiceNumber個の位置を取得する
        int[][] topPositions = new int[choiceNumber][];
        for (int i = 0; i < choiceNumber; i++)
        {
            float maxScore = float.NegativeInfinity;
            int maxX = -1;
            int maxY = -1;
            // スコア配列を走査して最大値を見つける
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    if (scores[x, y] > maxScore)
                    {
                        maxScore = scores[x, y];
                        maxX = x;
                        maxY = y;
                    }
                }
            }
            if (maxX != -1 && maxY != -1)
            {
                topPositions[i] = new int[] { maxX, maxY };
                scores[maxX, maxY] = float.NegativeInfinity; // 次の最大値を見つけるために現在の最大値を無効化
            }
        }
        return topPositions;
    }

    // 石の取得数を価値として計算する
    int GetStoneValue(int x, int y, int player)
    {
       
        int score =reversiGame.GetFlippableCount(x, y, player); 
        return score;//仮実装
    }

    int GetMobilityValue(int x, int y, int player)
    {
        // ここに可動性の価値を計算するロジックを実装
        return 0;
    }
}
