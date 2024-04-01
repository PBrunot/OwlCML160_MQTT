using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwlCM160_MQTT
{
    public class RealtimeMeasurement
    {
        /// <summary>
        /// Current measurement in Ampere
        /// </summary>
        public double Current_A { get; set; }
        /// <summary>
        /// Power calculation in kilowatts
        /// </summary>
        public double Power_kW { get; set; }

        public override string ToString()
        {
            return $"Current : {Current_A.ToString("0.000")} A, Power : {Power_kW.ToString("0.000")} kW";
        }
    }
}
