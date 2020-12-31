﻿using UnityEngine;
using System.Collections;   // 引用 系統.集合 API - 協同程序

public class TetrisManager : MonoBehaviour
{
    [Header("掉落時間"), Range(0.1f, 3)]
    public float timeFall = 1.5f;
    [Header("目前分數")]
    public int score;
    [Header("最高分數")]
    public int scoreHight;
    [Header("等級")]
    public int lv = 1;
    [Header("結束畫面")]
    public GameObject objFinal;
    [Header("音效：地落、移動、消除與結束")]
    public AudioClip soundFall;
    public AudioClip soundMove;
    public AudioClip soundClear;
    public AudioClip soundLose;
    [Header("下一個俄羅斯方塊區域")]
    public Transform traNextArea;
    [Header("生成俄羅斯方塊的父物件")]
    public Transform traTetrisParent;
    [Header("生成的起始位置")]
    public Vector2[] posSpawn =
    {
        new Vector2(20, 400),
        new Vector2(20, 360),
        new Vector2(0, 360),
        new Vector2(0, 380),
        new Vector2(0, 380),
        new Vector2(20, 360),
        new Vector2(20, 360)
    };

    /// <summary>
    /// 下一顆俄羅斯方塊編號
    /// </summary>
    private int indexNext;

    /// <summary>
    /// 目前俄羅斯方塊
    /// </summary>
    private RectTransform currentTetris;

    /// <summary>
    /// 計時器
    /// </summary>
    private float timer;

    // 開始事件：開始時候執行一次
    private void Start()
    {
        RandomTetris();
    }

    // 更新事件：一秒執行約 60 次
    private void Update()
    {
        ControlTertis();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // 啟動協同程序(協同方法());
            StartCoroutine(ShakeEffect());
        }
    }

    /// <summary>
    /// 控制俄羅斯方塊
    /// </summary>
    private void ControlTertis()
    {
        // 如果 已經有 目前的俄羅斯方塊
        if (currentTetris)
        {
            timer += Time.deltaTime;        // 計時器 累加 一幀的時間 - 累加時間

            if (timer >= timeFall)
            {
                timer = 0;
                currentTetris.anchoredPosition -= new Vector2(0, 40);
            }

            #region 控制俄羅斯方塊 左右、旋轉與加速

            // 取得 目前俄羅斯方塊的 Tetris 腳本
            Tetris tetris = currentTetris.GetComponent<Tetris>();

            // 如果 X 座標 小於 280 才能往右移動
            // if (currentTetris.anchoredPosition.x < 280)
            // 如果 目前俄羅斯方塊 沒有 碰到右邊牆壁
            if (!tetris.wallRight)
            {
                // 或者 ||
                // 按下 D 或者 右鍵 往右 50 
                if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
                {
                    currentTetris.anchoredPosition += new Vector2(40, 0);
                }
            }
            // 如果 X 座標 小於 280 才能往右移動
            // if (currentTetris.anchoredPosition.x > -280)
            // 如果 目前俄羅斯方塊 沒有 碰到左邊牆壁
            if (!tetris.wallLeft)
            {
                // 或者 ||
                // 按下 D 或者 右鍵 往右 50 
                if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    currentTetris.anchoredPosition -= new Vector2(40, 0);
                }
            }

            // 如果 俄羅斯方塊 可以旋轉
            // 按下 W 逆時針轉 90 度
            if (tetris.canRotate)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
                {
                    // 屬性面板上面的 rotation 必須用 eulerAngles 控制
                    currentTetris.eulerAngles += new Vector3(0, 0, 90);

                    tetris.Offset();
                }
            }
            
            
            // 如果 玩家 按住 S 就加速
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                timeFall = 0.1f;
            }
            // 否則 就恢復速度
            else
            {
                timeFall = 1.5f;
            }
            #endregion

            // 如果 俄羅斯方塊 碰到了地板 就重新開始 - 生成下一顆
            if (tetris.wallDown)
            {
                StartGame();
            }
        }
    }

    private bool down;

    /// <summary>
    /// 生成俄羅斯方塊
    /// 1. 隨機顯示一個下一顆俄羅斯方塊 0 - 6
    /// </summary>
    private void RandomTetris()
    {
        // 下一顆編號 = 隨機 的 範圍(最小，最大) - 整數不會等於最大值
        indexNext = Random.Range(0, 7);

        // 測試
        // indexNext = 0;

        // 下一個俄羅斯方塊區域 . 取得子物件(子物件編號) . 轉為遊戲物件 . 啟動設定(顯示)
        traNextArea.GetChild(indexNext).gameObject.SetActive(true);
    }

    /// <summary>
    /// 開始遊戲
    /// 1. 生成俄羅斯方塊要放在正確位置
    /// 2. 上一次俄羅斯方塊隱藏
    /// 3. 隨機取得下一個
    /// </summary>
    public void StartGame()
    {
        // 1. 生成俄羅斯方塊要放在正確位置
        // 保存上一次的俄羅斯方塊
        GameObject tetris = traNextArea.GetChild(indexNext).gameObject;
        // 目前俄羅斯方塊 = 生成物件(物件，父物件)
        GameObject current = Instantiate(tetris, traTetrisParent);
        // GetComponent<任何元件>()
        // <T> 泛型 - 指的是所有類型
        // 目前俄羅斯方塊 . 取得元件<介面變形>() . 座標 = 生成座標陣列[編號]
        current.GetComponent<RectTransform>().anchoredPosition = posSpawn[indexNext];

        // 2. 上一次俄羅斯方塊隱藏
        tetris.SetActive(false);
        // 3. 隨機取得下一個
        RandomTetris();

        // 將生成的俄羅斯方塊 RectTransform 元件儲存
        currentTetris = current.GetComponent<RectTransform>();
    }

    /// <summary>
    /// 添加分數
    /// </summary>
    /// <param name="add">要添加的分數</param>
    public void AddScore(int add)
    {

    }

    private void GameTime()
    {

    }

    private void GameOver()
    {

    }

    public void ReplayGame()
    {

    }

    public void QuitGame()
    {

    }

    // 協同程序
    // IEnumerator 傳回類型 - 時間
    private IEnumerator ShakeEffect()
    {
        // 取得震動效果物件的 Rect
        RectTransform rect = traTetrisParent.GetComponent<RectTransform>();

        // 晃動 向上 30 > 0 > 20 > 0
        // 等待 0.05
        float interval = 0.05f;

        rect.anchoredPosition += Vector2.up * 30;
        yield return new WaitForSeconds(interval);
        rect.anchoredPosition = Vector2.zero;
        yield return new WaitForSeconds(interval);
        rect.anchoredPosition += Vector2.up * 20;
        yield return new WaitForSeconds(interval);
        rect.anchoredPosition = Vector2.zero;
        yield return new WaitForSeconds(interval);
    }
}
