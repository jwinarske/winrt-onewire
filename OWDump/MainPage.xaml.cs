﻿using System;
using System.Collections.Generic;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace OWDump
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //string[] args = new string[] { "r", "TIME_TEST" };
            //string[] args = new string[] { "k", "TIME_TEST" }; //Fails on second read, as it's expecting status byte at offset 0, not the case...
            //string[] args = new string[] { "p", "TIME_TEST" };
            //string[] args = new string[] { "r" };
            //string[] args = new string[] { "k" }; //Fails on second read, as it's expecting status byte at offset 0, not the case...
            string[] args = new string[] { "p" };

            OWDump1.Main1(args);
        }
    }
}
