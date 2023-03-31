using MetarEditor.Core;
using SharedMemory;
using System;
using System.Windows;
using System.Threading.Tasks;
using MetarReceiver.MVVM.Model;
using System.Text.Json;

namespace MetarReceiver.MVVM.ViewModel
{
    internal class MainViewModel : ObservableObject
    {
        private bool _isReading;

        private MemoryFile _memoryFile;

        private string _receivedMetar;

        public string ReceivedMetar 
        {
            get { return _receivedMetar; }
            set
            {
                _receivedMetar = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel() 
        {
            _isReading = false;
            _memoryFile = new MemoryFile("MetarFile");
        }

        private async Task ReceivingData() 
        {
            try
            {
                _memoryFile.OpenNonPersisted();

                while (_isReading)
                {
                    var data = await _memoryFile.ReadDataAsync();

                    if (data != null) 
                        ReceivedMetar = JsonSerializer.Deserialize<MeteoReport>(data).ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public RelayCommand StartReadingCommand
        {
            get
            {
                return new RelayCommand(async (obj) =>
                {
                    _isReading = true;
                    await ReceivingData();
                });
            }
        }

        public RelayCommand EndReadingCommand
        {
            get
            {
                return new RelayCommand((obj) => _isReading = false );
            }
        }

    }
}
