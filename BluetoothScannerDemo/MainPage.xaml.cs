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


// Pour plus d'informations sur le modèle d'élément Page vierge, consultez la page https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BluetoothScannerDemo
{
    /// <summary>
    /// Une page vide peut être utilisée seule ou constituer une page de destination au sein d'un frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /** BLE watcher */
        private BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();

        /** scanning state */
        private bool watchStarted = false;

        /** search filter */
        private string filter = String.Empty;

        public MainPage()
        {
            this.InitializeComponent();
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
            this.tagList.Clear();
            this.tagColl.Clear();
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
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            filter = txtSearch.Text;
            this.tagColl.Clear();
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
                
                await Windows.Storage.FileIO.WriteTextAsync(file, file.Name);

                foreach(Taginfo tag in tagList.Values)
                {
                    if(tag.TagName.Length >= 8)
                    {
                        if (tag.TagName.Substring(0, 4).Equals("IMEI"))
                        {
                            await Windows.Storage.FileIO.WriteTextAsync(file, tag.TagName.Substring(8) + '\t' + tag.TagMenu);

                        }

                    }
                }


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
                        if (taginfo.TagName.Length == 6)
                        {
                            taginfo.TagName = "IMEI : 3596271" + taginfo.TagName;
                        }
                        if (taginfo.TagDataRaw.Count >= 3 && taginfo.TagDataRaw[2].Length > 6)
                        {
                            taginfo.TagMenu = taginfo.TagDataRaw[2].Replace("-", "").Substring(4);
                        }
                        else
                        {
                            taginfo.TagMenu = "";
                        }
                    }
                }
                taginfo.getData();
                
                if (taginfo.TagName.Equals(String.Empty) == false)
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

    }
}