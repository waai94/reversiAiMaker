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
    [SerializeField] bool reverseMode = false; // 評価値を反転させるモード

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
                if (reversiGame.GetFlippableCount(x, y, myTurn) == 0)
                {
                    score[x, y] = float.NegativeInfinity; //置けない場所は極端に低い値にする
                    continue;
                }

                // 可動性
                score[x, y] += mobilityWeight * GetMobilityValue(x, y, myTurn);

                // 位置の価値
                score[x, y] *= positionWeight[x, y];
                if (reverseMode && score[x,y] !=float.NegativeInfinity)
                {
                    score[x, y] = -score[x, y];
                }
            }
        }
        //int[][] flipPositions = GetFlipPosition(score);
        int[] flipPosition = ChooseRandomTopPosition(score, choiceNumber);
         Debug.Log($"{(myTurn == 1 ? "Black" : "White")}  AI choice {flipPosition[0]},{flipPosition[1]}");
        Debug.Log("AI making move...");
        if (!reversiGame) return;
        reversiGame.MakeMove(flipPosition[0], flipPosition[1]);
    }
    // スコア配列から上位choiceNumber個の位置を取得する

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
    // 上位choiceNumber個の中からランダムに1つ選ぶ
    int[] ChooseRandomTopPosition(float[,] scores, int choiceNumber)
    {
        int[][] topPositions = GetFlipPosition(scores); // 上位n件を取得
        if (topPositions.Length == 0)
        {
            Debug.LogError("No valid positions available for AI to choose from.");
            return null;
        }

        // 0 ～ topPositions.Length-1 のランダムなインデックスを取得
        int randomIndex = UnityEngine.Random.Range(0, topPositions.Length);
        Debug.Log($"AI selected position: {topPositions[randomIndex][0]}, {topPositions[randomIndex][1]}");
        return topPositions[randomIndex]; // 選ばれた [x, y] を返す
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
