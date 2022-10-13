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

namespace MCT.Function
{
    public static class Toevoegen
    {
        [FunctionName("AddRegistration")]
        public static async Task<IActionResult> AddRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "v1/registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Registration newRegistration = JsonConvert.DeserializeObject<Registration>(requestBody);

                newRegistration.RegistrationId = Guid.NewGuid();

                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO Registration (RegistrationID, Lastname, Firstname, Email, Zipcode, Age, IsFirstTimer) VALUES(@RegistrationID, @LastName, @FistName, @EMail, @Zipcode, @Age, @IsFirstTimer)"))
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Parameters.AddWithValue("@RegistrationId", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@LastName", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@FirstName", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@Email", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@Zipcode", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@Age", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@IsFirstTimer", newRegistration.RegistrationId);
                        await sqlCommand.ExecuteNonQueryAsync();


                    }
                }
                return new CreatedResult($"/registrations/{newRegistration.RegistrationId}", newRegistration);
            }
            catch (System.Exception ex)
            {
                return new StatusCodeResult(500);
            }

        }

        [FunctionName("GetRegistration")]
        public static async Task<IActionResult> GetRegistration(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "registrations")] HttpRequest req,
            ILogger log)
        {
            try
            {
                string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");
                string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                Registration newRegistration = JsonConvert.DeserializeObject<Registration>(requestBody);
                newRegistration.RegistrationId = Guid.NewGuid();

                using (SqlConnection sqlConnection = new SqlConnection(ConnectionString))
                {
                    await sqlConnection.OpenAsync();
                    using (SqlCommand sqlCommand = new SqlCommand("INSERT INTO Registrations (RegistrationId, LastName, FirstName, Email, ZipCode, Age, IsFirstTime) VALUES (@RegistrationId,@LastName,@FirstName,@Email,@ZipCode,@Age,@IsFirstTime)"))
                    {
                        sqlCommand.Connection = sqlConnection;
                        sqlCommand.Parameters.AddWithValue("@RegistrationId", newRegistration.RegistrationId);
                        sqlCommand.Parameters.AddWithValue("@LastName", newRegistration.LastName);
                        sqlCommand.Parameters.AddWithValue("@FirstName", newRegistration.FirstName);
                        sqlCommand.Parameters.AddWithValue("@Email", newRegistration.EMail);
                        sqlCommand.Parameters.AddWithValue("@ZipCode", newRegistration.Zipcode);
                        sqlCommand.Parameters.AddWithValue("@Age", newRegistration.Age);
                        sqlCommand.Parameters.AddWithValue("@IsFirstTime", newRegistration.IsFirstTimer);
                        await sqlCommand.ExecuteNonQueryAsync();
                    }
                }

                return new CreatedResult($"/registrations/{newRegistration.RegistrationId}", newRegistration);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult(500);
            }
        }
    }
}
