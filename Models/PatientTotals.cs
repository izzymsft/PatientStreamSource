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

        public PatientTotals()
        {

        }

        public PatientTotals(string id, int totals)
        {
            this.PatientId = id;
            this.Total = totals;
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