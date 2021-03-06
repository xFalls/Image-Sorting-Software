﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Xml;
using Image_Manager.Properties;
using Microsoft.VisualBasic;
using Color = System.Drawing.Color;
using Control = System.Windows.Controls.Control;
using DataFormats = System.Windows.DataFormats;
using DragEventArgs = System.Windows.DragEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using System.Globalization;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using ListViewItem = System.Windows.Controls.ListViewItem;
using Path = System.IO.Path;
using Point = System.Windows.Point;
using TextBox = System.Windows.Controls.TextBox;
using System;
using System.IO;
using System.Windows;
using System.Windows.Shell;

namespace Image_Manager
{
    /// <summary>
    /// The main window where content is loaded
    /// </summary>
    public partial class MainWindow
    {
        // Sets what type of folders to show
        private bool _showSubDir = false;
        private bool _showSets = true;
        private bool _showPrefix = true;
        public static bool _rescale = false;

        // List of all stored objects
        private Folder _originFolder;
        private readonly List<DisplayItem> _displayItems = new List<DisplayItem>();
        private readonly List<DisplayItem> _movedItems = new List<DisplayItem>();
        private List<Image> _previewContainer = new List<Image>();

        // Keeps track of changes in the folder structure
        public static List<string> NewFiles = new List<string>();
        public static List<string> MovedFiles = new List<string>();

        // The index of the displayed item in _displayItems
        private static int _displayedItemIndex;
        private DisplayItem _currentItem;

        // Other toggles
        private bool _isActive;
        private bool _isDrop;
        private bool _isTyping;
        private bool _isEndless;
        private bool _changed;
        private bool _renameShown = Settings.Default.Rename;

        // Image manipulation
        private Point _start;
        private Point _origin;
        private double _currentZoom = 1;
        private readonly TransformGroup _imageTransformGroup = new TransformGroup();
        private readonly TranslateTransform _tt = new TranslateTransform();
        private readonly ScaleTransform _st = new ScaleTransform();
        private readonly BlurEffect _videoBlur = new BlurEffect();

