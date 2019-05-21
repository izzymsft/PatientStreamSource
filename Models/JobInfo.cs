using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.EventHubs;
using System.Text;
using PatientStreamSource.Models;
using PatientStreamSource.Utilities;

namespace PatientStreamSource.Models
{
    public class JobInfo
    {
        public string CaptureTime { get; set; }
        public string id { get; set; }
        public int total { get; set; }
        public double min_temperature { get; set; }
        public double max_temperature { get; set; }

        public double min_pulse { get; set; }
        public double max_pulse { get; set; }

        public double min_diastolic { get; set; }
        public double max_diastolic { get; set; }
        public double min_systolic { get; set; }
        public double max_systolic { get; set; }

        public JobInfo()
        {
            Random random = new Random();

            int patientId = 99;

            this.CaptureTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

            this.id = HashUtils.SHA1HashStringForUTF8String(patientId + "");
            this.total = random.Next(8, 16);

            this.min_temperature = random.Next(97, 99);
            this.max_temperature = random.Next(99, 105);

            this.min_pulse = random.Next(60, 70);
            this.max_pulse = random.Next(70, 125);
            this.min_diastolic = random.Next(60, 65);
            this.max_diastolic = random.Next(65, 90);
            this.min_systolic = random.Next(90, 100);
            this.max_systolic = random.Next(100, 200);
        }

        /**
        * Returns a JSON representation of the object 
        */
        public override string ToString()
        {
            string result = JsonConvert.SerializeObject(this);

            return result;
        }
    }
}