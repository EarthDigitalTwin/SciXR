<%@ WebService Language="C#" Class="MyService" %>

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;
using System.IO;

[WebService(Namespace = "http://192.168.1.32:9000/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class MyService : System.Web.Services.WebService
{
    public string dataPath = "Data";

    public MyService()
    {
        //
        // TODO: Add any constructor code required
        //
    }

    [WebMethod]
    public int Add(int a, int b)
    {
        Console.Write("Received Add request.\n");
        return a+b;
    }

    [WebMethod]
    public List<String> GetFileNames() {
        Console.Write("Received GetFileNames request.\n");

        string[] files = Directory.GetFiles(dataPath);
        string[] directories = Directory.GetDirectories(dataPath);

        List<String> result = new List<String>();
        foreach (string f in files)
        {
            result.Add(f);
        }
        foreach (string d in directories)
        {
            result.Add(d);
        }

        return result;
    }

    [WebMethod]
    public List<String> GetFile(string path)
    {
        Console.Write("Received GetFile request for file " + path + ".\n");

        string[] output = File.ReadAllLines(path);
        List<String> file = new List<String>();

        foreach (string line in output)
        {
            file.Add(line);
        }

        return file;
    }
}
