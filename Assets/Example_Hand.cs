
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Example_Hand : MonoBehaviour
{

    //線段設定---------------------------------------------------------------------------
    private bool isPressed;
    public GameObject lineDrawPrefabs; // this is where we put the prefabs object
    private GameObject lineDrawPrefab;
    private LineRenderer lineRenderer;
    private List<Vector3> drawPoints = new List<Vector3>();
    //-----------------------------------------------------------------------------------

    //測試設定---------------------------------------------------------------------------
    public bool isTest = false;
    public GameObject lineDrawTestPrefabs; // this is where we put the prefabs object
    private GameObject lineDrawTestPrefab;
    private LineRenderer lineTestRenderer;
    private List<Vector3> drawTestPoints = new List<Vector3>();
    //-----------------------------------------------------------------------------------

    //符文設定----------------------------------------------------------------------------
    //最大符文筆畫
    private int maxStroke = 10;

    public string[] rBit = { "0",
    /*                       Void*/
                             "161", "682","717", "727", "852", "825" ,
    /*                        Aer   Ignis  Terra  Aqua  Ordo   Perdito*/
                             "7135", "3858", "8525"};
    /*                        Space   Over  Time */
    public string[] rune = { "Void",
                            "Aer","Ignis", "Terra", "Aqua", "Ordo" ,"Perdito",
                            "Space", "Over", "Time"};

    //儲存當下的符文各筆畫方向
    public string pattern;
    //儲存當下點筆畫方向，來決定是否產生了新的筆畫
    public int[] pBuffer = new int[4];
    //儲存角  前一個角  可能的新角  最新的點
    public Vector2[] vBuffer = new Vector2[3];
    //前角度 後角度  角度變化量
    public float angle1, angle2, angle3;
    public int pt1, pt2, pt3;
    //pattern記錄點數量
    public int pCount = 0;

    //長度過濾
    private float distThreshold = 0.2f;

    //-----------------------------------------------------------------------------------

    //畫面上的text
    public Text dt;
    

    //設置text 顯示符文種類
    void setRune(int index)
    {
        dt.text = rune[index];
    }


    //初始化
    void Start()
    {
        isPressed = false;
    }

    //每一張frame會呼叫一次update
    void Update()
    {
        //偵測是使用滑鼠還是觸控面板
        #if UNITY_EDITOR || UNITY_STANDALONE
            MouseInput();   // 滑鼠偵測
        #elif UNITY_ANDROID
		    MobileInput();  // 觸碰偵測
        #endif

       
    }
  

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

            // 刪除測試線-------------------------------------------------------------------------------------
            if (isTest)
            {
                deleteLine("LineDrawTest");
            }
            //------------------------------------------------------------------------------------------------


            //將座標轉成世界座標
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));


            //設定實際線-----------------------------------------------------------------------------------------
            lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
            lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
            lineRenderer.SetVertexCount(0);
            //------------------------------------------------------------------------------------------------
            //設定測試線-----------------------------------------------------------------------------------------
            if (isTest)
            {
                lineDrawTestPrefab = GameObject.Instantiate(lineDrawTestPrefabs) as GameObject;
                lineTestRenderer = lineDrawTestPrefab.GetComponent<LineRenderer>();
                lineTestRenderer.SetVertexCount(0);
            }
            //------------------------------------------------------------------------------------------------
            //偵測目前的pattern
            setPattern(new Vector2(mousePos.x, mousePos.y), false);
            isPressed = true;
           
        }
        //----------------------------------------------------------------------------------------------------
        //放開左鍵--------------------------------------------------------------------------------------------
        else if (Input.GetMouseButtonUp(0))
        {
            //將座標轉成世界座標
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));


            //判斷符文
            setPattern(new Vector2(mousePos.x, mousePos.y),true);
            patternRecognition();


            //設定成未按著滑鼠
            isPressed = false;

            //清楚目前點資料
            drawPoints.Clear();
            drawTestPoints.Clear();


            // 刪除畫下的線-------------------------------------------------------------------------------------
            if (!isTest)
            {
                deleteLine("LineDraw");
            }           
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
            //判斷符文
            setPattern(new Vector2(mousePos.x, mousePos.y), false);            
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

                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 1.0f));

                //設定實際線-----------------------------------------------------------------------------------------
                lineDrawPrefab = GameObject.Instantiate(lineDrawPrefabs) as GameObject;
                lineRenderer = lineDrawPrefab.GetComponent<LineRenderer>();
                lineRenderer.SetVertexCount(0);
                //------------------------------------------------------------------------------------------------
                //設定測試線-----------------------------------------------------------------------------------------
                if (isTest)
                {
                    lineDrawTestPrefab = GameObject.Instantiate(lineDrawTestPrefabs) as GameObject;
                    lineTestRenderer = lineDrawTestPrefab.GetComponent<LineRenderer>();
                    lineTestRenderer.SetVertexCount(0);
                }
                //------------------------------------------------------------------------------------------------
                //偵測目前的pattern
                setPattern(new Vector2(mousePos.x, mousePos.y), false);
                isPressed = true;
            }
            //------------------------------------------------------------------------------------------------------
            //放開螢幕-------------------------------------------------------------------------------------------
            else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[0].phase == TouchPhase.Canceled)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 1.0f));

                //判斷符文
                setPattern(new Vector2(mousePos.x, mousePos.y), true);
                patternRecognition();


                //設定成未按著滑鼠
                isPressed = false;

                //清楚目前點資料
                drawPoints.Clear();
                drawTestPoints.Clear();

                // 刪除實際線-------------------------------------------------------------------------------------
                deleteLine("LineDraw");
                //------------------------------------------------------------------------------------------------
                // 刪除測試線-------------------------------------------------------------------------------------
                if (isTest)
                {
                    deleteLine("LineDrawTest");
                }
                //------------------------------------------------------------------------------------------------

            }
            //------------------------------------------------------------------------------------------------------
            //持續壓著左鍵-------------------------------------------------------------------------------------------
            if (isPressed)
            {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 1.0f));

                //畫實際線----------------------------------------------------------------------------------------------
                drawPoints.Add(mousePos);
                lineRenderer.SetVertexCount(drawPoints.Count);
                lineRenderer.SetPosition(drawPoints.Count - 1, mousePos);
                //-------------------------------------------------------------------------------------------------------
                //判斷符文
                setPattern(new Vector2(mousePos.x, mousePos.y), false);
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

    //設置當下點的pattern---------------------------------------------------------------------------------------------------
    //v2 當下點擊位置
    //isEnd 是否為終點(終點必定要畫)
    void setPattern(Vector2 v,bool isEnd)
    {
        if (pCount == 0) {
            vBuffer[0] = new Vector2(v.x, v.y);
            vBuffer[1] = new Vector2(v.x, v.y);
            vBuffer[2] = new Vector2(v.x, v.y);
            pattern= "";
            pCount++;
            //起點畫測試線
            if (isTest)
            {
                drawTestPoints.Add(new Vector3(v.x, v.y + 0.5f, -10));
                lineTestRenderer.SetVertexCount(drawTestPoints.Count);
                lineTestRenderer.SetPosition(drawTestPoints.Count - 1, new Vector3(v.x, v.y + 0.5f, -10));
            }
            
            return;
        }

       
        if(isEnd) {
            //終點畫測試線
            if (isTest)
            {
                drawTestPoints.Add(new Vector3(v.x, v.y + 0.5f, -10));
                lineTestRenderer.SetVertexCount(drawTestPoints.Count);
                lineTestRenderer.SetPosition(drawTestPoints.Count - 1, new Vector3(v.x, v.y + 0.5f, -10));
            }
        }


        if (getDist(v, vBuffer[0])>distThreshold)
        {
            vBuffer[0] = v;
            angle1 = getAngle(vBuffer[1], vBuffer[0]);
            pt1 = ((int)(angle1 + 22.5) / 45) % 8 + 1;
            if (pt3 > 4)
            {
                pt3 = -pt3 + 8;
            }

            //根據PT (方向) 來決定是否有轉角
            if(pBuffer[0]!= pt1)
            {
                if(pBuffer[1] != pt1)
                {
                    pBuffer[1] = pt1;
                }else
                {
                    for (int i = 2; i < pBuffer.Length; i++)
                    {
                        if (pBuffer[i] == pt1)
                        {
                            if (i != pBuffer.Length - 1)
                            {
                                continue;
                            }
                            else
                            {
                                if (pCount >= maxStroke)
                                {
                                    runeInitial();
                                    return;
                                }

                                //畫測試線---------------------------------------------------------------------------------------
                                if (pCount != 1 && isTest)
                                {

                                    drawTestPoints.Add(new Vector3(vBuffer[1].x , vBuffer[1].y + 0.5f, -10));
                                    lineTestRenderer.SetVertexCount(drawTestPoints.Count);
                                    lineTestRenderer.SetPosition(drawTestPoints.Count - 1, new Vector3(vBuffer[1].x , vBuffer[1].y + 0.5f, -10));
                                }
                                else
                                {
                                    vBuffer[2] = vBuffer[1];
                                }
                                //----------------------------------------------------------------------------------------------
                               
                                pattern += pt1;
                                pCount++;
                                for (int j = 1; j < pBuffer.Length; j++)
                                {
                                    pBuffer[j] = 0;
                                }
                                pBuffer[0] = pt1;
                            }

                        }
                        else
                        {
                            pBuffer[i] = pt1;
                        }

                    }
                }
                pBuffer[1] = pt1;

            }
            else
            {
                vBuffer[1] = vBuffer[0];
            }      
        }       
    }

    //------------------------------------------------------------------------------------------------------
    //初始符文buffer的值---------------------------------------------------------------------------------------------------
    void runeInitial()
    {
        for (int j = 0; j < pBuffer.Length; j++)
        {
            pBuffer[j] = 0;
        }
        pCount = 0;
       
    }
    //------------------------------------------------------------------------------------------------------
    //計算角度------------------------------------------------------------------------------------------------------
    float getAngle(Vector2 v1, Vector2 v2)
    {
        
        float a;
        float vy, vx = 0;
        vy = (v2.y - v1.y)*100;
        vx = (v2.x - v1.x)*100;
       
        a = Mathf.Atan(vy / vx) * 180 / Mathf.PI;
        if (vx < 0)
        {
            a += 180;
        }
        else if (vy < 0)
        {
            a += 360;
        }
      
        return a;
    }
    //------------------------------------------------------------------------------------------------------
    //計算距離------------------------------------------------------------------------------------------------------
    float getDist(Vector2 v1, Vector2 v2)
    {

        float d;
        float vy, vx = 0;
        vy = (v2.y - v1.y) ;
        vx = (v2.x - v1.x) ;

        d = Mathf.Sqrt(vx*vx+vy*vy);
       

        return d;
    }
    //------------------------------------------------------------------------------------------------------
    //判斷符文種類------------------------------------------------------------------------------------------------------
    int patternRecognition()
    {
        //若非任一種符文  則設為0(void)符文
        setRune(0);
        //Debug.Log("pattern:" + pattern[1]);
        int min = 100;
        int minIndex = -1;
        int temp = 0;

        //跟每一個符文計算其差異程度
        for (int i=0; i < rBit.Length; i++)
        {
            temp = 0;
            //若符文筆畫不同，必定為不同符文
            if (pCount - 1 != rBit[i].Length)
            {
                continue;
            }
            else
            {
                for (int j = 0; j < pCount - 1; j++)
                {
                    temp += Mathf.Abs(pattern[j] - rBit[i][j]);
                }
                if (temp < min)
                {
                    if (temp == 0)
                    {
                        setRune(i);
                        runeInitial();
                        return 0;
                    }
                    min = temp;
                    minIndex = i;
                }
            }
            
        }

        //差異程度過大，該次符文失敗-------------------------------------------------------------------------
      
        if (min > pCount)
        {
            setRune(0);
        }
        else
        {
            setRune(minIndex);
        }

        
        //------------------------------------------------------------------------------------------------------
        runeInitial();
        return 0;
    }
    //------------------------------------------------------------------------------------------------------
}