using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PatientStreamSource.Models
{
    public class PatientTotals
    {
        public string PatientId { get; set; }
        public int Total { get; set; }
        public string VitalTimestamp { get; set; }

        public PatientTotals()
        {

        }

        public PatientTotals(string id, int totals, string captureTime)
        {
            this.PatientId = id;
            this.Total = totals;
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