using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;

public class SWITRS
{
    public static (string[], double[][]) LoadDataSet()
    {
        var qry = File.ReadAllText("query.sql");
        string[] identities = [];
        List<double>[] values = [];
        using (var connection = new SqliteConnection("Data Source=switrs.sqlite"))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = qry;
            using (var reader = command.ExecuteReader())
            {
                values = new List<double>[reader.FieldCount - 1];
                foreach (var cellId in Enumerable.Range(0, reader.FieldCount - 1))
                    values[cellId] = new List<double>();
                while (reader.Read())
                {
                    identities.Append(reader.GetString(0));
                    foreach (var cellId in Enumerable.Range(0, reader.FieldCount - 1))
                        values[cellId].Add(reader.GetDouble(cellId + 1));
                }
            }
        }
        return (identities, values.Select(x => x.ToArray()).ToArray());
    }
}