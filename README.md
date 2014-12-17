Method-Log
==========

Method Log

================
Example:

....

public void DemoMethod(int Param1,string Param2)
{
   
    LogUtility.Method().AddLog("DemoMethod",
                DateTime.Now,
                "127.0.0.1",
                "user",
                () => Param1,
                () => Param2);
                
  }
  
  ...
