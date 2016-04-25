using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SocialPoint.Tool.Server;
using System.Diagnostics;
using SocialPoint.Attributes;

public static class UnityCLI
{
    public static void help()
    {
        string help_txt = @"UnityCLI:
        ---
        configFile= Path to the JSON file used to configure the running command.
        
        executionLogFile= Path to the file where the execution results will be written.
        ";
        System.Console.Write(help_txt);
    }

    public static void run()
    {
        System.Console.WriteLine("[BEGIN]------------------------------------------");

        UnityCLIParameters.help();
        UnityCLIParameters parameters = UnityCLIParameters.parse(System.Environment.GetCommandLineArgs());
        perform(parameters);

        System.Console.WriteLine("[END]--------------------------------------------");
    }
	
    public static void perform(UnityCLIParameters parameters)
    {
        ToolServiceResults results = new ToolServiceResults();
        ToolServiceDelegate service;

        try
        {
            System.Console.WriteLine("Reading configFile: " + parameters.configFile);
            UnityEngine.Debug.Log("Reading configFile: " + parameters.configFile);
    		
            string configFileContents = Utils.GetFileContents(parameters.configFile);
    	
            // Parse request configuration
            var commonParameters = ToolServiceParameters.Instantiate(configFileContents, typeof(ToolServiceParameters));
            var serviceParameters = ToolServiceDelegateFactory.parseParameters(commonParameters.commandName, configFileContents);

            // Perform call to Tool Service
            service = ToolServiceDelegateFactory.create(commonParameters.commandName);
            service.perform(serviceParameters);

            results = service.LogResults;
        }
        catch(Exception e)
        {
            string error_message = e.Message.Replace("\"", "'") + " " + e.StackTrace.Replace("\"", "'").Replace("\n", "").Replace("\t", "");
            results.MarkAsFailed(error_message);
            throw;
        }
        finally
        {
            DateTime currTime = DateTime.Now;
            results.execution_date = currTime.ToLongDateString() + ", " + currTime.ToLongTimeString();
            Utils.SetFileContents(parameters.executionLogFile, results.ToJson(), create: true);
        }
    }
}
