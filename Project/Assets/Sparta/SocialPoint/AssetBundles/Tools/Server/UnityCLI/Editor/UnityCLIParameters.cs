using System;
using System.Collections;

public sealed class UnityCLIParameters
{
    public string configFile;
    public string executionLogFile;


    public static UnityCLIParameters parse(string[] args)
    {
        UnityCLIParameters parameters = new UnityCLIParameters();
        Hashtable htArgs = new Hashtable();
        int index;

        foreach(string arg in args)
        {
            index = arg.IndexOf("=");
            if(index > 0)
            {
                htArgs.Add(arg.Substring(0, index), arg.Substring(index + 1));
            }
        }

        foreach(DictionaryEntry pair in htArgs)
        {
            System.Console.WriteLine("    *Param: '" + pair.Key + "' --> '" + pair.Value + "'");
        }

        if(!htArgs.Contains("configFile"))
        {
            throw new Exception("Missing parameters: configFile");
        }
        
        if(!htArgs.Contains("executionLogFile"))
        {
            throw new Exception("Missing parameters: executionLogFile");
        }

        parameters.configFile = (string)htArgs["configFile"];
        parameters.executionLogFile = (string)htArgs["executionLogFile"];

        return parameters;
    }

    public static void help()
    {
        string help_txt = @"UnityCLI:
        ---
        configFile= Path to the JSON file used to configure the running command.
        
        executionLogFile= Path to the file where the execution results will be written.
        ";
        System.Console.Write(help_txt);
    }
}
