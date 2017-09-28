using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace labs_1_4_DistributedSystemsSoftware.Models
{
    public class dataType
    {
        public dataType(double firstVal, double secondVal, double thirdVal)
        {
            this.firstVal = firstVal;
            this.secondVal = secondVal;
            this.thirdVal = thirdVal;
        }

        public dataType() { }

        public double firstVal { get; set; }
        public double secondVal { get; set; }
        public double thirdVal { get; set; }
    }
}