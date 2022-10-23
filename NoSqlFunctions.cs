using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Data.Tables;
using Azure;
using System.Collections.Generic;
using CsvHelper;
using System.Globalization;
using Azure.Storage.Blobs;
using Azure.Identity;

namespace MCT.Function
{
    public class NoSqlFunctions
    {
        [FunctionName("AddRegistrationNoSql")]
        public async Task<IActionResult> AddRegistrationNoSql(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Post", Route = "v2/registrations")] HttpRequest req,
            ILogger log)
        {
            string TableUrl = Environment.GetEnvironmentVariable("TableUrl");
            string AccountName = Environment.GetEnvironmentVariable("AccountName");
            string AccountKey = Environment.GetEnvironmentVariable("AccountKey");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Registration newRegistration = JsonConvert.DeserializeObject<Registration>(requestBody);

            newRegistration.RegistrationId = Guid.NewGuid();

            string partionKey = newRegistration.Zipcode;
            string rowKey = newRegistration.RegistrationId.ToString();

            var tableClient = new TableClient(new Uri(TableUrl), "Registrations", new TableSharedKeyCredential(AccountName, AccountKey));


            var registrationEntity = new TableEntity(partionKey, rowKey)
            {
                {"LastName", newRegistration.LastName},
                {"FirstName", newRegistration.FirstName},
                {"Email", newRegistration.EMail},
                {"Zipcode", newRegistration.Zipcode},
                {"Age", newRegistration.Age.ToString()},
                {"IsFirstTimer", newRegistration.IsFirstTimer.ToString()}
            };

            await tableClient.AddEntityAsync(registrationEntity);
            return new OkObjectResult(registrationEntity);
        }

        [FunctionName("GetRegistrationNoSql")]
        public async Task<IActionResult> GetRegistrationNoSql(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v2/registrations/{partionKey}")] HttpRequest req,
            string partionKey,
            ILogger log)
        {
            try
            {
                string TableUrl = Environment.GetEnvironmentVariable("TableUrl");
                string AccountName = Environment.GetEnvironmentVariable("AccountName");
                string AccountKey = Environment.GetEnvironmentVariable("AccountKey");


                var tableClient = new TableClient(new Uri(TableUrl), "registrations", new DefaultAzureCredential());

                Pageable<TableEntity> registrations = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partionKey}'");
                var list = new List<Registration>();
                foreach (var registration in registrations)
                {
                    list.Add(new Registration
                    {
                        RegistrationId = Guid.Parse(registration.RowKey),
                        LastName = registration["LastName"].ToString(),
                        FirstName = registration["FirstName"].ToString(),
                        EMail = registration["Email"].ToString(),
                        Zipcode = registration["Zipcode"].ToString(),
                        Age = int.Parse(registration["Age"].ToString()),
                        IsFirstTimer = bool.Parse(registration["IsFirstTimer"].ToString())
                    });
                }

                return new OkObjectResult(list);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult(500);
            }

        }




        [FunctionName("Export")]
        public async Task<IActionResult> Export(
            [HttpTrigger(AuthorizationLevel.Anonymous, "Get", Route = "v2/registrations/{partionKey}")] HttpRequest req,
            string partionKey,
            ILogger log)
        {
            try
            {
                string TableUrl = Environment.GetEnvironmentVariable("TableUrl");



                var tableClient = new TableClient(new Uri(TableUrl), "registrations", new DefaultAzureCredential());

                Pageable<TableEntity> registrations = tableClient.Query<TableEntity>(filter: $"PartitionKey eq '{partionKey}'");
                var list = new List<Registration>();
                foreach (var registration in registrations)
                {
                    list.Add(new Registration
                    {
                        RegistrationId = Guid.Parse(registration.RowKey),
                        LastName = registration["LastName"].ToString(),
                        FirstName = registration["FirstName"].ToString(),
                        EMail = registration["Email"].ToString(),
                        Zipcode = registration["Zipcode"].ToString(),
                        Age = int.Parse(registration["Age"].ToString()),
                        IsFirstTimer = bool.Parse(registration["IsFirstTimer"].ToString())
                    });
                }

                string filename = $"registrations-{partionKey}.csv";
                string filePath = $"{Path.GetTempPath()}{filename}";


                using (var writer = new StreamWriter(filePath))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(list);
                    }
                }

                string containerUrl = Environment.GetEnvironmentVariable("ContainerUrl");

                BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(containerUrl), new DefaultAzureCredential());
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient("csv");
                BlobClient blobClient = containerClient.GetBlobClient(filename);
                await blobClient.UploadAsync(filePath);

                File.Delete(filePath);

                return new OkObjectResult(list);
            }
            catch (System.Exception ex)
            {
                log.LogError(ex.Message);
                return new StatusCodeResult(500);
            }

        }
    }
}
