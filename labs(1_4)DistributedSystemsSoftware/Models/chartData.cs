using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace labs_1_4_DistributedSystemsSoftware.Models
{
    //DataContract for Serializing Data - required to serve in JSON format
    [DataContract]
    public class chartData
    {
        public chartData(double y, string x)
        {
            this.Y = y;
            this.X = x;
        }

        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "y")]
        public double Y = 0.0;
        //Explicitly setting the name to be used while serializing to JSON.
        [DataMember(Name = "label")]
        public string X = string.Empty;
    }
}