using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Example_Hand : MonoBehaviour
{
    
    //宣告最大數與最小數------------------------------------------------------------------
    const float MIN = float.MinValue;
    const float MAX = float.MaxValue;
    //-----------------------------------------------------------------------------------
    //線段設定---------------------------------------------------------------------------
    private bool isPressed;
    public GameObject lineDrawPrefabs; // this is where we put the prefabs object
    private GameObject lineDrawPrefab;
    private LineRenderer lineRenderer;
    private List<Vector3> drawPoints = new List<Vector3>();
    //-----------------------------------------------------------------------------------

    //符文設定----------------------------------------------------------------------------
    //最大符文筆畫
    public int[] pattern;
    public const double learn = 0.05;//學習率
    private MLP PR;//多層感知機PR (PATTERN ROCOGNITOR)
    public int expectNumber = 0;

    public string[] rBit = { "0",
    /*                       Void*/
                             "161", "682","717", "727", "852", "825" ,
    /*                        Aer   Ignis  Terra  Aqua  Ordo   Perdito*/
                             "7135", "3858", "8525"};
    /*                        Space   Over  Time */
    public string[] rune = { "Void",
                            "Aer","Ignis", "Terra", "Aqua", "Ordo" ,"Perdito",
                            "Space", "Over", "Time"};



    //除文筆畫的最大最小XY值來正規化圖案成 10*10的圖
    public float dataSize = 10;
    public float max_x, max_y, min_x, min_y, med_x, med_y;    //最大最小中間值
    public float nScale;                        //正規化所需scale


    //-----------------------------------------------------------------------------------

    //text宣告----------------------------------------------------------------------------
    public Text textPattern;//畫面中間的圖形
    public Text textExpected;//畫面上面的期待值
    public Text textRecognized;//畫面下面的判斷值
    public Text textLast;//畫面下面的上一題
    //-----------------------------------------------------------------------------------


    //START初始化------------------------------------------------------------------------
    void Start()
    {
        
        PR = new MLP(new int[] { 100, 8 }, 100, learn);       
        setExpected();
        isPressed = false;
    }
    //-----------------------------------------------------------------------------------

    //每一張frame會呼叫一次update
    void Update()
    {
        
        if (expectNumber == 0)
        {
            setExpected();
        }
        //偵測是使用滑鼠還是觸控面板
        #if UNITY_EDITOR || UNITY_STANDALONE
                MouseInput();   // 滑鼠偵測
        #elif UNITY_ANDROID
		                    MobileInput();  // 觸碰偵測
        #endif
        
    }
    //-----------------------------------------------------------------------------------




    //設置text 顯示上一題種類--------------------------------------------------------------
    void setLast()
    {
        textLast.text = "上一題是 : " + expectNumber;
    }
    //-----------------------------------------------------------------------------------


    //設置text 顯示期望種類---------------------------------------------------------------
    void setExpected()
    {
        expectNumber = (int)Random.Range(0, 9);
        textExpected.text = "寫出此數 : "+ expectNumber;
    }
    //-----------------------------------------------------------------------------------

    //設置text 顯示符文種類
    void setRecognized(int result)
    {
        textRecognized.text ="系統判斷 : "+ result ;
    }
    //-----------------------------------------------------------------------------------


    //設置text 顯示符文圖案   
    void setPattern(int[,] d)
    {
        textPattern.text = "";
        for (int i = d.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = 0; j < d.GetLength(1); j++)
            {
                textPattern.text += d[i, j] + " ";
            }
            textPattern.text += "\n";
        }

    }
    //-----------------------------------------------------------------------------------   

    //使用滑鼠-----------------------------------------------------------------------------------------------
    void MouseInput()
    {
        //按下右鍵-------------------------------------------------------------------------------------------
        if (Input.GetMouseButtonDown(1))
        {



        }
        //----------------------------------------------------------------------------------------------------
        //按下左鍵---------------------------------------------------------------------------------------------
        if (Input.GetMouseButtonDown(0))
        {

            // 刪除畫下的線-------------------------------------------------------------------------------------
            deleteLine("LineDraw");
            //------------------------------------------------------------------------------------------------          

            //將座標轉成世界座標
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));
           
            //設定線-----------------------------------------------------------------------------------------
            lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
            lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
            lineRenderer.SetVertexCount(0);
            drawPoints.Add(mousePos);
            lineRenderer.SetVertexCount(drawPoints.Count);
            lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
            //------------------------------------------------------------------------------------------------

            isPressed = true;

        }
        //----------------------------------------------------------------------------------------------------
        //放開左鍵--------------------------------------------------------------------------------------------
        else if (Input.GetMouseButtonUp(0))
        {
            //判斷符文
            setLast();//顯示上一輪期望值
            pattern = dataNormalize();//圖形正規化
            setRecognized(PR.Recognize(pattern));//識別圖形並顯示                  
            //設定成未按著滑鼠
            isPressed = false;

            //清楚目前點資料
            drawPoints.Clear();

            // 刪除畫下的線-------------------------------------------------------------------------------------
            deleteLine("LineDraw");            
            //------------------------------------------------------------------------------------------------
        }
        //-------------------------------------------------------------------------------------------------------
        //持續壓著左鍵--------------------------------------------------------------------------------------------
        if (isPressed)
        {
            //將座標轉成世界座標
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));


            //畫實際線----------------------------------------------------------------------------------------------
            drawPoints.Add(mousePos);
            lineRenderer.SetVertexCount(drawPoints.Count);
            lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
            //-------------------------------------------------------------------------------------------------------
        }
        //------------------------------------------------------------------------------------------------------

    }
    //------------------------------------------------------------------------------------------------------

    //使用觸控-----------------------------------------------------------------------------------------------

    void MobileInput()
    {
        if (Input.touchCount <= 0)
            return;

        //1個手指觸碰螢幕
        if (Input.touchCount == 1)
        {
            //按下螢幕-------------------------------------------------------------------------------------------
            if (Input.touches[0].phase == TouchPhase.Began)
            {
                // 刪除畫下的線-------------------------------------------------------------------------------------
                deleteLine("LineDraw");
                //------------------------------------------------------------------------------------------------          
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 1.0f));

                //設定實際線-----------------------------------------------------------------------------------------
                lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
                lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
                lineRenderer.SetVertexCount(0);
                drawPoints.Add(mousePos);
                lineRenderer.SetVertexCount(drawPoints.Count);
                lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
                //------------------------------------------------------------------------------------------------                     
                isPressed = true;
            }
            //------------------------------------------------------------------------------------------------------
            //放開螢幕-------------------------------------------------------------------------------------------
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {

                //判斷符文
                setLast();//顯示上一輪期望值
                pattern = dataNormalize();//圖形正規化
                setRecognized(PR.Recognize(pattern));//識別圖形並顯示                
                //設定成未按著滑鼠
                isPressed = false;

                //清楚目前點資料
                drawPoints.Clear();

                // 刪除線-------------------------------------------------------------------------------------
                deleteLine("LineDraw");
                //------------------------------------------------------------------------------------------------              

            }
            //------------------------------------------------------------------------------------------------------
            //持續壓著螢幕-------------------------------------------------------------------------------------------
            if (isPressed)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 1.0f));

                //畫實際線----------------------------------------------------------------------------------------------
                drawPoints.Add(mousePos);
                lineRenderer.SetVertexCount(drawPoints.Count);
                lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
                //-------------------------------------------------------------------------------------------------------
            }

            //--------------------------------------------------------------------------------------------------------


        }
    }//end void

    //刪除線-------------------------------------------------------------------------------------------------
    void deleteLine(string s)
    {
        GameObject[] delete = GameObject.FindGameObjectsWithTag(s);
        int deleteCount = delete.Length;
        for (int i = deleteCount - 1; i >= 1; i--)
            Destroy(delete[i]);
    }
    //------------------------------------------------------------------------------------------------------ 

    //正規化為10*10------------------------------------------------------------------------------------------------------ 
    int[] dataNormalize()
    {

        //初始化
        int[,] data = new int[(int)dataSize, (int)dataSize];
        max_x = MIN;
        max_y = MIN;

        min_x = MAX;
        min_y = MAX;

        //取出最大最小
        foreach (Vector3 v in drawPoints)
        {
            if (v.x > max_x)
            {
                max_x = v.x;
            }

            if (v.x < min_x)
            {
                min_x = v.x;
            }

            if (v.y > max_y)
            {
                max_y = v.y;
            }

            if (v.y < min_y)
            {
                min_y = v.y;
            }
        }

        //計算正規scale
        float xTemp = max_x - min_x;
        float yTemp = max_y - min_y;
        med_x = (max_x + min_x) / 2;
        med_y = (max_y + min_y) / 2;


        if (xTemp > yTemp)
        {
            nScale = 9 / (xTemp);
        }
        else
        {
            nScale = 9 / (yTemp);

        }


        foreach (Vector3 v in drawPoints)
        {
            data[(int)((v.y - med_y) * nScale + dataSize / 2), (int)((v.x - med_x) * nScale + dataSize / 2)] = 1;
        }


        setPattern(data);//畫出圖形
       
        PR.Train(dataToIntArray(data));//訓練該筆資料
        expectNumber = 0;//重置
        return dataToIntArray(data);
    }
    //------------------------------------------------------------------------------------------------------
    //dataToIntArray------------------------------------------------------------------------------------------ 
    int[] dataToIntArray(int[,] data)
    {
        int[] s = new int[data.Length+1];
        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                s[i* data.GetLength(0)+j] = data[i, j];
            }
        }
        s[data.Length] = expectNumber;
        return s;
    }
    //------------------------------------------------------------------------------------------------------ 
    //dataToString------------------------------------------------------------------------------------------ 
    string dataToString(int[,] data)
    {
        string s = "";
        for (int i = 0; i < data.GetLength(0); i++)
        {
            for (int j = 0; j < data.GetLength(1); j++)
            {
                s += data[i, j] + " ";
            }
        }
        s += expectNumber;
        return s;
    }
    //------------------------------------------------------------------------------------------------------ 
    
   
}
