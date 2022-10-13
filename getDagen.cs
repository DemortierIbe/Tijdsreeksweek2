using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;


namespace MCT.Functions
{
    public static class getDagen
    {
        [FunctionName("getDagen")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "days")] HttpRequest req,
            ILogger log)
        {

            string connectionString = Environment.GetEnvironmentVariable("ConnectionString");
            List<string> dagen = new List<string>();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                using (SqlCommand command = new SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "select Distinct DagVanDeWeek From Bezoekers";

                    SqlDataReader reader = await command.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        var dag = reader["DagVanDeWeek"].ToString();
                        dagen.Add(dag);
                    }
                }
            }

            return new OkObjectResult(dagen);
        }

        [FunctionName("getVisitors")]
        public static async Task<IActionResult> GetVisitors(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "visitors/{day}")] HttpRequest req, string day,
            ILogger log)
        {

            try
            {
                string connectiongString = Environment.GetEnvironmentVariable("ConnectionString");
                List<Visit> visits = new List<Visit>();

                using (SqlConnection connection = new SqlConnection(connectiongString))
                {
                    await connection.OpenAsync();

                    using (SqlCommand command = new SqlCommand())
                    {
                        command.Connection = connection;
                        command.CommandText = "SELECT TijdstipDag, AantalBezoekers FROM Bezoekers Where DagVanDeWeek = @dag";
                        command.Parameters.AddWithValue("@dag", day);

                        SqlDataReader reader = await command.ExecuteReaderAsync();
                        while (await reader.ReadAsync())
                        {
                            var visit = new Visit();
                            visit.TijdstipDag = Convert.ToInt32(reader["TijdstipDag"]);
                            visit.AantalBezoekers = Convert.ToInt32(reader["AantalBezoekers"]);
                            visit.DagVanDeWeek = day;
                            visits.Add(visit);
                        }
                    }
                }
                return new OkObjectResult(visits);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult(500);
            }
        }
    }
}