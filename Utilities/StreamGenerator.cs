using System;
using System.Threading;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Blob;
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

        private string EventHubNameTotals;
        private string EventHubNameTemperature;
        private string EventHubNamePulseAndPressure;

        private EventHubClient eventHubClientTotals;
        private EventHubClient eventHubClientTemperature;
        private EventHubClient eventHubClientPulsePressure;

        public StreamGenerator(int patientCount, string eventHubConnection, string totalsTopic, string tempTopic, string pulsePressureTopic)
        {
            this.NumberOfPatients = patientCount;
            this.EventHubConnectionString = eventHubConnection;
            this.EventHubNameTotals = totalsTopic;
            this.EventHubNameTemperature = tempTopic;
            this.EventHubNamePulseAndPressure = pulsePressureTopic;

            var connectionStringBuilderTotals = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubNameTotals
            };

            var connectionStringBuilderTemperature = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubNameTemperature
            };

            var connectionStringBuilderPulseAndPressure = new EventHubsConnectionStringBuilder(EventHubConnectionString)
            {
                EntityPath = EventHubNameTotals
            };

            this.eventHubClientTotals = EventHubClient.CreateFromConnectionString(connectionStringBuilderTotals.ToString());
            this.eventHubClientTemperature = EventHubClient.CreateFromConnectionString(connectionStringBuilderTemperature.ToString());
            this.eventHubClientPulsePressure = EventHubClient.CreateFromConnectionString(connectionStringBuilderPulseAndPressure.ToString());
        }

        public void generatePatientValueRanges()
        {

            // This method will create records for number of patients in this.NumberOfPatients
            for (int i=1; i <= this.NumberOfPatients; i++) {

                this.generateSinglePatientRecordRange(i).Wait();
            }
        }

        // Generates totals, temperature, pulse and blood pressure data at the same time
        // However, there is a simulation of delay in when the records arrive at event hub
        // Writes to the totals store after delay
        // Writes to the temperature store after delay
        // Writes to the pulse and pressure store after delay
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

            string temperatureData = PatientTemperatures.getCSVHeader() + "\n" + patientTemps.toCSVRow();

            await this.sendMessage("Totals", this.eventHubClientTotals, patientTotalsRecord.ToString(), firstPause);
            await this.sendMessage("Temperature", this.eventHubClientTemperature, temperatureData.ToString(), secondPause);
            await this.sendMessage("Pulse and Blood Pressure", this.eventHubClientPulsePressure, patientPulseAndBP.ToString(), thirdPause);
        }

        private async Task sendMessage(string category, EventHubClient client, string message, int pauseDurationSeconds) {
                
            Console.WriteLine("Preparing Transfer for Category: " + category + " Pause Duration: " + pauseDurationSeconds);

            // Convert String to bytes
            var rawMessage = Encoding.UTF8.GetBytes(message);
                
            // Package message as Event Data
            EventData eventData = new EventData(rawMessage);

            Thread.Sleep(new TimeSpan(0, 0, pauseDurationSeconds));
            Console.WriteLine("Sending Data for " + category + " after pause: " + pauseDurationSeconds);
            
            // Send the pulse and blood pressure data to Event Hub
            await client.SendAsync(eventData);
        }

        public void Close()
        {
             this.eventHubClientTotals.CloseAsync().Wait();
             this.eventHubClientTemperature.CloseAsync().Wait();
             this.eventHubClientPulsePressure.CloseAsync().Wait();
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