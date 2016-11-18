using System.IO;
using System;
using UnityEngine;
class fileControl
{
   
    StreamWriter swWriter;
    StreamReader srReader;

    public void fileWrite(string file_name,string data)
    {

        try
        {
            swWriter = new StreamWriter(file_name, true);
            //寫入數據
            swWriter.WriteLine(data);
            swWriter.Close();
        }
        catch (Exception e)
        {

            throw e;
        }
    }

    public void fileRead(string file_name)
    {
        string sLine = "";
        try
        {            
            srReader = new StreamReader(file_name);
           

            while ((sLine = srReader.ReadLine()) != null)
            {
                Debug.Log(sLine);
            }
            
            
            srReader.Close();
        }
        catch (Exception e)
        {
            throw e;
        }

    }
}