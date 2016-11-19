using System;
using System.Collections.Generic;

using UnityEngine;

    //多層類神經感知機
    class MLP
    {
        //宣告-----------------------------------------------------------------------------------------------        
        //double rmse;//方差
        fileControl fc;//建立fileControl來進行檔案操作
        double sum; //計算資料筆數
        double hit; //命中資料筆數
        double hitRate;//命中率
        List<double[]> weightData = new List<double[]>();//每一個神經元的權重
        List<Perceptron[]> mlp = new List<Perceptron[]>();//多層類神經網路LIST mlp[0] = 輸入層 mlp[n] = 第n層
        //--------------------------------------------------------------------------------------------------
        //MLP CONSTRUCT-------------------------------------------------------------------------------------
        //int[] layer 表示隱藏層跟輸出層的神經元個數
        public MLP(int[] layer, int dimension, double learn,string DPath)
        {
            fc = new fileControl(DPath);
            weightData = fc.WeightDataRead();//儲存各神經元權重
            int wdPointer = 0;//weight data指標

            //建立每一層的神經元
            //建立輸入層神經元
            Perceptron[] pTemp;
            pTemp = new Perceptron[dimension];//輸入層神經元數等於輸入維度
            int weightNum = dimension;//每一個神經元連接到上一層所需的連結數          

            //初始化輸入層神經元
            for (int j = 0; j < pTemp.Length; j++)
            {
                pTemp[j] = new Perceptron();
                pTemp[j].weightInit(weightNum + 1, learn, weightData[wdPointer++]);//多一維度是 w0 = 閥值
            }
            mlp.Add(pTemp); //將初始成功的神經元放入mlp list

            for (int i = 0; i < layer.Length; i++)
            {
                pTemp = new Perceptron[layer[i]];
                if (i != 0)//除了輸入曾與第一層，其餘的權重數為上一層的神經元數量
                {
                    weightNum = layer[i - 1];
                }
                //初始化神經元
                for (int j = 0; j < pTemp.Length; j++)
                {
                    pTemp[j] = new Perceptron();
                    pTemp[j].weightInit(weightNum + 1, learn, weightData[wdPointer++]);//多一維度是 w0 = 閥值
                }
                mlp.Add(pTemp); //將初始成功的神經元放入mlp list
            }
        }
        //--------------------------------------------------------------------------------------------------
        //訓練權重-------------------------------------------------------------------------------------------
        public void Train(List<int[]> trainData)
        {
         
            //對每一組DATA做運算
            foreach (int[] td in trainData)
            {

                //前饋階段-------------------------------------------------------------------------------------------  
                //輸入資料為 訓練資料的 0~N-1，第N個為expect
                //輸入資料的 inputData[0] = -1 用來跟w0計算閥值
                double[] inputData = new double[td.Length];
                inputData[0] = -1;
                for (int i = 1; i < inputData.Length; i++)
                {
                    inputData[i] = td[i - 1];
                }

                //前饋開始
                foreach (Perceptron[] p in mlp)
                {
                    double[] outputTemp = new double[p.Length + 1];//多一個維度是-1 用來計算閥值
                    outputTemp[0] = -1;//第一個值為-1 用來跟w0計算閥值
                    for (int i = 0; i < p.Length; i++)
                    {
                        outputTemp[i + 1] = p[i].cal(inputData);
                    }
                    inputData = outputTemp;
                }


                //--------------------------------------------------------------------------------------------------
                //倒傳遞階段-----------------------------------------------------------------------------------------
                //將期望結果換算成二進制
                int[] codeTemp = Encoder(td[td.Length - 1]);
                double[] expect = new double[codeTemp.Length];
                for (int i = 0; i < expect.Length; i++)
                {
                    expect[i] = codeTemp[i];//獲得各輸出點期望結果
                }

                double[] deltaTemp = new double[mlp[mlp.Count - 1][0].getWeight().Length];//儲存給一上層的delta值
                //計算輸出層的delta並進入倒傳遞處理
                for (int i = 0; i < mlp[mlp.Count - 1].Length; i++)
                {
                    double[] weight = mlp[mlp.Count - 1][i].getWeight(); //獲得上一輪權重
                    double expectDelta = expect[i] - mlp[mlp.Count - 1][i].getOutput();//計算其期望差異
                    mlp[mlp.Count - 1][i].backPropagation(expectDelta);//倒傳遞運算更新權重
                    //利用上一輪權重*DELTA計算上一層的期望差異
                    for (int j = 0; j < weight.Length; j++)
                    {
                        deltaTemp[j] += weight[j] * mlp[mlp.Count - 1][i].getDelta();
                    }
                }
                //計算輸入層跟隱藏層的delta並進入倒傳遞處理
                for (int i = mlp.Count - 2; i >= 0; i--)
                {
                    double[] expectDelta = deltaTemp;//期望差異
                    for (int j = 0; j < mlp[i].Length; j++)
                    {
                        double[] weight = mlp[i][j].getWeight();//獲得上一輪權重
                        mlp[i][j].backPropagation(expectDelta[j]);//倒傳遞運算更新權重
                        //利用上一輪權重*DELTA計算上一層的期望差異
                        for (int k = 0; k < weight.Length; k++)
                        {
                            deltaTemp[k] += weight[k] * mlp[i][j].getDelta();
                        }
                    }
                }

                //--------------------------------------------------------------------------------------------------
            }

        }
    //--------------------------------------------------------------------------------------------------
    //訓練單筆資料-------------------------------------------------------------------------------------------
    public void Train(int[] trainData)
    {

        //對一筆DATA做運算
            //前饋階段-------------------------------------------------------------------------------------------  
            //輸入資料為 訓練資料的 0~N-1，第N個為expect
            //輸入資料的 inputData[0] = -1 用來跟w0計算閥值
            double[] inputData = new double[trainData.Length];
            inputData[0] = -1;
            for (int i = 1; i < inputData.Length; i++)
            {
                inputData[i] = trainData[i - 1];
            }

            //前饋開始
            foreach (Perceptron[] p in mlp)
            {
                double[] outputTemp = new double[p.Length + 1];//多一個維度是-1 用來計算閥值
                outputTemp[0] = -1;//第一個值為-1 用來跟w0計算閥值
                for (int i = 0; i < p.Length; i++)
                {
                    outputTemp[i + 1] = p[i].cal(inputData);
                }
                inputData = outputTemp;
            }


            //--------------------------------------------------------------------------------------------------
            //倒傳遞階段-----------------------------------------------------------------------------------------
            //將期望結果換算成二進制
            int[] codeTemp = Encoder(trainData[trainData.Length - 1]);
            double[] expect = new double[codeTemp.Length];
            for (int i = 0; i < expect.Length; i++)
            {
                expect[i] = codeTemp[i];//獲得各輸出點期望結果
            }

            double[] deltaTemp = new double[mlp[mlp.Count - 1][0].getWeight().Length];//儲存給一上層的delta值
                                                                                      //計算輸出層的delta並進入倒傳遞處理
            for (int i = 0; i < mlp[mlp.Count - 1].Length; i++)
            {
                double[] weight = mlp[mlp.Count - 1][i].getWeight(); //獲得上一輪權重
                double expectDelta = expect[i] - mlp[mlp.Count - 1][i].getOutput();//計算其期望差異
                mlp[mlp.Count - 1][i].backPropagation(expectDelta);//倒傳遞運算更新權重
                                                                   //利用上一輪權重*DELTA計算上一層的期望差異
                for (int j = 0; j < weight.Length; j++)
                {
                    deltaTemp[j] += weight[j] * mlp[mlp.Count - 1][i].getDelta();
                }
            }
            //計算輸入層跟隱藏層的delta並進入倒傳遞處理
            for (int i = mlp.Count - 2; i >= 0; i--)
            {
                double[] expectDelta = deltaTemp;//期望差異
                for (int j = 0; j < mlp[i].Length; j++)
                {
                    double[] weight = mlp[i][j].getWeight();//獲得上一輪權重
                    mlp[i][j].backPropagation(expectDelta[j]);//倒傳遞運算更新權重
                                                              //利用上一輪權重*DELTA計算上一層的期望差異
                    for (int k = 0; k < weight.Length; k++)
                    {
                        deltaTemp[k] += weight[k] * mlp[i][j].getDelta();
                    }
                }
            }

            //--------------------------------------------------------------------------------------------------
        
    }
    //--------------------------------------------------------------------------------------------------

    //測試結果-------------------------------------------------------------------------------------------
    public void Test(List<int[]> testData)
        {
            //SUM = 測試總次數  HIT = 命中次數   hitRate = 命中率
            sum = 0;
            hit = 0;
            hitRate = 0;
            //double eTemp;
            //rmse = 0;
            //對每一組DATA做運算
            foreach (int[] td in testData)
            {
                sum++;//計算了一筆資料
                //前饋階段-------------------------------------------------------------------------------------------  
                //輸入資料為 測試資料的 0~N-1，第N個為expect
                //輸入資料的 inputData[0] = -1 用來跟w0計算閥值
                double[] inputData = new double[td.Length];
                inputData[0] = -1;
                for (int i = 1; i < inputData.Length; i++)
                {
                    inputData[i] = td[i - 1];
                }

                //前饋開始
                foreach (Perceptron[] p in mlp)
                {
                    double[] outputTemp = new double[p.Length + 1];//多一個維度是-1 用來計算閥值
                    outputTemp[0] = -1;//第一個值為-1 用來跟w0計算閥值
                    for (int i = 0; i < p.Length; i++)
                    {
                        outputTemp[i + 1] = p[i].cal(inputData);
                    }
                    inputData = outputTemp;//這一輪的輸出結果 = 下一輪的輸入
                }


                //--------------------------------------------------------------------------------------------------
                //判斷答案階段-----------------------------------------------------------------------------------------
                //將計算結果換算成十進制
                int[] encode = new int[mlp[mlp.Count - 1].Length];
                for (int i = 0; i < mlp[mlp.Count - 1].Length; i++)
                {
                    encode[i] = (int)(mlp[mlp.Count - 1][i].getOutput() + 0.5);//換算成結果
                }

                int result = Decoder(encode);//換算成十進制結果
                if (result == td[td.Length - 1])//結果相同則命中
                {
                    hit++;//命中了一筆資料
                }

                //--------------------------------------------------------------------------------------------------
            }
            hitRate = hit * 100 / sum;//計算命中率
            //Console.WriteLine("hit rate is : " + hitRate + " %");
        }
    //--------------------------------------------------------------------------------------------------

    //圖形判斷-------------------------------------------------------------------------------------------
    public int Recognize(int[] patternData)
    {
       
        //rmse = 0;
        //對一組DATA做運算
        
            //前饋階段-------------------------------------------------------------------------------------------  
            //輸入資料為 測試資料的 0~N-1，第N個為expect
            //輸入資料的 inputData[0] = -1 用來跟w0計算閥值
            double[] inputData = new double[patternData.Length];
            inputData[0] = -1;
            for (int i = 1; i < inputData.Length; i++)
            {
                inputData[i] = patternData[i - 1];
            }

            //前饋開始
            foreach (Perceptron[] p in mlp)
            {
                double[] outputTemp = new double[p.Length + 1];//多一個維度是-1 用來計算閥值
                outputTemp[0] = -1;//第一個值為-1 用來跟w0計算閥值
                for (int i = 0; i < p.Length; i++)
                {
                    outputTemp[i + 1] = p[i].cal(inputData);
                }
                inputData = outputTemp;//這一輪的輸出結果 = 下一輪的輸入
            }


            //--------------------------------------------------------------------------------------------------
            //判斷答案階段-----------------------------------------------------------------------------------------
            //將計算結果換算成十進制
            int[] encode = new int[mlp[mlp.Count - 1].Length];
            for (int i = 0; i < mlp[mlp.Count - 1].Length; i++)
            {
                encode[i] = (int)(mlp[mlp.Count - 1][i].getOutput() + 0.5);//換算成結果
            }

            int result = Decoder(encode);//換算成十進制結果
            return result;

            //--------------------------------------------------------------------------------------------------
        
    }
    //--------------------------------------------------------------------------------------------------


    //解碼器，將輸出的二進位碼解碼成十進位----------------------------------------------------------------

    public int Decoder(int[] code)
        {
            int pattern = 0;
            for (int i = 0; i < code.Length; i++)
            {
                if (code[i] == 1)
                {
                    pattern += (int)Math.Pow(2, i);
                }

            }


            return pattern;
        }
        //--------------------------------------------------------------------------------------------------
        //加碼器，將輸入的十進位碼加碼成二進位-----------------------------------------------------------------

        public int[] Encoder(int pattern)
        {
            int[] code = new int[mlp[mlp.Count - 1].Length];
            for (int i = 0; i < code.Length; i++)
            {
                code[i] = pattern % 2;
                pattern /= 2;
            }

            return code;
        }
        //--------------------------------------------------------------------------------------------------

        //匯出WEIGHT DATA-----------------------------------------------------------------------------------
        public void exportWeight()
        {
            List<string> weightData = new List<string>();
            String wString = "";
            foreach (Perceptron[] p in mlp)
            {
                for (int i = 0; i < p.Length; i++)
                {
                    wString = "";
                    double[] weightTemp = p[i].getWeight();
                    for (int j = 0; j < weightTemp.Length; j++)
                    {
                        if (j == weightTemp.Length - 1)
                        {
                            wString += weightTemp[j];
                        }
                        else
                        {
                            wString += weightTemp[j] + " ";
                        }
                    }
                    weightData.Add(wString);
                }
            }

            fc.weightDataWrite(weightData);
        }
        //--------------------------------------------------------------------------------------------------
    }

