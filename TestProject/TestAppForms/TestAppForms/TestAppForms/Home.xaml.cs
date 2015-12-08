﻿using System;
using System.Collections.Generic;
using TestAppForms.Models;
using TestAppForms.Pages;
using Xamarin.Forms;
using TestAppForms.Helpers;

namespace TestAppForms
{
    public partial class Home : ContentPage
    {

        public Home()
        {
            InitializeComponent();

            PluginList.ItemSelected += (sender, args) =>
            {

                if (PluginList.SelectedItem == null)
                    return;

                switch (((PluginItem)PluginList.SelectedItem).Name)
                {
                    case "Battery":
                        Navigation.PushAsync(new BatteryPage());
                        break;
                    case "Connectivity":
                        Navigation.PushAsync(new ConnectivityPage());
                        break;
                    case "Compass":
                        Navigation.PushAsync(new CompassPage());
                        break;
                    case "Contacts":
                        Navigation.PushAsync(new ContactsPage());
                        break;
                    case "Device Info":
                        Navigation.PushAsync(new DeviceInfoPage());
                        break;
                    case "External Maps":
                        Navigation.PushAsync(new ExternalMapsPage());
                        break;
                    case "Geolocator":
                        Navigation.PushAsync(new GeolocatorPage());
                        break;
                    case "ImageView Circle":
                        Navigation.PushAsync(new ImageCirclePage());
                        break;
                    case "Local Notifications":
                        Navigation.PushAsync(new LocalNotificationsPage());
                            break;
                    case "Media":
                        Navigation.PushAsync(new MediaPage());
                        break;
                    case "Messaging":
                        Navigation.PushAsync(new MessagingPage());
                        break;
                    case "Permissions":
                        Navigation.PushAsync(new PermissionsPage());
                        break;
                    case "Settings":
                        Navigation.PushAsync(new SettingsPage());
                        break;
                    case "Share":
                        Navigation.PushAsync(new SharePage());
                        break;
                    case "Text to Speech":
                        Navigation.PushAsync(new TextToSpeechPage());
                        break;
                    case "Vibrate":
                        Navigation.PushAsync(new VibratePage());
                        break;
                    case "Learn more":
                        Device.OpenUri(new Uri("http://github.com/xamarin/plugins"));
                        break;
                    case "James on Twitter":
                        Device.OpenUri(new Uri("http://mobile.twitter.com/jamesmontemagno"));
                        break;
                }

                PluginList.SelectedItem = null;
            };

            int count = 0;

            var cell = new DataTemplate(typeof(ImageCell));
            cell.SetBinding(TextCell.TextProperty, "Name");
            cell.SetBinding(TextCell.DetailProperty, "Description");
            cell.SetBinding(ImageCell.ImageSourceProperty, new Binding("Image"));
            PluginList.ItemTemplate = cell;

            var items = new List<PluginItem>
            {
                new PluginItem
                {
                    Name = "Battery",
                    Image = "http://www.refractored.com/images/battery_icon.png",
                    Description = "Access battery stats and charging information.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Connectivity",
                    Image = "http://www.refractored.com/images/connectivity_icon.png",
                    Description = "Check if connected to the internet and type of connection.",
                    Id = count++
                    },
                new PluginItem
                {
                    Name = "Compass",

                    Image = "http://www.jarleysoft.com/images/compass_plugin_icon.png",
                    Description = "Provides a simple way to get compass heading.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Contacts",

                    Image = "http://www.refractored.com/images/plugin_icon_contacts.png",
                        Description = "Browse through all contacts on device.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Device Info",
                    Image = "http://www.refractored.com/images/device_info_icon.png",
                    Description = "Check type of phone, OS, and more.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "External Maps",
                    Image = "http://www.refractored.com/images/external_map_icon.png",
                    Description = "Launch maps application to address or GPS.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Geolocator",
                    Image = "http://www.refractored.com/images/plugin_icon_geolocator.png",
                    Description = "Gather GPS information.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "ImageView Circle",
                    Image = "http://www.refractored.com/images/circle_image_icon.png",
                    Description = "Xamarin.Forms Circle Image Control.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Local Notifications",
                    Image = "https://raw.githubusercontent.com/edsnider/Xamarin.Plugins/master/Notifier/NuGet/Xam.Plugins.Notifier.icon.png",
                    Description = "Display notifications to the system.",
                    Id = count++
                    },
                new PluginItem
                {
                    Name = "Media",
                    Image = "http://www.refractored.com/images/plugin_icon_media.png",
                    Description = "Take or pick photos and videos.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Messaging",
                    Image = "https://raw.githubusercontent.com/cjlotz/Xamarin.Plugins/master/Messaging/Common/MessagingSmall.png",
                    Description = "Send sms, email, or place a phone call..",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Permissions",
                    Image = "http://refractored.com/images/plugin_permissions.png",
                    Description = "Check and Request permissions from shared code.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Settings",
                    Image = "http://www.refractored.com/images/pcl_settings_icon.png",
                    Description = "Cross platform settings (string, int, double, etc.)",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Share",
                    Image = "http://refractored.com/images/plugin_share.png",
                    Description = "Share text, links, or open the browser.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Text to Speech",
                    Image = "http://www.refractored.com/images/tts_icon_large.png",
                    Description = "Speak back text and gather languages on device.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Vibrate",
                    Image = "http://www.refractored.com/images/vibrate_icon_large.png",
                    Description = "Vibrate the device easily.",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "Learn more",
                    Image = "https://raw.githubusercontent.com/jamesmontemagno/Xamarin-Templates/master/Plugins-Templates/icons/plugin_icon.png",
                    Description = "github.com/xamarin/plugins",
                    Id = count++
                },
                new PluginItem
                {
                    Name = "James on Twitter",
                    Image = "https://s.gravatar.com/avatar/5df4d86308e585c879c19e5f909d8bfe?s=80",
                    Description = "@JamesMontemagno",
                    Id = count++
                },

            };


            PluginList.ItemsSource = items;
            BindingContext = items;



        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (Settings.FirstRun)
            {
                Settings.FirstRun = false;

                await DisplayAlert("Welcome", "Welcome to the Plugins for Xamarin sample! All of these pages highlight a different cross-platform plugin for Xamarin. In fact I just used the settings plugin to see if it was your first run or not! How cool! Check it out and leave feedback on the GitHub page or tweet @JamesMontemagno", "OK");
            }
        }
    }
}
