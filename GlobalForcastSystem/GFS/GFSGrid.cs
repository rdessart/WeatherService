using System;
using System.Collections.Generic;

namespace GlobalForcastSystem
{
    public class GFSGrid
    {
        protected List<GFSLayer> _layers;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public List<GFSLayer> Layers {
            get => _layers;
            set
            {
                _layers = value;
                Forcast = value[0].Forcast;
            }
        }
        public TimeSpan Forcast { get; set; }

    }
}