        SolidColorBrush currentBrush;
        SolidColorBrush GoldBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 255, 69, 0));
        SolidColorBrush GoldTBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 69, 0));
        SolidColorBrush PurpleBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 159, 49, 222));
        SolidColorBrush PurpleTBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 159, 49, 222));
        SolidColorBrush RedBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 230, 30, 88));
        SolidColorBrush RedTBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 230, 30, 88));
        SolidColorBrush BlueBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 39, 200, 226));
        SolidColorBrush BlueTBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 19, 200, 226));
        SolidColorBrush YellowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 243, 243, 1));
        SolidColorBrush YellowTBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 243, 243, 1));
        SolidColorBrush GreenBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(155, 50, 255, 50));
        SolidColorBrush GreenTBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 50, 255, 50));

        DoubleAnimation colorTransitionAnim = new DoubleAnimation(54, 18, new Duration(TimeSpan.FromSeconds(0.7)));

        public static int imageViewerSize;


        public MainWindow()
        {
            // Resets user settings to default values
            ////Settings.Default.Reset();

            // Loads all elements into view
            InitializeComponent();

            // Initializes variables used for zooming and panning
            _imageTransformGroup.Children.Add(_st);
            _imageTransformGroup.Children.Add(_tt);
            imageViewer.RenderTransform = _imageTransformGroup;

            // Sets default values
            _videoBlur.Radius = _defaultBlurRadius;
            DisplayItem.ShortLength = FileNameSize;
            UpdateSettingsChanged();
            UpdatePreviewLength();

            // Default view
            PreviewField.Visibility = Settings.Default.IsPreviewOpen ? Visibility.Visible : Visibility.Hidden;
            ShowSortMenuMenu.IsChecked = Settings.Default.SortMode;
            FolderGrid.Opacity = Settings.Default.SortMode ? 1 : 0;

            // Remove harmless error messages from output
            PresentationTraceSources.DataBindingSource.Switch.Level = SourceLevels.Critical;

            FillRecent();

            colorTransitionAnim.EasingFunction = new CubicEase();


            
            
            //JumpList
            //Needs to be able to handle Open With... for folders first
            /*
            //Configure a new JumpTask
            JumpTask jumpTask1 = new JumpTask();
            jumpTask1.Title = "Most Recent";
            jumpTask1.Arguments = "0";
            JumpTask jumpTask2 = new JumpTask();
            jumpTask2.Title = "Second Most Recent";
            jumpTask2.Arguments = "1";
            // Create and set the new JumpList.
            JumpList jumpList2 = new JumpList();
            jumpList2.JumpItems.Add(jumpTask1);
            jumpList2.JumpItems.Add(jumpTask2);
            JumpList.SetJumpList(App.Current, jumpList2);*/
        }


        public void FillRecent()
        {
            string folderString = Settings.Default.RecentFolders;
            string[] folders = folderString.Split(',');

            RecentMenu.Items.Clear();
            RecentItems.Items.Clear();

            JumpList jumpList = new JumpList();

            foreach (string folder in folders)
            {
                if (folder == "") continue;

                AddFolderToRecent(folder);

                // Fills taskbar jump list
                JumpTask jumpTask = new JumpTask();
                string[] folname = folder.Split('\\');
                jumpTask.Title = folname[folname.Length-1];
                jumpTask.Arguments = folder;
                jumpList.JumpItems.Add(jumpTask);
            }

            JumpList.SetJumpList(App.Current, jumpList);

            if (RecentMenu.Items.Count > 0)
            {
                RecentMenu.IsEnabled = true;
                RecentItems.IsEnabled = true;
            }
        }

        public void AddFolderToRecent(string folder)
        {
            MenuItem menu = new MenuItem();
            menu.Header = folder;
            menu.Click += MenuItem_Click_27;

            RecentMenu.Items.Add(menu);

            ListViewItem menu2 = new ListViewItem();
            menu2.MouseLeftButtonUp += MenuItem_Click_28;
            menu2.Content = folder;
            menu2.Foreground = Brushes.White;
            menu2.MinHeight = 30;
            //menu2.Margin = new Thickness(0,0,0,0);

            RecentItems.Items.Add(menu2);
        }

        public void AddFolderToSettings(string folder)
        {
            string folderString = Settings.Default.RecentFolders;
            string[] folders = folderString.Split(',');

            string newString = folder + ",";

            for (int i = 0; i < 15 && i < folders.Length; i++)
            {
                if (!newString.Contains(folders[i] + ","))
                    newString += folders[i] + ",";
            }

            Settings.Default.RecentFolders = newString;

            FillRecent();
        }


        public void UpdatePreviewLength()
        {
            PreviewContainer.Children.RemoveRange(0, PreviewContainer.Children.Count);
            _previewContainer.Clear();

            // Sets how many preview images to create in one direction
            for (int i = 0; i < _previewSteps * 2 + 1; i++)
            {
                string borderXAML = XamlWriter.Save(PreviewImage);
                StringReader stringReader = new StringReader(borderXAML);
                XmlReader xmlReader = XmlReader.Create(stringReader);
                Border previewImage = (Border)XamlReader.Load(xmlReader);

                previewImage.Visibility = Visibility.Visible;
                PreviewContainer.Children.Add(previewImage);
            }

            // Gets all preview image containers
            foreach (var item in PreviewContainer.Children)
            {
                _previewContainer.Add((Image)((Border)item).Child);
            }
        }


        /// <summary>
        /// Accepts a file and then returns its type as a string
        /// </summary>
        /// <param name="inputFile">The name of the given file</param>
        /// <returns>A string indicating the type of file given</returns>
        public static string FileType(string inputFile)
        {
            string temp = inputFile.ToLower();

            // Image file
            if (temp.EndsWith(".jpg") || temp.EndsWith(".jpeg") || temp.EndsWith(".tif") ||
                    temp.EndsWith(".tiff") || temp.EndsWith(".png") || temp.EndsWith(".bmp") ||
                    temp.EndsWith(".ico") || temp.EndsWith(".wmf") || temp.EndsWith(".emf") ||
                    temp.EndsWith(".webp") || temp.EndsWith(".jpg_large"))
                return "image";

            // Gif file
            if (temp.EndsWith(".gif"))
                return "gif";

            // Text file
            if (temp.EndsWith(".txt"))
                return "text";

            // Video file
            if (temp.EndsWith(".mp4") || temp.EndsWith(".mkv") || temp.EndsWith(".webm")
                 || temp.EndsWith(".wmv") || temp.EndsWith(".flv") || temp.EndsWith(".avi")
                 || temp.EndsWith(".mov"))
                return "video";

            return "file";
        }

        // Changes the visibility of all UI elements not related to the current displayed item
        private void MakeTypeVisible(string fileType)
        {
            imageViewer.Visibility = gifViewer.Visibility = gifViewer.Visibility =
                textViewer.Visibility = VideoPlayIcon.Visibility = iconViewer.Visibility =
                Visibility.Hidden;
            imageViewer.Effect = null;
            gifViewer.Source = null;
            GifItem._gifImage = null;

            switch (fileType)
            {
                case "image":
                    imageViewer.Visibility = Visibility.Visible;
                    break;
                case "gif":
                    gifViewer.Visibility = Visibility.Visible;
                    break;
                case "video":
                    VideoPlayIcon.Visibility = imageViewer.Visibility = Visibility.Visible;
                    imageViewer.Effect = _videoBlur;
                    break;
                case "text":
                    textViewer.Visibility = Visibility.Visible;
                    break;
                case "file":
                    iconViewer.Visibility = Visibility.Visible;
                    break;
            }
        }

        public void ShowStars(int stars, SolidColorBrush color)
        {
            foreach (TextBlock star in Starbar.Children)
            {
                star.Visibility = Visibility.Hidden;
            }

            for (int i = 0; i < stars; i++)
            {
                TextBlock star = (TextBlock) Starbar.Children[i];
                star.Visibility = Visibility.Visible;
                star.Foreground = color;
            }
        }
        

        // Changes the currently displayed content
        public void UpdateContent()
        {
            // Show specific view when content is empty
            if (_displayItems.Count == 0)
            {
                PreviewContent();
                UpdateSettingsChanged();
                CreateSortMenu();
                return;
            }

            // Disable undo menu when there's nothing to undo
            if (_movedItems.Count == 0)
            {
                UndoMenu.IsEnabled = false;
            }

            _currentItem = _displayItems[_displayedItemIndex];

            // If set, add prefix to previously unviewed images. Also sets a color based on type of prefix
            if (!_currentItem.GetFileName().StartsWith("=") && !_currentItem.GetFileName().StartsWith(QuickPrefix) && _renameShown)
            {
                string updatedName =
                    Path.GetFileNameWithoutExtension(_currentItem.GetFileNameExcludingExtension());
                string newInput = "=" + updatedName;

                RenameFile(newInput);
                _currentItem.SetIsNew();
                
                ColorInfo.Fill = new SolidColorBrush(Colors.LawnGreen);
                InfoContainer.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 124, 252, 0));


            }
            else if (_currentItem.GetFileName().StartsWith("++++++"))
            {
                // Show animation if color changes
                if (currentBrush != GoldBrush)
                {
                    MenuStrip.BeginAnimation(HeightProperty, colorTransitionAnim);
                }

                MenuStrip.Background = GoldBrush;
                ColorInfo.Fill = GoldBrush;
                InfoContainer.Background = GoldTBrush;
                currentBrush = GoldBrush;
                ShowStars(6, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 69, 0)));
            }
            else if (_currentItem.GetFileName().StartsWith("+++++"))
            {
                // Show animation if color changes
                if (currentBrush != PurpleBrush)
                {
                    MenuStrip.BeginAnimation(HeightProperty, colorTransitionAnim);
                }

                MenuStrip.Background = PurpleBrush;
                ColorInfo.Fill = PurpleBrush;
                InfoContainer.Background = PurpleTBrush;
                currentBrush = PurpleBrush;
                ShowStars(5, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 224, 187, 228)));
            }
            else if (_currentItem.GetFileName().StartsWith("++++"))
            {
                // Show animation if color changes
                if (currentBrush != RedBrush)
                {
                    MenuStrip.BeginAnimation(HeightProperty, colorTransitionAnim);
                }

                MenuStrip.Background = RedBrush;
                ColorInfo.Fill = RedBrush;
                InfoContainer.Background = RedTBrush;
                currentBrush = RedBrush;
                ShowStars(4, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 179, 186)));
            }
            else if (_currentItem.GetFileName().StartsWith("+++"))
            {
                // Show animation if color changes
                if (currentBrush != BlueBrush) {
                    MenuStrip.BeginAnimation(HeightProperty, colorTransitionAnim);
                }

                MenuStrip.Background = BlueBrush;
                ColorInfo.Fill = BlueBrush;
                InfoContainer.Background = BlueTBrush;
                currentBrush = BlueBrush;
                ShowStars(3, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 186, 225, 255)));
            }
            else if (_currentItem.GetFileName().StartsWith("++"))
            {
                // Show animation if color changes
                if (currentBrush != YellowBrush)
                {
                    MenuStrip.BeginAnimation(HeightProperty, colorTransitionAnim);
                }

                MenuStrip.Background = YellowBrush;
                ColorInfo.Fill = YellowBrush;
                InfoContainer.Background = YellowTBrush;
                currentBrush = YellowBrush;
                ShowStars(2, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 186)));
            }
            else if (_currentItem.GetFileName().StartsWith("+"))
            {
                // Show animation if color changes
                if (currentBrush != GreenBrush)
                {
                    MenuStrip.BeginAnimation(HeightProperty, colorTransitionAnim);
                }

                MenuStrip.Background = GreenBrush;
                ColorInfo.Fill = GreenBrush;
                InfoContainer.Background = GreenTBrush;
                currentBrush = GreenBrush;
                ShowStars(1, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 186, 255, 201)));
            }



            else if (_currentItem.IsNew())
            {
                ColorInfo.Fill = new SolidColorBrush(Colors.LawnGreen);
                InfoContainer.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 124, 252, 0));
                ShowStars(0, new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 124, 252, 0)));
            }
            else
            {
                MenuStrip.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(0, 0, 0));
                ColorInfo.Fill = new SolidColorBrush(Colors.Gray);
                currentBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 0, 0));
                InfoContainer.Background = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 128, 128, 128));
                ShowStars(0, new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255)));
            }

            // Makes all irrelevant elements invisible
            MakeTypeVisible(_currentItem.GetTypeOfFile());
            string currentFileType = _currentItem.GetTypeOfFile();

            try
            {
                // Preloads images ahead of time
                AddToCache();

                // Gets and show the content
                switch (currentFileType)
                {
                    case "image":
                        imageViewer.Source = ((ImageItem) _currentItem).GetImage();
                        break;
                    case "gif":
                        gifViewer.Source = ((GifItem) _currentItem).GetGif(gifViewer);
                        break;
                    case "video":
                        imageViewer.Source = ((VideoItem) _currentItem).GetThumbnail();
                        break;
                    case "text":
                        textViewer.Text = ((TextItem) _currentItem).GetText();
                        break;
                    case "file":
                        iconViewer.Source = (_currentItem).GetThumbnail();
                        break;
                }
            }
            catch (System.OutOfMemoryException)
            {
                Console.WriteLine("Out of memory!");
                _displayItems.Clear();
                _movedItems.Clear();
                isInCache.Clear();
                GC.Collect();
                RemoveOldContext();
            }
            catch 
            {
                // If content can't get loaded, show a blank black screen               
                MakeTypeVisible("");
                // TODO Eventual default view for images that have failed to load
            }

            _loadedOffset = 0;

            // File may load before WebP conversion finishes, so refresh after every conversion
            if (currentFileType == "image" && ((ImageItem) _currentItem).wasConverted)
            {
                try
                {
                    System.Threading.Thread.Sleep(200);
                    Refresh();
                    ((ImageItem)_currentItem).wasConverted = false;
                }
                catch
                {
                    // TODO Eventual default view for images that have failed to load
                }
            }

            PreviewContent();

            ResetView();
            UpdateSettingsChanged();

            imageViewerSize = (int) imageViewer.ActualHeight;

            if (_changed)
            {
                CreateSortMenu();
                _changed = false;
            }
        }

        private void Refresh()
        {
            _currentItem.RemovePreloadedContent();
            _currentItem.PreloadContent();
            imageViewer.Source = ((ImageItem)_currentItem).GetImage();
        }

        private void RefreshAll()
        {
            try
            {
                int currentIndex = _displayedItemIndex;
                string[] openFolder = { lastFolder };
                string[] originalFolder = { _originFolder.GetFolderPath() };

                AddNewFolder(originalFolder);

                try
                {
                    CreateNewContext(openFolder);
                    _displayedItemIndex = currentIndex;
                    UpdateContent();
                }
                catch
                {
                    CreateNewContext(originalFolder);
                    _displayedItemIndex = 0;
                    UpdateContent();
                }
            }
            catch
            {
                Console.WriteLine("Nothing loaded yet!");
            }
        }

        // Preview content in front or back
        private void PreviewContent()
        {
            int firstPreview = -((_previewContainer.Count - 1) / 2);

            for (var index = 0; index < _previewContainer.Count; index++)
            {
                Image item = _previewContainer[index];

                // Don't display images out of bounds
                if (_displayedItemIndex + index + firstPreview > _displayItems.Count - 1)
                {
                    item.Source = null;
                    continue;
                }
                if (_displayedItemIndex + index + firstPreview < 0)
                {
                    item.Source = null;
                    continue;
                }

                var offsetItem = _displayItems[_displayedItemIndex + index + firstPreview];

                // Give the center image a border
                if (index + firstPreview == 0)
                {
                    Border parBorder = (Border)item.Parent;

                    parBorder.BorderBrush = (SolidColorBrush)(new BrushConverter().ConvertFrom("#b01e76")); ;
                    parBorder.BorderThickness = new Thickness(2, 2, 2, 2);
                }

                // Sets the image to view
                item.Source = offsetItem.GetThumbnail();
//                item.Source = offsetItem.GetTypeOfFile() == "image"
//                    ? ((ImageItem)offsetItem).GetImage()
//                    : offsetItem.GetThumbnail();
            }
        }

        /// <summary>
        /// Zooms in or out of the current viewed image
        /// </summary>
        /// <param name="zoomAmount">The amount to zoom, where 0.5 is 50% and 2 is 200% zoomed in</param>
        public void Zoom(double zoomAmount)
        {
            // Only allow zooming in an image
            if (_currentItem.GetTypeOfFile() != "image") return;

            // Disallow zooming beyond the specified limits
            if (zoomAmount > 0 && _currentZoom + zoomAmount >= MaxZoom ||
                zoomAmount < 0 && _currentZoom + zoomAmount <= MinZoom) return;

            // Changes the current zoom level and updates the image
            _currentZoom += zoomAmount;
            _st.ScaleX = _currentZoom;
            _st.ScaleY = _currentZoom;
            imageViewer.RenderTransform = _imageTransformGroup;
        }



        // Adds all valid new files to a list
        private void ProcessNewFiles()
        {
            foreach (var item in NewFiles)
            {
                string fileType = FileType(item);
                switch (fileType)
                {
                    case "image":
                        _displayItems.Add(new ImageItem(item));
                        //Console.WriteLine(Marshal.SizeOf(_displayItems));
                        isInCache.Add(false);
                        break;
                    case "gif":
                        _displayItems.Add(new GifItem(item));
                        isInCache.Add(false);
                        break;
                    case "video":
                        _displayItems.Add(new VideoItem(item));
                        isInCache.Add(false);
                        break;
                    case "text":
                        _displayItems.Add(new TextItem(item));
                        isInCache.Add(false);
                        break;
                    case "file":
                        if (!_allowOtherFiles ||
                            File.GetAttributes(item).HasFlag(FileAttributes.Hidden)) continue;
                        _displayItems.Add(new FileItem(item));
                        isInCache.Add(false);
                        break;
                }
            }
            NewFiles.Clear();
        }

        // Recursively finds all files and subfolders in a folder
        private void FindFilesInSubfolders(string[] folder)
        {
            foreach (var s in folder)
            {
                if (Directory.Exists(s))
                {
                    if (_isDrop) InitializeDrop(s);

                    SearchOption scanFolderStructure =
                        _showSubDir ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

                    // Files to add
                    foreach (string foundFile in Directory.EnumerateFiles(s, "*.*", scanFolderStructure))
                    {
                        // Exlude folders started with an underscore
                        if (Path.GetDirectoryName(foundFile).Contains("[META]")) continue;
                        // Exclude special folders when set to do so
                        //if (!_showSets &&
                        //    _specialFolders.Any(o => Path.GetDirectoryName(foundFile).Contains(o.Key))) continue;

                        // If set, exclude showing files with the set prefix
                        if (!_showPrefix && foundFile.Contains(QuickPrefix)) continue;

                        /////// Filters

                        // If set to only show new files
                        switch (filter)    
                        {
                            case "none":
                                break;

                            case "++++++":
                                if (!foundFile.Contains("++++++")) continue;
                                break;

                            case "+++++":
                                if (!foundFile.Contains("+++++") || foundFile.Contains("++++++")) continue;
                                break;

                            case "++++":
                                if (!foundFile.Contains("++++") || foundFile.Contains("+++++")) continue;
                                break;

                            case "+++":
                                if (!foundFile.Contains("+++") || foundFile.Contains("++++")) continue;
                                break;

                            case "++":
                                if (!foundFile.Contains("++") || foundFile.Contains("+++")) continue;
                                break;

                            case "+":
                                if (!foundFile.Contains("+") || foundFile.Contains("++")) continue;
                                break;

                            // Add file if + does NOT exist
                            case "new":
                                if (foundFile.Contains("+")) continue;
                                break;

                            default:
                                break;
                        }

                        NewFiles.Add(foundFile);
                    }
                }
                else if (File.Exists(s))
                {
                    if (Path.GetDirectoryName(s).Contains("[META]")) continue;
                    if (_isDrop) InitializeDrop(Path.GetDirectoryName(s));

                    // Add filepath
                    NewFiles.Add(s);
                }
            }
            NewFiles.Sort();
        }

        // Resets specific settings when content is loaded from a drop as opposed to
        // loading an already defined subfolder
        private void InitializeDrop(string s)
        {
            // The folder highest in the tree
            _originFolder = new Folder(s);
            DisplayItem.RootFolder = Directory.GetParent(_originFolder.GetFolderPath()).ToString();

            _isDrop = false;
        }

        // Handler for drag-dropping files
        private void ControlWindow_Drop(object sender, DragEventArgs e)
        {
            string[] folder = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            AddNewFolder(folder);
        }

        // Initializes a new folder, removing the previous state
        public void AddNewFolder(string[] folder)
        {
            try
            {
                _displayedItemIndex = 0;
                DirectoryTreeList.Items.Clear();

                _originFolder?.GetAllFolders()?.Clear();
                _originFolder?.GetAllShownFolders()?.Clear();

                RemoveOldContext();
                _isDrop = true;
                CreateNewContext(folder);
                CreateSortMenu();
                
                AddFolderToSettings(folder[0]);
                RecentItems.Visibility = Visibility.Hidden;
            }
            catch
            {
                RemoveOldContext();
                Interaction.MsgBox("Couldn't load files");
            }
        }

        // Content-specific actions
        private void FocusContent()
        {
            // Sets focus on image and text files
            if (_currentItem.GetTypeOfFile() == "text" || _currentItem.GetTypeOfFile() == "image")
            {
                _isActive = !_isActive;
                textViewer.IsEnabled = _isActive;

                if (_isActive == false)
                {
                    ResetView();
                }
            }

            // Start video in default player
            else if (_currentItem.GetTypeOfFile() == "video" || _currentItem.GetTypeOfFile() == "file")
            {
                try
                {
                    Process.Start(_displayItems[_displayedItemIndex].GetFilePath());
                }
                catch
                {
                    Interaction.MsgBox("Failed to open file");
                }
            }
        }

        /// <summary>
        /// Resets the current image's zoom level and panning to default
        /// </summary>
        public void ResetView()
        {
            _tt.Y = 0.5;
            _tt.X = 0.5;
            _st.ScaleX = 1;
            _st.ScaleY = 1;
            _currentZoom = 1;
            imageViewer.RenderTransform = _imageTransformGroup;
            _isActive = false;
        }

        string lastFolder;

        // Loads a new folder
        private void CreateNewContext(string[] folder)
        {
            lastFolder = folder[0];

            _displayedItemIndex = 0;
            RemoveOldContext();
            FindFilesInSubfolders(folder);
            ProcessNewFiles();

            UpdateContent();

            // Update menubar
            foreach (Control item in ViewMenu.Items)
            {
                if (!(item is MenuItem)) continue;
                item.IsEnabled = true;
            }

            EditMenu.IsEnabled = true;
            OpenMenu.IsEnabled = true;

            if (Settings.Default.IsPreviewOpen)
            {
                PreviewField.Visibility = Visibility.Visible;
            }
        }

        // Removes an old folder
        private void RemoveOldContext()
        {
            _isActive = false;
            _isTyping = false;
            SortTypeBox.Visibility = Visibility.Hidden;

            imageViewer.Source = null;

            UpdateTitle();
            ResetView();

            _displayItems.Clear();
            _movedItems.Clear();
            isInCache.Clear();

            PreviewField.Visibility = Visibility.Hidden;

            if (_isEndless)
            {
                OpenEndlessView();
            }

            UpdateTitle();
            UpdateInfobar();

            foreach (Control item in ViewMenu.Items)
            {
                if (!(item is MenuItem) || item.Name == "FullscreenMenu" || item.Name == "IncludeSpecialMenu" ||
                    item.Name == "IncludeOtherFilesMenu" || item.Name == "IncludeSubMenu" || item.Name == "ShowSortMenuMenu" ||
                    item.Name == "IncludePrefixMenu") continue;
                item.IsEnabled = false;
            }

            EditMenu.IsEnabled = false;
            OpenMenu.IsEnabled = false;

            GC.Collect();
        }

        // Shows the folder sidebar on mouseover (if enabled)
        private void FolderGrid_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!Settings.Default.SortMode)
            {
                FolderGrid.Opacity = 1;
            }
        }

        private void FolderGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!Settings.Default.SortMode)
            {
                FolderGrid.Opacity = 0;
            }
        }


        private void UpdateFilter(string setfilter)
        {
            filter = setfilter;
            _displayedItemIndex = 0;
            RefreshAll();
        }

        string filter = "none";

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdateFilter("none");
        }


        private void RadioButton_Checked_0(object sender, RoutedEventArgs e)
        {
            UpdateFilter("++++++");
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            UpdateFilter("+++++");
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            UpdateFilter("++++");
        }

        private void RadioButton_Checked_3(object sender, RoutedEventArgs e)
        {
            UpdateFilter("+++");
        }

        private void RadioButton_Checked_4(object sender, RoutedEventArgs e)
        {
            UpdateFilter("++");
        }

        private void RadioButton_Checked_5(object sender, RoutedEventArgs e)
        {
            UpdateFilter("+");
        }

        private void RadioButton_Checked_6(object sender, RoutedEventArgs e)
        {
            UpdateFilter("new");
        }
    }

}

