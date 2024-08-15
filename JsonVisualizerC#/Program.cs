using System.Collections;
using System.Drawing;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using ScottPlot;

namespace JsonVisualizerC_
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string apikey = "vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==";

            string apiUrl = $"https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code={apikey}";

            List<Employee> employees = await FetchEmployeeDatAsync(apiUrl);

            Console.WriteLine("Data Fetched Successfully");

            ProcessEmployeeData(employees);

            Console.WriteLine("Completed Successfully");
            Console.WriteLine("open this path to see web page : ");
            Console.WriteLine(@"JsonVisualizerC#/JsonVisualizerC#/bin/Debug/net8.0/WebPage/employees.html");
            Console.ReadLine();
            
        }

        // Get Data From api
        static async Task<List<Employee>> FetchEmployeeDatAsync(string url)
        {
            using HttpClient client = new HttpClient();
            string response = await client.GetStringAsync(url);
            return JsonSerializer.Deserialize<List<Employee>>(response);
        }

        // Calculate Total Working Time and Order by Working Time 
        static void ProcessEmployeeData(List<Employee> employees)
        {
            Dictionary<string, int> employeeWithWorkingTime = new Dictionary<string, int>();

            foreach (Employee employee in employees)
            {
                if (employee.EmployeeName == null || employee.StarTimeUtc == null || employee.EndTimeUtc == null) continue;

                TimeSpan timeSpan = DateTime.Parse(employee.EndTimeUtc) - DateTime.Parse(employee.StarTimeUtc); ;

                _ = employeeWithWorkingTime.ContainsKey(employee.EmployeeName) ? 
                    employeeWithWorkingTime[employee.EmployeeName] += timeSpan.Hours :
                    employeeWithWorkingTime[employee.EmployeeName] = timeSpan.Hours;
            }
            employeeWithWorkingTime = employeeWithWorkingTime.OrderByDescending(e => e.Value).ToDictionary();
            GenerateHtml(employeeWithWorkingTime, "./WebPage/employees.html", "peiChart.png");
            CreateAndSavePieChart(employeeWithWorkingTime, "./WebPage/peiChart.png");
        }

        //Generate HTML Content
        static void GenerateHtml(Dictionary<string, int> employees, string filePath, string imgUrl)
        {
            // Basic Table Structure of Html
            string html = @"<!DOCTYPE html>
                            <html>
                                <head>
                                    <title>Employee Time Report</title>
                                    <link rel=""stylesheet"" href=""style.css"">
                                </head>
                                <body>
                                    <div class=""container"">
                                        <div class=""box box-1""></div>
                                        <div class=""box box-2""></div>
                                        <div class=""box box-3""></div>
                                        <div class=""box box-4""></div>
                                        <nav>
                                            <h1>Employee Time Report</h1>
                                        </nav>
                                        <div class=""inner-container"">
                                            <div class=""table-container"">
                                                <table>
                                                    <thead>
                                                        <tr>
                                                            <th>Name</th>
                                                            <th>Total Time Worked (hours)</th>
                                                        </tr>
                                                    </thead>
                                                    <tbody>";

            // Displaying Each Row
            foreach (var employee in employees)
            {
                string rowClass = employee.Value < 100 ? "id=\"low-time\"" : "";
                html += $@"
            <tr {rowClass}>
                <td>{employee.Key}</td>
                <td>{employee.Value.ToString("F2", CultureInfo.InvariantCulture)} hrs </td>
            </tr>";
            }

            // Closing Tags
            html += @"</tbody>
                    </table>
                </div>
                <div class=""img-container"">
                    <div class=""img-text""><h3>Pei Chart For Employee Work Time</h3></div>
                    <img src=""" + imgUrl + @""" alt=""Image"" width=""100%"" height=""100%"" />
                </div>
            </div>
        </div>
    </body>
</html>";
            File.WriteAllText(filePath, html);  // Writing to a HTML file
            Console.WriteLine("HTML Content Generated");
        }

        // Creating and Saving peiChart
        static void CreateAndSavePieChart(Dictionary<string, int> employeeWithWorkingTime, string filePath)
        {
            var plt = new Plot(600, 400);

            int[] values = employeeWithWorkingTime.Values.ToArray();
            double[] doubleValues = Array.ConvertAll(values, i => (double)i);
            string[] labels = employeeWithWorkingTime.Keys.ToArray();
            var pie = plt.AddPie(doubleValues);
            pie.SliceLabels = labels;
            pie.ShowPercentages = true;
            plt.Legend();
            plt.Style(figureBackground: Color.Black,
                  dataBackground: Color.Black);

            plt.SaveFig(filePath);
            Console.WriteLine("PeiChart Created");
        }
    }

    // Employee class for storing data
    class Employee
    {
        public string? EmployeeName { get; set; }
        public string? StarTimeUtc { get; set; }
        public string? EndTimeUtc { get; set; }
    }
}
