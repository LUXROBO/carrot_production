using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Storage.Streams;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using Windows.UI;


// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BluetoothScannerDemo
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        /** BLE watcher */
        private BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
        /** scanning state */
        private bool watchStarted = false;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
        /** search filter */
        private string filter = String.Empty;
        private int _setCount = 39;
        private String _count = "0";
        public String Count
        {
            private set
            {
                _count = value;
                txtCount.Text = _count;
                OnPropertyChanged("TagCount");
            }
            get
            {
                return _count;
            }
        }
        private string __passCount = "0";
        private int _passCount = 0;

        public String PassCount
        {
            private set
            {
                __passCount = value;
                txtPassCount.Text = Convert.ToString(__passCount);
                if (_passCount == _setCount)
                    txtPassCount.Foreground = new SolidColorBrush(Colors.LimeGreen);
                else if(_passCount == int.Parse(_count))
                    txtPassCount.Foreground = new SolidColorBrush(Colors.Yellow);
                else
                    txtPassCount.Foreground = new SolidColorBrush(Colors.RoyalBlue);
                OnPropertyChanged("PassCount");
            }
            get
            {
                return Convert.ToString(__passCount);
            }
        }


        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }



        /** list of showned tag linked to display */
        public ObservableCollection<Taginfo> tagColl = new ObservableCollection<Taginfo>();

        /** dictionnary of known tag */
        public Dictionary<string, Taginfo> tagList = new Dictionary<string, Taginfo>();

        #region event declaration
        /** @brief : start/stop scanning depending on the current status
         */
        private void BtnStartBle_Click(object sender, RoutedEventArgs e)
        {
            //start scanning
            if (this.watchStarted == false)
            {
                this.watcher.Received += Tag_Received;
                this.watcher.Start();
                this.watchStarted = true;

                //update pictogram
                this.btnStartBle.Content = Char.ConvertFromUtf32(0xE769);
            }
            //stop scanning
            else if (this.watchStarted == true)
            {
                this.watcher.Stop();
                this.watcher.Received -= Tag_Received;
                this.watchStarted = false;

                //update pictogram
                this.btnStartBle.Content = Char.ConvertFromUtf32(0xE768);
            }
        }

        /** @brief : forget all tag
         * clear dictionnary and observable collection 
         */
        private void BtnClearBle_Click(object sender, RoutedEventArgs e)
        {
            if(this.watchStarted)
                this.watcher.Stop();
            this.tagList.Clear();
            this.tagColl.Clear();
            Count = "0";
            PassCount = "0";
            if(this.watchStarted)
                this.watcher.Start();
        }

        /** @brief : update filter for the search
         * update shown tags depending on the search
         */
        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            filter = txtSearch.Text;
            this.tagColl.Clear();

            //update shown tags corresponding to search text
            foreach (KeyValuePair<string, Taginfo> pair in this.tagList)
            {
                Taginfo taginfo = new Taginfo();
                taginfo = pair.Value;
                if (Filter(taginfo))
                {
                    this.tagColl.Add(taginfo);
                }
            }
        }
        private void TxtSetCount_TextChanged(object sender, TextChangedEventArgs e)
        {
            _setCount = int.Parse(txtSetCount.Text);
        }
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var savePicker = new Windows.Storage.Pickers.FileSavePicker();
            savePicker.SuggestedStartLocation =
                Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            // Dropdown of file types the user can save the file as
            savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
            // Default file name if the user does not type one in or select a file to replace
            savePicker.SuggestedFileName = "list.txt";
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file

                String WriteLineBuffer = "";

                foreach(Taginfo tag in tagList.Values)
                {
                    if(tag.CarrotPlugFlag)
                    {
                        String WriteLine = tag.TagName.Substring(7) + '\t'+ tag.TagMenu.Substring(7, tag.TagMenu.Length - 7) + '\n';
                        WriteLineBuffer += WriteLine;
                    }
                }
                await Windows.Storage.FileIO.WriteTextAsync(file, WriteLineBuffer);


                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);


            }

        }
        /** @brief : call when tag infos are recieved
         */
        private async void Tag_Received(BluetoothLEAdvertisementWatcher received, BluetoothLEAdvertisementReceivedEventArgs args)
        {
            
            //show only connectable tags
            if (args.AdvertisementType == BluetoothLEAdvertisementType.NonConnectableUndirected || args.AdvertisementType == BluetoothLEAdvertisementType.ConnectableUndirected)
            {
                //get tag infos
                Taginfo taginfo = new Taginfo();
                taginfo.TagMac = args.BluetoothAddress.ToString("X");
                taginfo.TagName = args.Advertisement.LocalName;

                //get tag datas
                string datasection = String.Empty;
                foreach (BluetoothLEAdvertisementDataSection section in args.Advertisement.DataSections)
                {
                    var data = new byte[section.Data.Length];
                    using (var reader = DataReader.FromBuffer(section.Data))
                    {
                        reader.ReadBytes(data);
                        datasection = String.Format("{0}", BitConverter.ToString(data));
                        taginfo.TagDataRaw.Add(datasection);
                        taginfo.TagRssi = args.RawSignalStrengthInDBm;
                        try
                        {
                            if (taginfo.TagName.Length == 8)
                            {
                                taginfo.TagName = "IMEI : 3596271" + taginfo.TagName;
                                taginfo.CarrotPlugFlag = true;
                            }

                            if (taginfo.TagDataRaw.Count >= 3 && taginfo.TagDataRaw[2].Length > 6)
                            {
                                taginfo.TagFlagString = "";
                                string flag_string = taginfo.TagDataRaw[2].Replace("-", "").Substring(0, 2);
                                taginfo.TagFlag = uint.Parse(flag_string);
                                if ((taginfo.TagFlag & 0x01) == 0x01)
                                    taginfo.TagFlagString += "GPS Fail";
                                else
                                    taginfo.TagFlagString += "GPS OK";
                                taginfo.TagFlagString += ", ";
                                if ((taginfo.TagFlag & 0x02) == 0x02)
                                    taginfo.TagFlagString += "BLE Fail";
                                else
                                    taginfo.TagFlagString += "BLE OK";
                                taginfo.TagFlagString += ", ";
                                if ((taginfo.TagFlag & 0x0C) == 0x08)
                                    taginfo.TagFlagString += "CAP Low";
                                else if ((taginfo.TagFlag & 0x0C) == 0x04)
                                    taginfo.TagFlagString += "CAP Over";
                                else
                                    taginfo.TagFlagString += "CAP OK";
                                taginfo.TagFlagString += ", ";
                                if ((taginfo.TagFlag & 0xF0) != 0x00)
                                    taginfo.TagFlagString += "LTE Fail";
                                else
                                    taginfo.TagFlagString += "LTE OK";

                                taginfo.TagMenu = "\tCCID : " + taginfo.TagDataRaw[2].Replace("-", "").Substring(4);
                            }
                            else
                            {
                                taginfo.TagMenu = "";
                            }
                        }
                        catch
                        {

                        }
                    }
                }
                taginfo.getData();

                if (taginfo.CarrotPlugFlag)
                {
                    if (taginfo.TagName.Contains(filter))
                    {
                        //add new tag
                        if (this.tagList.ContainsKey(taginfo.TagMac) == false)
                        {
                            this.tagList.Add(taginfo.TagMac, taginfo);
                            if (Filter(taginfo))
                            {
                                await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                                {
                                    tagColl.Add(taginfo);
                                });
                            }
                        }
                        //update existing tag infos
                        else if (tagList.ContainsValue(taginfo) == false)
                        {
                            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                            {
                                IEnumerable<Taginfo> existing = tagColl.Where(x => x.TagMac == taginfo.TagMac);
                                int a = tagColl.IndexOf(existing.FirstOrDefault());
                                if (Filter(taginfo))
                                {
                                    if (a >= 0 && a < tagColl.Count())
                                    {
                                        tagColl[a].update(taginfo);
                                    }
                                }
                                this.tagList[taginfo.TagMac].update(taginfo);
                            });
                        }
                        await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                        {
                            _passCount = 0;
                            foreach (Taginfo tag in tagColl)
                            {
                                if ((tag.TagFlag & 0xFF ) == 0 || (tag.passFlag == 1))
                                {
                                    _passCount++;
                                    tag.passFlag = 1;
                                }
                            }
                            Count = Convert.ToString(tagColl.Count);
                            PassCount = Convert.ToString(_passCount);
                        });
                    }
                }
            }
        }
        #endregion

        /** @brief : filter tag mac adress or name
         * @return : true if tag matches false otherwise
         */
        private bool Filter(Taginfo taginfo)
        {
            if (taginfo.TagMac.ToLower().Contains(filter.ToLower()) || taginfo.TagName.ToLower().Contains(filter.ToLower()))
            {
                return true;
            }
            return false;
        }

        private void btnUpdateDB_Click(object sender, RoutedEventArgs e)
        {

            Mydb mydb = new Mydb();
            foreach (Taginfo tag in tagList.Values)
            {
                if (tag.CarrotPlugFlag)
                {
                    string icc_id = tag.TagMenu.Substring(7, tag.TagMenu.Length - 7);
                    string imei = tag.TagName.Substring(7);
                    string qa1 = tag.passFlag.ToString();
                    string ng1_type = tag.TagFlagString;
                    mydb.UpdateQuery(imei, icc_id, qa1, ng1_type);

                }
            }
        }
    }
}