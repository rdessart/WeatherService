using System;
using System.Collections.Generic;
using System.Text;

namespace GlobalForcastSystem
{
    public class GFSLayer
    {
        protected int Forcast;
        public double GeoAltitudeFt { get; set; }
        public double BaroAltitudeFt { get; set; }
        public double TemperatureCel { get; set; }
        public double WindUSpeed { get; set; }
        public double WindVSpeed { get; set; }
        public double RelativeHumidity { get; set; }
        public double WindDirectionTrue
        {
            get
            {
                return 180 + (180.0/Math.PI) * Math.Atan2(-1 * WindUSpeed, -1 * WindVSpeed);
            }

        }
        public double WindSpeedKts { 
            get
            {
                return Math.Sqrt(Math.Pow(WindUSpeed, 2) + Math.Pow(WindVSpeed, 2)) * 3.280839895013;
            }
        }
        public bool IsLayerTropopause { get; protected set; }
        public bool IsLayerSurface { get; protected set; }

        public GFSLayer()
        {
            GeoAltitudeFt = double.MinValue;
            BaroAltitudeFt = double.MinValue;
            TemperatureCel = double.MinValue;
            WindUSpeed = 0.0;
            WindVSpeed = 0.0;
            RelativeHumidity = double.MinValue;
            IsLayerTropopause = false;
            IsLayerTropopause = false;
        }
        public int GetForcastTime()
        {
            return Forcast;
        }

        public GFSLayer(double pressurehPa, List<GFSLine> datas)
        {
            Forcast = datas[0].ForcastTime;
            foreach (GFSLine data in datas)
            {
                if (pressurehPa == -1)
                {
                    IsLayerTropopause = true;
                }
                else if (pressurehPa == -2)
                {
                    IsLayerSurface = true;
                }
                switch (data.Field)
                {
                    case "UGRD":
                        WindUSpeed = data.Value;
                        break;
                    case "VGRD":
                        WindVSpeed = data.Value;
                        break;
                    case "RH":
                        RelativeHumidity = data.Value;
                        break;
                    case "HGT":
                        GeoAltitudeFt = data.Value * 3.280839895013;
                        break;
                    case "TMP":
                        TemperatureCel = data.Value - 273.15;
                        break;
                    default:
                        break;
                }
                if(data.IsPressureAltitude && pressurehPa > 0)
                {
                    GeoAltitudeFt = pressurehPa * 3.280839895013;
                }
                
            }
        }
    }
}
