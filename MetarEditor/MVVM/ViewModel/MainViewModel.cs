using MetarEditor.Core;
using MetarEditor.MVVM.Model;
using SharedMemory;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Threading;
using Metar.Decoder;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using System.Windows.Markup;

namespace MetarEditor.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private const string FILE_MAME = "MetarFile";

        private MeteoRecord _metar;

        public MeteoRecord Metar
        {
            get { return _metar; }
            set
            {
                _metar = value;
                OnPropertyChanged();
            }
        }

        private MemoryFile _memoryFile;

        public string InitialMetar { get; }

        public string Aerodrome
        {
            get { return _metar.Aerodrome; }
            set
            {
                if (value != _metar.Aerodrome)
                {
                    _metar.Aerodrome = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ObservationTime
        {
            get { return _metar.ObservationTime; }
            set
            {
                if (value != _metar.ObservationTime)
                {
                    _metar.ObservationTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string SurfaceWind
        {
            get { return _metar.SurfaceWind; }
            set
            {
                if (value != _metar.SurfaceWind)
                {
                    _metar.SurfaceWind = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Visibility
        {
            get { return _metar.Visibility; }
            set
            {
                if (value != _metar.Visibility)
                {
                    _metar.Visibility = value;
                    OnPropertyChanged();
                }
            }
        }

        public string RunwaysVisualRange
        {
            get { return _metar.RunwaysVisualRange; }
            set
            {
                if (value != _metar.RunwaysVisualRange)
                {
                    _metar.RunwaysVisualRange = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PresentWeather
        {
            get { return _metar.PresentWeather; }
            set
            {
                if (value != _metar.PresentWeather)
                {
                    _metar.PresentWeather = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Clouds
        {
            get { return _metar.Clouds; }
            set
            {
                if (value != _metar.Clouds)
                {
                    _metar.Clouds = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TempAndDew
        {
            get { return _metar.TempAndDew; }
            set
            {
                if (value != _metar.TempAndDew)
                {
                    _metar.TempAndDew = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Pressure
        {
            get { return _metar.Pressure; }
            set
            {
                if (value != _metar.Pressure)
                {
                    _metar.Pressure = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel() 
        {
            InitialMetar = "METAR YSCB 211720Z 31005MPS 280V350 //// R23L/M0750 R23L/M0750U R23L/M0750d FEW015 SCT015 15/13 Q1005";

            //создание файла памяти
            try
            {
                _memoryFile = new MemoryFile(FILE_MAME);
                _memoryFile.CreateNonPersisted();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ParseMetar();
        }

        //парсинг исходной сводки
        private void ParseMetar()
        {
            Dictionary<string, string> metarPatterns = new Dictionary<string, string>
            {
                { "Type", "(?<type>METAR|SPECI)" },
                { "Aerodrome", @"[^(METAR|SPECI)]([A-Z][A-Z0-9]{3})" },
                { "ObservationTime", "(?<day>[0-9]{2})(?<hour>[0-9]{2})(?<min>[0-9]{2})Z" },
                { "SurfaceWind", "([0-9]{3}|VRB)([0-9]{2,3})G?([0-9]{2,3})?(KT|MPS|KMH)" },
                { "Visibility", @"(?<vis>(?<dist>(M|P)?\d\d\d\d|////)" +
                                @"(?<dir>[NSEW][EW]? | NDV)? |" +
                                @"(?<distu>(M|P)?(\d+|\d\d?/\d\d?|\d+\s+\d/\d))" +
                                @"(?<units>SM|KM|M|U) | CAVOK )" },
                { "RunwaysVisualRange", @"(RVRNO|R(?<name>\d\d(RR?|LL?|C)?)/(?<low>(M|P)?(\d\d\d\d|/{4}))" +
                                        @"(V(?<high>(M|P)?\d\d\d\d))?" +
                                        @"(?<unit>FT)?[/NDU]*)" },
                { "PresentWeather", @"(?<int>(-|\+|VC)*)" +
                                    @"(?<desc>(MI|PR|BC|DR|BL|SH|TS|FZ)+)?" +
                                    @"(?<prec>(DZ|RA|SN|SG|IC|PL|GR|GS|UP|/)*)" +
                                    @"(?<obsc>BR|FG|FU|VA|DU|SA|HZ|PY)?" +
                                    @"(?<other>PO|SQ|FC|SS|DS|NSW|/+)?" +
                                    @"(?<int2>[-+])?" },
                { "Clouds", @"(?<cover>VV|CLR|SKC|SCK|NSC|NCD|BKN|SCT|FEW|[O0]VC|[^/]///[^/])" +
                            @"(?<height>[\dO]{2,4}|///)?" +
                            @"(?<cloud>([A-Z][A-Z]+|///))?" },
                { "TempAndDew", @"(?<temp>(M|-)?\d{1,2}|//|XX|MM)/(?<dewpt>(M|-)?\d{1,2}|//|XX|MM)" },
                { "Pressure", @"(?<unit>A|Q|QNH)(?<press>[\dO]{3,4}|////)(?<unit2>INS)?" }
            };

            _metar.Name = Regex.Match(InitialMetar, metarPatterns["Type"], RegexOptions.Compiled).Value;
            Aerodrome = Regex.Match(InitialMetar, metarPatterns["Aerodrome"], RegexOptions.Compiled).Value;
            ObservationTime = Regex.Match(InitialMetar, metarPatterns["ObservationTime"], RegexOptions.Compiled).Value;
            SurfaceWind = Regex.Match(InitialMetar, metarPatterns["SurfaceWind"], RegexOptions.Compiled).Value;
            Visibility = Regex.Match(InitialMetar, metarPatterns["Visibility"], RegexOptions.Compiled).Value;
            RunwaysVisualRange = String.Join(' ', Regex.Matches(InitialMetar, metarPatterns["RunwaysVisualRange"], RegexOptions.Compiled).Select(o => o.Value));
            PresentWeather = Regex.Match(InitialMetar, metarPatterns["PresentWeather"], RegexOptions.Compiled).Value;
            Clouds = String.Join(' ', Regex.Matches(InitialMetar, metarPatterns["Clouds"], RegexOptions.Compiled).Select(o => o.Value));
            TempAndDew = Regex.Match(InitialMetar, metarPatterns["TempAndDew"], RegexOptions.Compiled).Value;
            Pressure = Regex.Match(InitialMetar, metarPatterns["Pressure"], RegexOptions.Compiled).Value;
        }

        public RelayCommand SendCommand
        {
            get
            {
                return new RelayCommand(async (obj) =>
                {     
                    try
                    {
                        await _memoryFile.WriteDataAsync(_metar);
                    }
                    catch (Exception ex) 
                    {
                       MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }
    }
}
