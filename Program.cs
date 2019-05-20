using System;
using System.IO;
using PatientStreamSource.Utilities;

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Extensions.Configuration;
using System.Security.Cryptography;
using System.Text;

using Newtonsoft.Json;



using System.Threading;



namespace PatientStreamSource
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
 
            IConfiguration config = builder.Build();

            // This is the number of unique patients to generate
            var numberOfPatients = 1;

            Console.WriteLine("Generating Patient Stream Data to Event Hub and Blob Storage ...");
            
            string eventHubConnectionString = config.GetConnectionString("EventHubConnection");
            string eventHubTopicTotals = config.GetConnectionString("EventHubNameTotals");
            string eventHubTopicTemperature = config.GetConnectionString("EventHubNameTemperature");
            string eventHubTopicPulseAndPressure = config.GetConnectionString("EventHubNamePulseAndPressure");

            var generator = new StreamGenerator(numberOfPatients, eventHubConnectionString, eventHubTopicTotals, eventHubTopicTemperature, eventHubTopicPulseAndPressure);

            generator.generatePatientValueRanges();

            generator.Close();

            Console.WriteLine("\n");
            Console.Write("Data Generation Completed ....\n\n\n");
        }
    }
}
