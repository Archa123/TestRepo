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

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

class ClientDetail
{
    public string grant_type = "client_credentials";
    //public string client_id  = Environment.GetEnvironmentVariable(DIRECTORY_ID);
    public string client_id =  Environment.GetEnvironmentVariable("CLIENT_ID", EnvironmentVariableTarget.Process);
    public string client_secret = Environment.GetEnvironmentVariable("CLIENT_SECRET");
    public string resource = "https://management.azure.com/";
    public string tenant_id = Environment.GetEnvironmentVariable("DIRECTORY_ID");
}

class Result {
    public string token_type {get;set;}
    public int expires_in {get;set;}
    public int ext_expires_in {get;set;}
    public string expires_on{get;set;}
    public string not_before {get;set;}
    public string resource {get;set;}
    public string access_token {get;set;}
}

private static string GetAccessToken(ILogger log1)
{
    ClientDetail cd = new ClientDetail();    
    
    log1.LogInformation("Enter Get Access Token Function!!");
    var keyValues = new List<KeyValuePair<string, string>>();
    keyValues.Add(new KeyValuePair<string, string>("grant_type", cd.grant_type));

    log1.LogInformation("client_id :" +cd.client_id);
    keyValues.Add(new KeyValuePair<string, string>("client_id", cd.client_id));

    log1.LogInformation("client_secret :" +cd.client_secret);
    keyValues.Add(new KeyValuePair<string, string>("client_secret", cd.client_secret));
    keyValues.Add(new KeyValuePair<string, string>("resource", cd.resource));

    var httpClient = new HttpClient();
    var response = httpClient.PostAsync("https://login.microsoftonline.com/" + cd.tenant_id +"/oauth2/token", new FormUrlEncodedContent(keyValues)).Result;
    string result = response.Content.ReadAsStringAsync().Result.ToString();    
    Result rs = JsonConvert.DeserializeObject<Result>(result);
    return rs.access_token;    
}


public static void Run(DurableOrchestrationContext context, ILogger log)
{
    log.LogInformation("Entered sub orchestrator function!!");
    Tuple<string, string> mydata = context.GetInput<Tuple<string,string>>();
    
    string activity_func = mydata.Item1;
    log.LogInformation("Activity Function to trigger: "+activity_func);

    string resource_id = mydata.Item2;
    log.LogInformation("Resource ID: "+resource_id);
    
    log.LogInformation("Generating access token: ");
    string stoken = GetAccessToken(log);
    log.LogInformation("Token: "+ stoken);

    var apidata = Tuple.Create(resource_id,stoken);

    context.CallActivityAsync("RemediateSQLDB", apidata);
   
    
}