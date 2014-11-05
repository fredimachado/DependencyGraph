using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DependencyGraph
{
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("Something went wrong! Please specify the root folder and the output PNG path.");
                Console.WriteLine("Example: dg.exe C:\\Projects\\ProjectFolder yuml.png");
                return 1;
            }

            var rootFolder = args[0];

            if (!Directory.Exists(rootFolder))
            {
                Console.WriteLine("Root folder does not exist!");
                return 1;
            }

            var yuml = GenerateYUML(rootFolder);

            try
            {
                HttpClient client = new HttpClient();

                client.GetAsync("http://yuml.me/diagram/class/" + SafeYUML(yuml)).ContinueWith(
                    (resp) =>
                    {
                        HttpResponseMessage response = resp.Result;
                        response.EnsureSuccessStatusCode();


                        SaveDiagramAsync(response.Content, args[1]).ContinueWith(
                            (t) =>
                            {
                                Process process = new Process();
                                process.StartInfo.FileName = args[1];
                                process.Start();
                            });
                    }).Wait();

            }
            catch (Exception ex)
            {
                Console.WriteLine("There was an error when writing to the output file: " + ex.Message);
            }

            return 0;
        }

        private static string SafeYUML(string yuml)
        {
            return System.Web.HttpUtility.HtmlEncode(yuml.Replace(Environment.NewLine, ", ")
                                                         .Replace(@"\n", ", ")
                                                         .Replace(",,", ","));
        }

        private static string GenerateYUML(string rootFolder)
        {
            var sb = new StringBuilder();
            var projects = Directory.GetFiles(rootFolder, "*.csproj", SearchOption.AllDirectories);

            foreach (var project in projects)
            {
                var projectName = Path.GetFileNameWithoutExtension(project);

                XElement projectNode = XElement.Load(project);
                XNamespace ns = "http://schemas.microsoft.com/developer/msbuild/2003";
                var dependencies = projectNode.Descendants(ns + "ItemGroup")
                                              .Descendants(ns + "ProjectReference")
                                              .Descendants(ns + "Name")
                                              .Select(e => e.Value);

                foreach (var dependency in dependencies)
                {
                    sb.Append(string.Format("[{0}] -> [{1}]{2}", projectName, dependency, Environment.NewLine));
                }
            }

            return sb.ToString();
        }

        private static Task SaveDiagramAsync(HttpContent content, string filename)
        {
            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None);
                return content.CopyToAsync(fileStream).ContinueWith(
                    (copyTask) =>
                    {
                        fileStream.Close();
                    });
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Close();
                }

                throw;
            }
        }
    }
}
