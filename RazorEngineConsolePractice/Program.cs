using RazorEngine;
using RazorEngine.Templating;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Permissions;
using System.Security.Policy;
using System.Security;
using RazorEngine.Configuration;
using System.IO;

namespace RazorEngineConsolePractice
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Title { get; set; }
    }

    internal class Program
    {
        // 多個人的資料
        static List<Person> people = new List<Person>
        {
            new Person { Name = "John", Age = 30, Title = "Mr." },
            new Person { Name = "Jane", Age = 28, Title = "Ms." },
            new Person { Name = "Bob", Age = 35, Title = "Dr." }
        };


        public static void Main()
        {
            //CompileFromString();

            CompileFromTemplateFile();

            //手動清除暫檔的方式
            CleanupTempFiles();

            //使用domain控制的方式自動刪除暫存檔的方式，但是實驗後沒有作用

            //if (AppDomain.CurrentDomain.IsDefaultAppDomain())
            //{
            //    // RazorEngine cannot clean up from the default appdomain...
            //    Console.WriteLine("Switching to secound AppDomain, for RazorEngine...");
            //    AppDomainSetup adSetup = new AppDomainSetup();
            //    adSetup.ApplicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            //    var current = AppDomain.CurrentDomain;
            //    // You only need to add strongnames when your appdomain is not a full trust environment.
            //    var strongNames = new StrongName[0];

            //    var domain = AppDomain.CreateDomain("MyMainDomain", null,
            //current.SetupInformation, new PermissionSet(PermissionState.Unrestricted),
            //strongNames);
            //    //var exitCode = domain.ExecuteAssembly(Assembly.GetExecutingAssembly().Location);

            //    // RazorEngine will cleanup.
            //    AppDomain.Unload(domain);
            //}
        }

        private static void CleanupTempFiles()
        {
            string tempFolder = Path.GetTempPath();
            string[] razorEngineDirs = Directory.GetDirectories(tempFolder, "RazorEngine_*");
            foreach (var dir in razorEngineDirs)
            {
                try
                {
                    Directory.Delete(dir, true);
                    Console.WriteLine($"Deleted: {dir}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deleting {dir}: {ex.Message}");
                }
            }
        }

        private static void CompileFromTemplateFile()
        {
            //將Template存入Cache以利重複使用
            Engine.Razor.AddTemplate(
                "MailBody", // Cache Key
                System.IO.File.ReadAllText("template.cshtml"));


            // 使用 RazorEngine 來產生每個人的 HTML
            foreach (var person in people)
            {                
                //傳入Cache Key、Model物件型別、Model物件取得套表結果
                var result = Engine.Razor.RunCompile("MailBody", typeof(Person), person);

                //除了RunCompile，也可Compile一次，Run多次以提高效能
                Engine.Razor.Compile("MailBody", typeof(Person));
                Engine.Razor.Run("MailBody", typeof(Person), person);

                Console.WriteLine(result);
            }
        }

        /// <summary>
        /// 從字串中進行編譯的方式
        /// </summary>
        private static void CompileFromString()
        {
            // 輸出的 HTML 字串
            string result = "";

            // 使用 RazorEngine 來產生每個人的 HTML
            foreach (var person in people)
            {
                string template = @"<!DOCTYPE html>
                                <html>
                                <head>
                                    <title>@Model.Title @Model.Name</title>
                                </head>
                                <body>
                                    <h1>Hello, @Model.Title @Model.Name!</h1>
                                    <p>Age: @Model.Age</p>
                                </body>
                                </html>";

                // 使用 RazorEngine 產生資料
                string html = Engine.Razor.RunCompile(template, Guid.NewGuid().ToString(), null, person);
                result += html;
            }

            Console.WriteLine(result);
        }
    }
}
