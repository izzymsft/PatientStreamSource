using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatientStreamSource.Models
{
    public class PatientPulseAndPressure
    {
        public string PatientId { get; set; }
        public double MinPulse { get; set; }
        public double MaxPulse { get; set; }
        public double MinDiastolic { get; set; }
        public double MaxDiastolic { get; set; }
        public double MinSystolic { get; set; }
        public double MaxSystolic { get; set; }

        public string VitalTimestamp { get; set; }

        public PatientPulseAndPressure()
        {

        }

        public PatientPulseAndPressure(string id, double minP, double maxP, double minD, double maxD, double minS, double maxS, string captureTime)
        {
            this.PatientId = id;
            this.MinPulse = minP;
            this.MaxPulse = maxP;
            this.MinDiastolic = minD;
            this.MaxDiastolic = maxD;
            this.MinSystolic = minS;
            this.MaxSystolic = maxS;
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
    }
}