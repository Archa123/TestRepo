/*
 * This function is not intended to be invoked directly. Instead it will be
 * triggered by an HTTP starter function.
 * 
 * Before running this sample, please:
 * - create a Durable activity function (default name is "Hello")
 * - create a Durable HTTP starter function
 */

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"
#r "Newtonsoft.Json"
#load "model.csx"

using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

private static Tuple<string, string> ReadCheckConfig(string check_id, string path, ILogger log)
{
  var result1 = Tuple.Create("", "");
  Console.WriteLine("Test Console");
  
  FileStream fsSource = new FileStream(path, FileMode.Open, FileAccess.Read);
  using (StreamReader sr = new StreamReader(fsSource))
  {
      while(!sr.EndOfStream)
      {
       string line = sr.ReadLine();
       if (line.Contains(check_id))
        {
           log.LogInformation("Check "+ check_id +" found in config file");
           var fields = line.Split(':');
           string f0 = fields[0].Trim();
           //log.LogInformation("Check ID: "+ f0);
           string f1 = fields[1].Trim();
           //log.LogInformation("Function App: "+ f1);
           string f2 = fields[2].Trim();
           //log.LogInformation("Function Module: "+ f2);

           var result = Tuple.Create(f1, f2);
           return result;

        }else
        {
           log.LogInformation("Error: Could not find the check "+ check_id +" in config file");
           log.LogInformation("Resolve: Manually enter <check_id>:<Function_App>:<Function_Module> ");
           return result1;
        }

      }
      log.LogInformation("Error: The Check Config file is Empty!!");
      return result1;
    }  
}

public static bool HasValue( Tuple<string, string> tuple)
    {
        return !string.IsNullOrEmpty(tuple?.Item1) && !string.IsNullOrEmpty(tuple?.Item2);
    }

private static string GetResourceType(string resourceid)
{ 
    string resourcetype = "could not found";
    if(resourceid.Contains("Microsoft.Sql")){
        if (resourceid.Contains("/databases/"))
        {
            resourcetype = "SQL_DB";
            return resourcetype;
        }

    }
    return resourcetype;
}

private static List<string> GetCheckIDs(List<Check> checks){
    var checkList = new List<string>();
    foreach (Check check in checks)
    {
      string checkid = check.id;
      checkList.Add(checkid);
    }
    return checkList;
}

public static void Run(DurableOrchestrationContext context, ILogger log, ExecutionContext context1)
{
  var event_data = context.GetInput<string>();
  //var outputs = new List<string>();

  //Check for event_data
  Data data = JsonConvert.DeserializeObject<Data>(event_data);
  log.LogInformation("Payload ID" + data.payload.payload_id);
  log.LogInformation("Resource account" + data.payload.resource.account_id);
  log.LogInformation("Resource Name" + data.payload.resource.name);
  log.LogInformation("Resource region" + data.payload.resource.region);
  string resource_type = GetResourceType(data.payload.resource.id);
  log.LogInformation("Resource Type " + resource_type);

  string resource_id = data.payload.resource.id; 
  List<Check> checks = data.payload.checks;
  List<string> checklist = GetCheckIDs(checks);

  //get the path of check config file
  var path = System.IO.Path.Combine(context1.FunctionDirectory, "check_config.csv");  
  log.LogInformation("Check Config file path: "+ path); 
  
  foreach (string check in checklist){
     Tuple<string,string> getFunctionApp = ReadCheckConfig(check, path, log);
     string functionapp = getFunctionApp.Item1;
     string moduleapp = getFunctionApp.Item2;
     bool b1 = HasValue(getFunctionApp);

     if(b1){
         log.LogInformation("Function App to call : "+ functionapp);
         log.LogInformation("Module App to call : "+ moduleapp);

         //Form tuple to call API
         var tuple1 = Tuple.Create(moduleapp, resource_id);
          context.CallSubOrchestratorAsync("SubOrchestratorSQL", tuple1);
     }else{
         log.LogInformation("Error: Pre-requisite Function App or Module Name missing!!");
     }
     /*test*/
    }

     //return outputs;


}