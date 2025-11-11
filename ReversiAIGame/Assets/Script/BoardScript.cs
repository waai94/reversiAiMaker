using UnityEngine;

public class BoardScript : MonoBehaviour
{
    public int myBoardX { get; set; }//盤面のX座標
    public int myBoardY { get; set; }//盤面のY座標
    public int id { get; set; }//盤面のID

    public ReversiGame reversiGame { get; set; } //ReversiGameスクリプトへの参照

    private SpriteRenderer stoneRenderer;//石のスプライトレンダラー
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        SpriteRenderer[] renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.gameObject.name == "Stone")
            {
                stoneRenderer = renderer;
                break;
            }
        }
    }
    void Start()
    {
        
        //int randomValue = Random.Range(0, 3); // 0, 1, 2のいずれかをランダムに取得
        //UpdateBoardVisual(randomValue); // 初期状態では石を非表示にする
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMouseDown()
    {
        Debug.Log("Board clicked at: " + myBoardX + ", " + myBoardY + " with ID: " + id);
        // ここにクリック時の処理を追加
        if(!reversiGame) 
        {
            Debug.LogError("ReversiGame reference is not set!");
            return;
        }
        reversiGame.MakeMove(myBoardX, myBoardY);
    }

    public void UpdateBoardVisual(int player)
    {
        // ここに盤面のビジュアル更新ロジックを追加
        // 例えば、playerが1なら黒石、2なら白石を表示するなど
        if(!stoneRenderer)
        {
            Debug.LogError("Stone SpriteRenderer is not set!");
            return;
        }

        if (player == 1)
        {
            // 黒石を表示
            stoneRenderer.color = Color.black;
            stoneRenderer.enabled = true;
        }
        else if (player == 2)
        {
            // 白石を表示
            stoneRenderer.color = Color.white;
            stoneRenderer.enabled = true;
        }
        else if( player == -1)// 無効な手を示す表示（赤色）
        {
            stoneRenderer.color = Color.red;
            stoneRenderer.enabled = true;
        } else
        {
            // 石を非表示
            stoneRenderer.enabled = false;
        }
    }
}
