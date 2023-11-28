using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MyoVisualizer.Data
{
    public class DataCommunicator
    {

        public DataCommunicator()
        {
            
        }

        public void SaveCsv<T>(string @path, Dictionary<string, List<T>> data)
        {

            var dirPath = Path.GetDirectoryName(Path.GetFullPath(path));
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            
            // File.WriteAllLines(path, data.Select(value => $"{value},"), Encoding.UTF8);

            var label = new StringBuilder();
            label.Append("# ");
            
            var dataLineArray = new List<string>();
            
            data.Keys.ToList().ForEach(key => label.Append($"{key},")); // label作成
            dataLineArray.Add(label.ToString());
            var dataList = data.Values.ToList();
            var maxDataCount = dataList.Select(i => i.Count).Max();

            for (int i = 0; i < maxDataCount; i++)
            {
                var temp = new StringBuilder();
                for (int j = 0; j < dataList.Count; j++)
                {
                    temp.Append($"{(i + 1 <= dataList[j].Count ? $"{dataList[j][i]}" : "0")},");
                }
                
                dataLineArray.Add($"{temp}");
            }

            File.WriteAllLines(path, dataLineArray.ToArray(), Encoding.UTF8);


        }

        public void DataTransfer()
        {
            
        }


    }
}