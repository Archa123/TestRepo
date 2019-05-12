/*
 * This function is not intended to be invoked directly. Instead it will be
 * triggered by an orchestrator function.
 * 
 * Before running this sample, please:
 * - create a Durable orchestration function
 * - create a Durable HTTP starter function
 */

#r "Microsoft.Azure.WebJobs.Extensions.DurableTask"
#r "Newtonsoft.Json"
#load "properties.csx"

using System;
using System.Net;
using System.Net.Http;
using System.Configuration;
using System.Security.Claims; 
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static void Run(Tuple<string, string> tuple1, ILogger log)
{
   log.LogInformation("Activity function started...");
   string resource_id = tuple1.Item1;
   string stoken = tuple1.Item2;
   
   log.LogInformation("Remediation started for resource with ID: ");
   log.LogInformation(resource_id);

   var httpClient = new HttpClient();
   httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer " + stoken);

   string endpoint = "https://management.azure.com";
   string jsonContent;
   PropertiesEncrypt PropEnc = new PropertiesEncrypt();
   endpoint = endpoint + resource_id + "/transparentDataEncryption/current?api-version=2014-04-01";
   log.LogInformation("URL for API: "+endpoint);
   PropEnc.status =  "Enabled";
   EncryptionProp ep = new EncryptionProp();
   ep.properties = PropEnc;
   jsonContent = JsonConvert.SerializeObject(ep);  

   var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

   var response = httpClient.PutAsync(endpoint, content).Result;
   string result = response.Content.ReadAsStringAsync().Result.ToString();

   log.LogInformation("SQL setting:" + result);
   var statuscode = response.StatusCode.ToString();

   log.LogInformation("Status code for API:" + statuscode);

    /*test*/
}