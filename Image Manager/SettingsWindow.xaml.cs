﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Image_Manager.Properties;
using Brush = System.Drawing.Brush;

namespace Image_Manager
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        private MainWindow main;

        public SettingsWindow(MainWindow main)
        {
            InitializeComponent();

            this.main = main;
            
        }

        // The Prefix name setter
        private void Prefix_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;

            // Saves the new default text to settings
            var mediaElement = (TextBox)sender;
            string text = mediaElement.Text;

            // Disallow empty characters
            if (string.IsNullOrWhiteSpace(text))
            {
                AnimateBackground(mediaElement, Colors.OrangeRed);
                return;
            }

            Settings.Default.PrefixName = text;
            Keyboard.ClearFocus();

            AnimateBackground(mediaElement, Colors.GreenYellow);
        }

        // Displays a short animation to indicate that changes have been saved
        private void AnimateBackground(Control elem, Color color)
        {
            ColorAnimation ca = new ColorAnimation(Colors.White, new Duration(TimeSpan.FromSeconds(0.5)));
            elem.Background = new SolidColorBrush(color);
            elem.Background.BeginAnimation(SolidColorBrush.ColorProperty, ca);
        }

        // Updates all changed values in the main window
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            main.UpdateSettingsChanged();
        }
    }
}