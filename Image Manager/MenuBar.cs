﻿using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using Image_Manager.Properties;
using Microsoft.VisualBasic;

namespace Image_Manager
{
    partial class MainWindow
    {
        // Undo
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UndoMove();
        }

        // Toggle sort view
        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            ToggleViewMode();
        }

        // Rename
        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            string input = Interaction.InputBox("Rename", "Select a new name",
                _currentItem.GetFileNameExcludingExtension());
            RenameFile(input);
        }

        // Add prefix
        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            string hqFileName =
                Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
            string hqInput = QuickPrefix + hqFileName;
            RenameFile(hqInput);
        }

        // Remove prefix
        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            string hQnoFileName =
                Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
            string hQnoInput = hQnoFileName?.Replace(QuickPrefix, "");
            RenameFile(hQnoInput);
        }

        // Open folder
        private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.ShowDialog();
                Console.WriteLine(dialog.SelectedPath);
                AddNewFolder(new[] { dialog.SelectedPath });
            }
        }

        // Clear view
        private void MenuItem_Click_9(object sender, RoutedEventArgs e)
        {
            RemoveOldContext();
            DirectoryTreeList.Items.Clear();

            _originFolder?.GetAllFolders()?.Clear();
            _originFolder?.GetAllShownFolders()?.Clear();
            MakeTypeVisible("");
        }

        // Toggle subfolders
        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            _showSubDir = !_showSubDir;
            UpdateTitle();
        }
        

        // Toggle special folders
        private void MenuItem_Click_7(object sender, RoutedEventArgs e)
        {
            _showSets = !_showSets;
            UpdateTitle();
        }

        // Toggle prefixed content
        private void MenuItem_Click_8(object sender, RoutedEventArgs e)
        {
            _showPrefix = !_showPrefix;
            UpdateTitle();
        }

        // Toggle prefixed content
        private void MenuItem_Click_12(object sender, RoutedEventArgs e)
        {
            _allowOtherFiles = !_allowOtherFiles;
            UpdateTitle();
        }

        // Settings Window
        private void MenuItem_Click_13(object sender, RoutedEventArgs e)
        {
            new SettingsWindow(this).Show();
        }

        // Open README
        private void MenuItem_Click_10(object sender, RoutedEventArgs e)
        {
            Process.Start("README.md");
        }

        // Open externally
        private void MenuItem_Click_11(object sender, RoutedEventArgs e)
        {
            if (_currentItem == null || !File.Exists(_currentItem?.GetFilePath())) return;
            Process.Start(_currentItem.GetFilePath());
        }
    }
}