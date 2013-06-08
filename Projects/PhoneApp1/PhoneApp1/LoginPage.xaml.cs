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
using System.Windows.Navigation;

namespace PhoneApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        private IsolatedStorageSettings appsettings = IsolatedStorageSettings.ApplicationSettings;
        public MainPage()
        {
            //CHECK FOR AUTOLOGIN
            InitializeComponent();
            Loaded += (s, e) =>
            {
                NavigationService.RemoveBackEntry();
                string autolog = "";
                if (appsettings.Contains("AUTOLOGIN"))
                {
                    emailBox.Text = appsettings["EMAIL"].ToString();
                    passBox.Password = appsettings["PASSWORD"].ToString();
                    autolog = appsettings["AUTOLOGIN"].ToString();
                }
                if (autolog == "True")
                {
                    emailBox.Text = appsettings["EMAIL"].ToString();
                    passBox.Password = appsettings["PASSWORD"].ToString();
                    autoCheck.IsChecked = true;
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }

            };
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            getToken();
        }

        private void getToken()
        {
            var client = new WebClient();
            String authContent = "email=" + emailBox.Text + "&password=" + passBox.Password;

            //ASSYNC CALLBACK FUNCTION
            client.UploadStringCompleted += (send, e) =>
            {
                //SUCCESFULLY CONNECTED TO SERVER?
                if (e.Error == null)
                {
                    string searchFor = "token";
                    int tokenIndex = e.Result.ToString().IndexOf(searchFor) + 14;
                    string token = e.Result.ToString().Substring(tokenIndex, 64);
                    //GOT AN INVALID TOKEN?
                    if (token.Contains("<") || token.Contains(" ") || token.Contains(">"))
                    {
                        MessageBox.Show("Connection fail! Please check your email and password.");
                        return;
                    }

                    //SAVES EMAIL PASSWORD AND TOKEN IN MEMORY
                    if (appsettings.Contains("EMAIL"))
                    {
                        appsettings.Remove("EMAIL");
                        appsettings.Remove("PASSWORD");
                        appsettings.Remove("TOKEN");
                    }

                    appsettings.Add("EMAIL", emailBox.Text);
                    appsettings.Add("PASSWORD", passBox.Password);
                    appsettings.Add("TOKEN", token);

                    if (autoCheck.IsChecked == true)
                    {
                        if (appsettings.Contains("AUTOLOGIN"))
                        {
                            appsettings.Remove("AUTOLOGIN");
                        }
                        appsettings.Add("AUTOLOGIN", true);
                    }
                    else
                    {
                        if (appsettings.Contains("AUTOLOGIN"))
                        {
                            appsettings.Remove("AUTOLOGIN");
                        }
                        appsettings.Add("AUTOLOGIN", false);
                    }
                    appsettings.Save();
                    //GOES TO MAIN PAGE
                    NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
                }
                else
                {
                    MessageBox.Show("Connection fail! Please check your email and password.");
                }
            };

            //ASSYNC POST 
            client.UploadStringAsync(new Uri("http://simplenote-export.appspot.com/auth"), "POST", authContent);
        }

    }
}