using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Debug = UnityEngine.Debug;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;


public class NexusRequest : MonoBehaviour
{

    public static IEnumerator PerformRequest(Action<List<NexusObject>> callback) 
    {
        string aqBaseUrl = "https://ideas-digitaltwin.jpl.nasa.gov/nexus";
        string apiUrl = $"{aqBaseUrl}/list";

        Debug.Log("Performing request to " + apiUrl);
        UnityWebRequest www = UnityWebRequest.Get(apiUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("UnityWebRequest failed:");
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("UnityWebRequest succeeded:");
            string responseBody = www.downloadHandler.text;

            // Deserialize the JSON response into objects
            NexusObject[] NexusObjects = JsonConvert.DeserializeObject<NexusObject[]>(responseBody);
            List<NexusObject> NexusList = new List<NexusObject>(NexusObjects);

            // Print the parsed objects
            foreach (NexusObject obj in NexusObjects)
            {
                string shortName = $"Short Name: {obj.shortName}\n";
                Debug.Log(shortName);
            }

            callback(NexusList);
        }
    }
}


    // static async Task Main()
    // {
    //     try
    //     {
    //         // Call the method that performs the HTTP request
    //         await PerformRequest();
    //     }
    //     catch (Exception ex)
    //     {
    //         //Debug.Log("An error occurred: " + ex.Message);
    //     }
    // }
    // public static List<NexusObject> PerformRequest()
    // {
    //     string aqBaseUrl = "https://ideas-digitaltwin.jpl.nasa.gov/nexus/";
    //     string apiUrl = $"{aqBaseUrl}/list";

//     using (HttpClient client = new HttpClient())
//     {
//         HttpResponseMessage response = client.GetAsync(apiUrl);
//         response.EnsureSuccessStatusCode();

//         string responseBody = response.Content.ReadAsStringAsync();

//         // Deserialize the JSON response into objects
//         NexusObject[] NexusObjects = JsonConvert.DeserializeObject<NexusObject[]>(responseBody);

//         // Print the parsed objects
//         foreach (NexusObject obj in NexusObjects)
//         {
//             string shortName = $"Short Name: {obj.shortName}\n";
//             Debug.Log(shortName);
//         }
//         return NexusObjects.ToList();
//     }
// }

// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using System.Linq;

// using RestSharp;
// using Newtonsoft.Json;

// public class NexusRequest {
//     static void Main()
//     {
//         // Call the method that performs the HTTP request
//         PerformRequest();
//     }

//     static void PerformRequest()
//     {
//         string aqBaseUrl = "https://ideas-digitaltwin.jpl.nasa.gov/nexus/";
//         string apiUrl = $"{aqBaseUrl}/list";

//         RestClient client = new RestClient(apiUrl);
//         RestRequest request = new RestRequest(Method.GET);

//         IRestResponse response = client.Execute(request);

//         if (response.IsSuccessful)
//         {
//             string responseBody = response.Content;

//             // Deserialize the JSON response into objects
//             NexusObject[] dataObjects = JsonConvert.DeserializeObject<NexusObject[]>(responseBody);

//             // Print the parsed objects
//             foreach (NexusObject obj in dataObjects)
//             {
//                 Console.WriteLine($"Short Name: {obj.shortName}");
//                 Console.WriteLine($"Title: {obj.title}");
//                 Console.WriteLine($"Tile Count: {obj.tileCount}");
//                 Console.WriteLine($"Start: {obj.start}");
//                 Console.WriteLine($"End: {obj.end}");
//                 Console.WriteLine($"ISO Start: {obj.iso_start}");
//                 Console.WriteLine($"ISO End: {obj.iso_end}");
//                 Console.WriteLine();
//             }
//         }
//         else
//         {
//             Console.WriteLine("An error occurred: " + response.ErrorMessage);
//         }
//     }
// }
