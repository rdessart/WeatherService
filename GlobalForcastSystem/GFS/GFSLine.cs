using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace GlobalForcastSystem
{
    public class GFSLine
    {
        public enum Units
        {
            NONE = -1,
            UNKNOWN = 0,
            Pa = 1,
            hPa,
            Meter,
            Feet,
            mps,
            knots,
            Kelvin,
            Celcius,
            DegTrue,
            Percent,
            KgPerKg,
            PerSeconds,
            PascalPerSeconds,
            Fraction,
            KgPerSquareMeter,
            KgPerSquareMeterPerSeconds,
        } 
        public int Id { get; set; }
        public string Num { get; set; }
        public DateTime Date { get; set; }
        public string Field { get; set; }
        public string FieldDescription { get; set; }
        public double Pressure { get; set; }
        public bool IsPressureAltitude { get; protected set; }
        //public TimeSpan ForcastTime { get; set; }
        public int ForcastTime { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double Value { get; set; }
        public Units Unit { get; set; }
        protected static Regex altMeterReg = new Regex(@"\b\d{3,4}\b \bm\b");
        public GFSLine(string input)
        {
            string[] datas = input.Split(':');
            Id = int.Parse(datas[0]);
            Num = datas[1];
            string sDate = datas[2].Substring(2);
            Date = new DateTime(int.Parse(sDate.Substring(0, 4)), int.Parse(sDate.Substring(4, 2)), int.Parse(sDate.Substring(6, 2)), int.Parse(sDate.Substring(8, 2)), 0, 0);
            Field = datas[3];
            Pressure = double.MinValue;
            IsPressureAltitude = false;
            if (datas[4].Contains("ground") == false && datas[4].Contains("mb"))
            {
                int pos = datas[4].IndexOf(" mb");
                Pressure = double.Parse(datas[4].Substring(0, pos), CultureInfo.InvariantCulture);
            }
            else if(altMeterReg.IsMatch(datas[4]))
            {
                IsPressureAltitude = true;
                string output = Regex.Match(datas[4], @"\b\d{2,4}\b").Value;
                Pressure = double.Parse(output, CultureInfo.InvariantCulture);
            }
            else if(datas[4].Contains("tropopause"))
            {
                IsPressureAltitude = false;
                Pressure = -1;
            }
            else if (datas[4] == "surface")
            {
                IsPressureAltitude = false;
                Pressure = -2;
            }
            if (datas[5] == "anl")
            {
                ForcastTime = 0;
            }
            else
            {
                ForcastTime = int.Parse(Regex.Match(datas[5], @"\d{0,2}\d").Value);
            }
            string[] field = datas[7].Split(',');
            Longitude = double.Parse(field[0].Substring(4), CultureInfo.InvariantCulture);
            Latitude = double.Parse(field[1].Substring(4), CultureInfo.InvariantCulture);
            Value = double.Parse(field[2].Substring(4), CultureInfo.InvariantCulture);
            Unit = Units.UNKNOWN;
            FieldDescription = "";
            ParseField();
        }

        protected void ParseField()
        {
            switch (Field)
            {
                case "PRES":
                    FieldDescription = "Pressure";
                    Unit = Units.Pa;
                    break;
                case "PRMSL":
                    FieldDescription = "Pressure reduced to MSL";
                    Unit = Units.Pa;
                    break;
                case "ICAHT":
                    FieldDescription = "ICAO ISA Ref Height";
                    Unit = Units.Meter;
                    break;
                case "TMP":
                    FieldDescription = "Temperature";
                    Unit = Units.Kelvin;
                    break;
                case "TMAX":
                    FieldDescription = "Temperature Maximum";
                    Unit = Units.Kelvin;
                    break;
                case "TMIN":
                    FieldDescription = "Temperature Minimum";
                    Unit = Units.Kelvin;
                    break;
                case "DPT":
                    FieldDescription = "DewPoint";
                    Unit = Units.Kelvin;
                    break;
                case "VIS":
                    FieldDescription = "Visibility";
                    Unit = Units.Meter;
                    break;
                case "WDIR":
                    FieldDescription = "Wind Direction";
                    Unit = Units.DegTrue;
                    break;
                case "WIND":
                    FieldDescription = "Wind Speed";
                    Unit = Units.mps;
                    break;
                case "UGRD":
                    FieldDescription = "Wind U Componenent";
                    Unit = Units.mps;
                    break;
                case "VGRD":
                    FieldDescription = "Wind V Componenent";
                    Unit = Units.mps;
                    break;
                case "GUST":
                    FieldDescription = "Surface Wind Gust";
                    Unit = Units.mps;
                    break;
                case "RH":
                    FieldDescription = "Relative Humidity";
                    Unit = Units.Percent;
                    break;
                case "ICMR":
                    FieldDescription = "Ice Mixing Ratio";
                    Unit = Units.KgPerKg;
                    break;
                case "TPFI":
                    FieldDescription = "Turbulence Potential Forcast Index";
                    Unit = Units.NONE;
                    break;
                case "TIPD":
                    FieldDescription = "Total Icing Potential";
                    Unit = Units.NONE;
                    break;
                case "POP":
                    FieldDescription = "Probability of precipitation";
                    Unit = Units.Percent;
                    break;
                case "CPOFP":
                    FieldDescription = "Percent of frozen precipitation";
                    Unit = Units.Percent;
                    break;
                case "CPOZP":
                    FieldDescription = "Probability of freezing precipitation";
                    Unit = Units.Percent;
                    break;
                case "HGT":
                    FieldDescription = "Geopotential Height";
                    Unit = Units.Meter;
                    break;
                case "O3MR":
                    FieldDescription = "Ozone mixing ratio";
                    Unit = Units.KgPerKg;
                    break;
                case "CLWMR":
                    FieldDescription = "Cloud Mixing Ratio";
                    Unit = Units.KgPerKg;
                    break;
                case "RWMR":
                    FieldDescription = "Rain water mixing ratio";
                    Unit = Units.KgPerKg;
                    break;
                case "SNMR":
                    FieldDescription = "Snow mixing ratio";
                    Unit = Units.KgPerKg;
                    break;
                case "ABSV":
                    FieldDescription = "Absolute vorticity";
                    Unit = Units.PerSeconds;
                    break;
                case "TCDC":
                    FieldDescription = "Total cloud cover";
                    Unit = Units.Percent;
                    break;
                case "VVEL":
                    FieldDescription = "Vertical Velocity Pressure";
                    Unit = Units.PascalPerSeconds;
                    break;
                case "DZDT":
                    FieldDescription = "Vertical Velocity Geometric";
                    Unit = Units.mps;
                    break;
                case "TSOIL":
                    FieldDescription = "Soil temperature";
                    Unit = Units.mps;
                    break;
                case "SOILW":
                    FieldDescription = "Volumetric soil moisture content";
                    Unit = Units.mps;
                    break;
                case "WEASD":
                    FieldDescription = "Water equiv. of accum. snow depth";
                    Unit = Units.KgPerSquareMeter;
                    break;
                case "SNOD":
                    FieldDescription = "Snow depth";
                    Unit = Units.Meter;
                    break;
                case "CSNOW":
                    FieldDescription = "Snow depth";
                    Unit = Units.mps;
                    break;
                case "SPFH":
                    FieldDescription = "Specific humidity";
                    Unit = Units.KgPerKg;
                    break;
                case "ICEG":
                    FieldDescription = "Ice growth rate";
                    Unit = Units.mps;
                    break;
                case "CPRAT":
                    FieldDescription = "Convective Precipitation rate";
                    Unit = Units.KgPerSquareMeterPerSeconds;
                    break;
                case "APCP":
                    FieldDescription = "Total precipitation";
                    Unit = Units.KgPerSquareMeter;
                    break;
                case "ACPCP":
                    FieldDescription = "Convective precipitation";
                    Unit = Units.KgPerSquareMeter;
                    break;
                case "PRATE":
                    FieldDescription = "Precipitation rate";
                    Unit = Units.KgPerSquareMeterPerSeconds;
                    break;

                default:
                    break;
            }
        }
    }
}
