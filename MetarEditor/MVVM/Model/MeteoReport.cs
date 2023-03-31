using System;

namespace MetarEditor.MVVM.Model
{
    internal struct MeteoRecord
    {
        public string Name { get; set; }

        public string Aerodrome { get; set; }

        public string ObservationTime { get; set; }

        public string SurfaceWind { get; set; }

        public string Visibility { get; set; }

        public string RunwaysVisualRange { get; set; } 

        public string PresentWeather { get; set; }

        public string Clouds { get; set; }

        public string TempAndDew { get; set; }

        public string Pressure { get; set; }

        public override string ToString()
        {
            return String.Join(' ', Name, Aerodrome, ObservationTime, SurfaceWind,
                                    Visibility, RunwaysVisualRange, PresentWeather, Clouds,
                                    TempAndDew, Pressure);
        }
    }
}
