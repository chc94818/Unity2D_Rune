
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


//類神經感知機節點
class Perceptron
    {
        //宣告-----------------------------------------------------------------------------------------------
        double[] lastInput;//上一次收到的輸入
        double[] weight; //輸出向下一層的權重
        double output;//輸出
        double delta;//變化
        double[] weightDelta;//權重變動
        double a = 0.3f;//慣性係數
        double learn;//學習率
        //--------------------------------------------------------------------------------------------------


        public double[] getWeight()
        {
            return weight;
        }

        public double getOutput()
        {
            return output;
        }

        public double getDelta()
        {
            return delta;
        }
        //WEIGHT初始化，無WEIGHT DATA，RANDOM初始化---------------------------------------------
        public void weightInit(int weightNum, double learn)
        {
            weightDelta = new double[weightNum];//初始化weightDelta
            weight = new double[weightNum];//初始化weight
            this.learn = learn;
            //逐個初始weight的權重
            for (int i = 0; i < weight.Length; i++)
            {
            weight[i] = Random.Range(-1, 1);
            }
        }
        //--------------------------------------------------------------------------------------
        //WEIGHT初始化，有WEIGHT DATA，直接存入WEIGHT DATA----------------------------------------
        public void weightInit(int weightNum, double learn, double[] weightData)
        {
            weightDelta = new double[weightNum];//初始化weightDelta
            weight = new double[weightNum];//初始化weight
            this.learn = learn;
            //逐個初始weight的權重
            for (int i = 0; i < weight.Length; i++)
            {
                weight[i] = weightData[i];
            }
        }
        //--------------------------------------------------------------------------------------

        //計算輸出
        public double cal(double[] input)
        {
            lastInput = input;
            double temp = 0;//暫存計算結果

            //輸入跟權重做內積計算

            for (int i = 0; i < weight.Length; i++)
            {
                // Console.WriteLine("input: " + input[i]);
                temp += weight[i] * input[i];

            }
            // Console.WriteLine(temp + " ");
            //類神經模糊函數            
            output = 1;
            output /= (1 + Mathf.Exp((float)-temp));

            return output;
        }
        //--------------------------------------------------------------------------------------

        // output
        public double backPropagation(double expectDelta)
        {

            delta = (output) * (1 - output) * (expectDelta);
            for (int i = 0; i < weight.Length; i++)
            {
                weightDelta[i] = (a * weightDelta[i]) + (learn * delta * lastInput[i]);
                weight[i] += weightDelta[i];
            }

            return delta * delta;

        }
        //--------------------------------------------------------------------------------------

    }

