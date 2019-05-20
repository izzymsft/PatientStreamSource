using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.Azure.EventHubs;
using System.Text;
using PatientStreamSource.Models;

namespace PatientStreamSource.Utilities
{
    public class StreamGenerator
    {
        private readonly int NumberOfPatients;

        private readonly string StorageAccountConnection;

        private readonly string FileShareName;

        private readonly string EventHubConnectionString;

        private string EventHubName;

        private EventHubClient eventHubClient;

        public StreamGenerator(int patientCount, string storageKey, string fileShare, string eventHubConnection, string eventHubTopic)
        {
            this.NumberOfPatients = patientCount;
            this.StorageAccountConnection = storageKey;
            this.FileShareName = fileShare;
            this.EventHubConnectionString = eventHubConnection;
            this.EventHubName = eventHubTopic;

            var connectionStringBuilder = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubName
            };

            this.eventHubClient = EventHubClient.CreateFromConnectionString(connectionStringBuilder.ToString());
        }

        public void generatePatientValueRanges()
        {

            // This method will create records for number of patients in this.NumberOfPatients
            for (int i=1; i <= this.NumberOfPatients; i++) {

                this.generateSinglePatientRecordRange(i).Wait();
            }
        }

        // Generates totals, temperature, pulse and blood pressure data at the same time
        // However, there is a simulation of delay in when the records arrive at event hub and blob storage
        // Writes to the totals blob after delay
        // Writes to the temperature blob after delay
        // Writes to the pulse and pressure event hub after delay
        private async Task generateSinglePatientRecordRange(int patientIdentifier) {

            var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd'T'HHmmss.fffK");
            var simpleTimestamp = DateTime.Now.ToString("yyyy-MM-ddHHmmssfff");

            Random random = new Random();

            var patientId = HashUtils.SHA1HashStringForUTF8String(patientIdentifier + "");
            var total = random.Next(8, 16);

            double minTemperature = random.Next(97, 99);
            double maxTemperature = random.Next(99, 105);

            double minPulse = random.Next(60, 70);
            double maxPulse = random.Next(70, 125);
            double minDiastolic = random.Next(60, 65);
            double maxDiastolic = random.Next(65, 90);
            double minSystolic = random.Next(90, 100);
            double maxSystolic = random.Next(100, 200);

            // Intentional Delays between event occurrence and capture time at storage layer
            int firstPause = random.Next(1, 2);
            int secondPause = random.Next(3, 5);
            int thirdPause = random.Next(8, 13);

            var patientTotalsRecord = new PatientTotals(patientId, total);
            var patientTemps = new PatientTemperatures(patientId, minTemperature, maxTemperature, timestamp);
            var patientPulseAndBP = new PatientPulseAndPressure(patientId, minPulse, maxPulse, minDiastolic, maxDiastolic, minSystolic, maxSystolic, timestamp);

            Console.WriteLine("\n");
            Console.WriteLine("========================================================================");
            Console.WriteLine("Patient Sequence and Id: " + patientIdentifier + "," + patientId);
            Console.WriteLine(patientTotalsRecord.ToString());
            Console.WriteLine(PatientTemperatures.getCSVHeader());
            Console.WriteLine(patientTemps.toCSVRow());
            Console.WriteLine(patientPulseAndBP.ToString());

            string PatientShare = this.FileShareName;

            var totalsFilePathPrefix = "patients/totals/" + currentDate;
            var temperaturesFilePathPrefix = "patients/temperatures/" + currentDate;

            CloudStorageAccount storageAccount = CreateStorageAccountFromConnectionString(this.StorageAccountConnection);

            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            CloudFileShare share = fileClient.GetShareReference(PatientShare);

            try
            {
                await share.CreateIfNotExistsAsync();
            }
            catch (StorageException)
            {
                Console.WriteLine("Please make sure your storage account has storage file endpoint enabled and specified correctly in the app.config - then restart the sample.");
            
                throw; 
            }

            // Get a reference to the root directory of the share.        
            CloudFileDirectory root = share.GetRootDirectoryReference();

            Console.WriteLine("Creating Totals Directory");
            CloudFileDirectory totalsDirectory = root.GetDirectoryReference(totalsFilePathPrefix);
            await CreateRecursiveIfNotExists(totalsDirectory);

            Console.WriteLine("Creating Temperature Directory");
            CloudFileDirectory temperatureDirectory = root.GetDirectoryReference(temperaturesFilePathPrefix);
            await CreateRecursiveIfNotExists(temperatureDirectory);

            string totalBlobFilePath = "totals-" + patientId + "-" + simpleTimestamp + ".json";
            string temperatureBlobFilePath = "temperature-" + patientId +  "-" + simpleTimestamp + ".csv";

            Thread.Sleep(new TimeSpan(0, 0, firstPause));
            Console.WriteLine("Uploading totals data to blob storage after pause: " + firstPause);
            CloudFile totalsFile = totalsDirectory.GetFileReference(totalBlobFilePath);
            await totalsFile.UploadTextAsync(patientTotalsRecord.ToString());

            Thread.Sleep(new TimeSpan(0, 0, secondPause));
            Console.WriteLine("Uploading temperature data to blob storage after pause: " + secondPause);
            CloudFile tempsFile = temperatureDirectory.GetFileReference(temperatureBlobFilePath);
            string tempData = PatientTemperatures.getCSVHeader() + "\n" + patientTemps.toCSVRow();
            await tempsFile.UploadTextAsync(tempData);

            // Convert C# object to JSON string
            var msg = patientPulseAndBP.ToString();
                
            // Convert String to bytes
            var rawMessage = Encoding.UTF8.GetBytes(msg);
                
            // Package message as Event Data
            EventData eventData = new EventData(rawMessage);

            Thread.Sleep(new TimeSpan(0, 0, thirdPause));
            Console.WriteLine("Uploading pulse and pressure data to event hub after pause: " + thirdPause);
            // Send the pulse and blood pressure data to Event Hub
            await eventHubClient.SendAsync(eventData);
        }

        public void Close()
        {
             this.eventHubClient.CloseAsync().Wait();
        }

        private async Task CreateRecursiveIfNotExists(CloudFileDirectory directory)
        {
            bool directoryExists = await directory.ExistsAsync();
            if (!directoryExists)
            {
                await CreateRecursiveIfNotExists(directory.Parent);
                await directory.CreateAsync();
            }
        }

        private static CloudStorageAccount CreateStorageAccountFromConnectionString(string storageConnectionString)
        {
            CloudStorageAccount storageAccount;
            try
            {
                storageAccount = CloudStorageAccount.Parse(storageConnectionString);
            }
            catch (FormatException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Invalid storage account information provided. Please confirm the AccountName and AccountKey are valid in the app.config file - then restart the sample.");
                throw;
            }

            return storageAccount;
        }
    }
}