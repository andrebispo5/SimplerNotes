using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO.IsolatedStorage;
using Newtonsoft.Json;
using System.IO;

namespace PhoneApp1
{
    public partial class PivotPage1 : PhoneApplicationPage
    {

        private IsolatedStorageSettings appsettings = IsolatedStorageSettings.ApplicationSettings;
        private KeyStorage[] keyStorage;

        public PivotPage1()
        {
            InitializeComponent();
            var token = appsettings["TOKEN"];
            var email = appsettings["EMAIL"];
           
            var client = new WebClient();
            //AFTER PAGE FINISHES LOADING REMOVES BACK TO CLOSE THE APP
            Loaded += (s, e) =>{ NavigationService.RemoveBackEntry(); };
            //CALLBACK FUNCTION TO GET THE NOTE KEYS
            client.DownloadStringCompleted += (send, e) =>
            {
                //JSON PARSER AND STORAGE
                keyStorage = JsonConvert.DeserializeObject<KeyStorage[]>(e.Result.ToString());
                                
                foreach (var item in keyStorage)
                {
                    if (item.deleted == "False")
                    {
                        //GETS THE NOTE FOR EACH KEY AND CALL RESPONSE_COMPLETED CALLBACKFUNCTION
                        HttpWebRequest httpWebRequest = (HttpWebRequest)HttpWebRequest.Create( new Uri("https://simple-note.appspot.com/api/note?key=" + item.key + "&auth=" + token + "&email=" + email));
                        httpWebRequest.BeginGetResponse(Response_Completed, httpWebRequest);
                    }
                    
                }

            };

            //STARTS ASSYNC WEBCLIENT REQUEST 
            client.DownloadStringAsync(new Uri("https://simple-note.appspot.com/api/index?auth=" + token + "&email=" + email));

        }

        private void Response_Completed(IAsyncResult ar)
        {
            HttpWebRequest request = (HttpWebRequest)ar.AsyncState;
            HttpWebResponse response = (HttpWebResponse)request.EndGetResponse(ar);
            using (TextReader reader = new StreamReader(response.GetResponseStream()))
            {
                var title = reader.ReadLine();
                var body = reader.ReadToEnd();
                string noteKey = response.Headers["note-key"];
               
                foreach (var item in keyStorage) {
                    if (item.key.Equals(noteKey)) {
                        item.title = title;
                        item.body = body;
                    }
                }

                
                Dispatcher.BeginInvoke(() =>
                {
                    Button tmpBtn = new Button();
                    TextBlock tmpTxt = new TextBlock();
                    tmpTxt.Text = title;
                    tmpTxt.TextWrapping = TextWrapping.Wrap;
                    tmpBtn.Height = 95;
                    tmpBtn.FontSize = 20;
                    tmpBtn.Background = new SolidColorBrush(Color.FromArgb(255,27,161,226));
                    tmpBtn.Content = tmpTxt;
                    tmpBtn.Click += (a, b) => { 
                        editPanel.Text = title + "\n" + body; 
                        Pivot.SelectedIndex = 1; 
                    };
                    tmpBtn.BorderThickness = new Thickness(0);
                    notesSP.Height += 95;
                    notesSP.Children.Add(tmpBtn);
                });
                
            }
        }

        public class KeyStorage
        {
            public string deleted { get; set; }
            public string modify { get; set; }
            public string key { get; set; }
            public string title { get; set; }
            public string body { get; set; }
        }

        private void ApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            var autolog = "";
            if (appsettings.Contains("AUTOLOGIN"))
            {
                autolog = appsettings["AUTOLOGIN"].ToString();
            }
            appsettings["AUTOLOGIN"] = "False";
            
            NavigationService.Navigate(new Uri("/LoginPage.xaml", UriKind.Relative));
        }

        private void editPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            String activeNote = "";
            String updateContent;
            MessageBox.Show("fuck c:");
            var client = new WebClient();
            if (appsettings.Contains("ACTIVENOTE"))
            {
                activeNote = appsettings["ACTIVENOTE"].ToString();
            }
            var token = appsettings["TOKEN"];
            var email = appsettings["EMAIL"];
            updateContent = activeNote + "&auth=" + token + "&email=" + email + "&content" + editPanel.Text;
            client.DownloadStringCompleted += (send, ear) =>
            {
                if (ear.Error == null)
                { MessageBox.Show("fuck c: "+e.ToString()); }
                else
                {
                    MessageBox.Show(editPanel.Text);
                }

            };
            client.UploadStringAsync(new Uri("http://simplenote-export.appspot.com/api2/data/" + activeNote), "POST", updateContent);
        }

    }
}