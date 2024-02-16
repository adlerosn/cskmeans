using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

public class SWITRS
{
    public static (string[], string?[], double[][]) LoadDataSet()
    {
        var qry = File.ReadAllText("query.sql");
        List<string> identities = [];
        List<string?> labels = [];
        List<double>[] values = [];
        using (var connection = new SqliteConnection("Data Source=switrs.sqlite"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = qry;
            using (var reader = command.ExecuteReader())
            {
                values = new List<double>[reader.FieldCount - 2];
                foreach (var cellId in Enumerable.Range(0, reader.FieldCount - 2))
                    values[cellId] = new List<double>();
                while (reader.Read())
                {
                    identities.Add(reader.GetString(0));
                    labels.Add(reader.IsDBNull(1) ? null : reader.GetString(1));
                    foreach (var cellId in Enumerable.Range(0, reader.FieldCount - 2))
                        values[cellId].Add(reader.GetDouble(cellId + 2));
                }
            }
        }
        return (identities.ToArray(), labels.ToArray(), values.Select(x => x.ToArray()).ToArray());
    }
}