using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace vireoXconfigSquare.Classes
{
    public interface ICellsEnumerator :
        IEnumerable<(string columnName, string cellContent)>
    {
    }

    public class CellsEnumerator :
        List<(string columnName, string cellContent)>,
        ICellsEnumerator
    {
    }

    //TODO controllare fattibilità di https://joshclose.github.io/CsvHelper/
    public static class CsvSerializer
    {
        private static Encoding DefaultEncoding => Encoding.UTF8;
        const string CsvSemicolonEscape = "%sc%";

        /// <summary>
        /// Asynchronously deserializes the data from a csv. Header line will be skipped from enumerator.
        /// Lines are a list of twins: each cell has the name of the column and the data content. Usage:
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        /// <example>
        /// async System.Threading.Tasks.Task Parse()
        /// {
        ///     var parsed = CsvSerializer.DeserializeAsync("filename.csv");
        ///     int i = 0;
        ///     await foreach (var row in parsed)
        ///     {
        ///         foreach ((string columnName, string cellContent) in row)
        ///             Console.WriteLine($"Line {i} has the value {cellContent} at the column {columnName}");
        ///         i++;
        ///     }
        /// }
        /// </example>
        public async static IAsyncEnumerable<ICellsEnumerator> DeserializeAsync(string inputFile, char separator = ';')
        {
            //open file in read mode
            using var sourceStream =
                new FileStream(
                    inputFile,
                    FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: short.MaxValue,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

            using var reader = new StreamReader(sourceStream, DefaultEncoding, detectEncodingFromByteOrderMarks: true);

            //read and split header line, break if not present
            var headerLine = await reader.ReadLineAsync();//.ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(headerLine))
                yield break;

            var header = headerLine.Split(separator);

            while (reader.EndOfStream == false)
            {
                //read data line
                var line = await reader.ReadLineAsync();//.ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var row = line.Split(separator);
                if (row.Length < header.Length)
                    continue;

                //convert data line in a list of twins
                int i = 0;
                //List<(string columnName, string cellContent)> newLine = new List<(string columnName, string cellContent)>();
                CellsEnumerator newLine = new CellsEnumerator();
                foreach (var colName in header)
                {
                    var cell = (colName, row[i].Replace(CsvSemicolonEscape, ";"));
                    newLine.Add(cell);
                    i++;
                }

                yield return newLine;
            }
        }



        public static async IAsyncEnumerable<IAsyncEnumerable<ICellsEnumerator>> Split(this IAsyncEnumerable<ICellsEnumerator> enumerable, int chunkSize)
        {
            //System.Linq.Async
            while (true)
            {
                yield return enumerable.Take(chunkSize);
                enumerable.Skip(chunkSize);
                if (await enumerable.AnyAsync() == false)
                    break;
            }
        }

        public async static IAsyncEnumerable<Dictionary<string, string>> DeserializeAsyncByRow(string inputFile, char separator = ';')
        {
            //open file in read mode
            using var sourceStream =
                new FileStream(
                    inputFile,
                    FileMode.Open, FileAccess.Read, FileShare.Read,
                    bufferSize: short.MaxValue,
                    FileOptions.Asynchronous | FileOptions.SequentialScan);

            using var reader = new StreamReader(sourceStream, DefaultEncoding, detectEncodingFromByteOrderMarks: true);

            //read and split header line, break if not present
            var headerLine = await reader.ReadLineAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(headerLine)) yield break;

            var header = headerLine.Split(separator);

            while (!reader.EndOfStream)
            {
                //read data line
                var line = await reader.ReadLineAsync().ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var row = line.Split(separator);
                if (row.Length < header.Length)
                    continue;

                //convert data line in a list of twins
                int i = 0;
                var newLine = new Dictionary<string, string>();
                foreach (var colName in header)
                {
                    newLine.Add(colName, row[i].Replace(CsvSemicolonEscape, ";"));
                    i++;
                }

                yield return newLine;
            }
        }

        /// <summary>
        /// Asynchronously serializes the data to a csv file. Header line will inferred from property names of the class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvFile"></param>
        /// <param name="toSerialize"></param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public async static Task SerializeAsync<T>(FileInfo csvFile, IEnumerable<T> toSerialize, char separator = ';')
            where T : class
        {
            //open file, create mode
            using var sourceStream =
                new FileStream(
                    csvFile.FullName,
                    FileMode.Create,
                    FileAccess.Write, FileShare.None,
                    bufferSize: short.MaxValue,
                    useAsync: true);

            //get columns name, from property names of the type in input
            var firstRow = toSerialize.FirstOrDefault();
            if (firstRow == null) return;
            IEnumerable<PropertyInfo> propertyInfo = firstRow.GetType()
                .GetProperties(
                        BindingFlags.Public |
                        BindingFlags.Instance);

            //build header line and write it to file
            string header = "";
            foreach (var p in propertyInfo)
                header += p.Name + separator;
            header = header.TrimEnd(separator) + Environment.NewLine;
            byte[] encodedHead = DefaultEncoding.GetBytes(header);
            await sourceStream.WriteAsync(encodedHead.AsMemory(0, encodedHead.Length));

            //write actual data to file
            foreach (var property in toSerialize)
            {
                string line = "";

                foreach (var p in propertyInfo)
                {
                    if (p == null)
                        break;

                    var elem = p.GetValue(property);
                    if (elem == null)
                        break;

                    line += elem.ToString()?.Replace(";", CsvSemicolonEscape) + separator;
                }
                line = line.Remove(line.Length - 1) + Environment.NewLine;

                byte[] encodedText = DefaultEncoding.GetBytes(line);
                await sourceStream.WriteAsync(encodedText.AsMemory(0, encodedText.Length));
            }
        }

        public async static Task SerializeAsync(string csvName, IEnumerable<string> header, IEnumerable<IEnumerable<string>> lines, char separator = ';')
        {
            //open file, create mode
            using var sourceStream =
                new FileStream(
                    csvName,
                    FileMode.Create,
                    FileAccess.Write, FileShare.None,
                    bufferSize: short.MaxValue,
                    useAsync: true);

            //write header line
            string headerLine = string.Join(separator, header) + Environment.NewLine;
            byte[] encodedHead = DefaultEncoding.GetBytes(headerLine);
            await sourceStream.WriteAsync(encodedHead.AsMemory(0, encodedHead.Length));

            //write actual data to file
            foreach (var line in lines)
            {
                string dataLine = "";
                foreach (string field in line)
                    dataLine += field.Replace(";", CsvSemicolonEscape) + separator;
                dataLine = dataLine.Remove(dataLine.Length - 1) + Environment.NewLine;

                byte[] encodedText = DefaultEncoding.GetBytes(dataLine);
                await sourceStream.WriteAsync(encodedText.AsMemory(0, encodedText.Length));
            }
        }
    }
}
