using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApp2
{
    class Program
    {
        static string DefaultFilePath()
        {
            string filePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\sum_of_history.json";
            return filePath;
        }

        //dict Value的處理
        static CalculationRecord Calculate(string userName, List<decimal> numberLists)
        {
            var sum = numberLists.Sum();
            Console.WriteLine($"Calculate result:{sum}");
            return new CalculationRecord()
            {
                date = DateTime.Now,
                user = userName,
                result = sum,
                description = $"Calculate result:{sum}",
                user_input = numberLists
            };
        }

        static void SaveToJson(string jsonFilePath, Dictionary<string,CalculationRecord> data)
        {
            var jsonObj = JObject.FromObject(data);
            File.WriteAllText(jsonFilePath, JsonConvert.SerializeObject(jsonObj, Formatting.Indented));
            Console.WriteLine("Saved records to " + jsonFilePath);
        }


        static void Main(string[] args)
        {

            var condition = true;
            var filePath = DefaultFilePath();
            var username = WindowsIdentity.GetCurrent().Name;

            //讀檔轉成json物件 -->再轉成dictionary
            var data = JObject.Parse(File.ReadAllText(filePath))
               .ToObject<Dictionary<string, CalculationRecord>>();


            while (condition)
            {
                Console.WriteLine("Please choose a function.\n " +
                    "'Q': Quit program.\n" +
                    "'SUM': Summarize the entered numbers.\n" +
                    "'DEL': Delete assigned data.\n" +
                    "'SEARCH': Search from history. (Result and calculation time) \n" +
                    "'UPDATE': Re-calculte assigned data");

                var inputString = Console.ReadLine();

                if (inputString.ToUpper() == "Q")
                    break;

                //加總sum，加總完立即存到json
                else if (inputString.ToUpper() == "SUM")
                {
                    Console.WriteLine("Please input numbers with seperated by space.");
                    var userInput = Console.ReadLine();

                    //輸入字串-->string [] --> LINQ -->List<decimal>
                    var numberLists = userInput.Split(' ')
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => decimal.Parse(x))
                        .ToList();

                    //Dictionary<string, CalculationRecord> data
                    data.Add(Guid.NewGuid().ToString().Substring(0, 5), Calculate(username, numberLists));

                   
                    SaveToJson(filePath, data);

                }

                //歷史查詢 
                else if (inputString.ToUpper() == "SEARCH")
                {
                    Console.WriteLine("enter the search text, empty for all data");
                    var searchText = Console.ReadLine();
                    var searchResult = data
                        .Where(x => x.Key.Contains(searchText) || JsonConvert.SerializeObject(x.Value).Contains(searchText))
                        .ToDictionary(x => x.Key, x => x.Value);
                    Console.WriteLine(JsonConvert.SerializeObject(searchResult, Formatting.Indented));
                }

                //刪除指定資料 
                else if (inputString.ToUpper() == "DEL")
                {
                    Console.WriteLine("Please enter the key");
                    var key = Console.ReadLine();
                    data.Remove(key);
                    
                    SaveToJson(filePath, data);

                }

                //指定某筆資料重算
                else if (inputString.ToUpper() == "UPDATE")
                {
                    //foreach (string dataKey in data.Keys)
                    //{
                    //    Console.WriteLine("{0}", dataKey);
                    //}
                    data.Select(x => x.Key).ToList().ForEach(Console.WriteLine);

                    Console.WriteLine("Please enter the key");
                    var key = Console.ReadLine();
                    var oldRecord = data[key];
                    data.Remove(key);

                    Console.WriteLine("Please input numbers with seperated by space.");
                    var userInput = Console.ReadLine();

                    var numberLists = userInput.Split(' ')
                        .Where(x => !string.IsNullOrEmpty(x))
                        .Select(x => decimal.Parse(x))
                        .ToList();

                    var newRecord = Calculate(username, numberLists);

                    var history = new List<CalculationHistory>();
                    history.Add(new CalculationHistory()
                    {
                        date = oldRecord.date,
                        user = oldRecord.user,
                        result = oldRecord.result,
                        description = oldRecord.description,
                        user_input = oldRecord.user_input
                    });
                    history.AddRange(newRecord.update_history);
                    newRecord.update_history = history;

                    data[key] = newRecord;
                    
                    SaveToJson(filePath, data);


                }

            }
        }
    }

    public class CalculationRecord
    {
        public DateTime date { get; set; }
        public string user { get; set; }
        public decimal result { get; set; }
        public string description { get; set; }

        public List<decimal> user_input { get; set; } = new List<decimal>();
        public List<CalculationHistory> update_history = new List<CalculationHistory>();
    }

    public class CalculationHistory
    {
        public DateTime date { get; set; }
        public string user { get; set; }
        public decimal result { get; set; }
        public string description { get; set; }
        public List<decimal> user_input { get; set; } = new List<decimal>();
    }
}
