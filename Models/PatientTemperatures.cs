using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatientStreamSource.Models
{
    public class PatientTemperatures
    {
        public string PatientId { get; set; }

        public double MinTemperature { get; set; }

        public double MaxTemperature { get; set; }

        public string VitalTimestamp { get; set; }

        public PatientTemperatures()
        {

        }
        
        public PatientTemperatures(string id, double minTemp, double maxTemp, string captureTime)
        {
            this.PatientId = id;
            this.MinTemperature = minTemp;
            this.MaxTemperature = maxTemp;
            this.VitalTimestamp = captureTime;
        }

        /**
        * Returns a JSON representation of the object 
        */
        public override string ToString()
        {
            string result = JsonConvert.SerializeObject(this);

            return result;
        }

        public static string getCSVHeader()
        {
            return "PatientId,MinTemperature,MaxTemperature,VitalTimestamp";
        }

        public string toCSVRow()
        {
            return this.PatientId + "," + this.MinTemperature + "," + this.MaxTemperature + "," + this.VitalTimestamp;
        }
    }
}