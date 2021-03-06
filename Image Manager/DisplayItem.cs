﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DirectShowLib;
using DirectShowLib.DES;
using WpfAnimatedGif;
using Image = System.Windows.Controls.Image;

namespace Image_Manager
{

    public static class FlyWeightPointer
    {
        public static LibwebpSharp.WebPDecoder _dec;

        static FlyWeightPointer()
        {
            _dec = new LibwebpSharp.WebPDecoder();
        }

        public static string SizeSuffix(long value)
        {
            string[] sizeSuffixes =
                { "bytes", "KB", "MB", "GB" };

            int i = 0;
            decimal dValue = value;
            while (Math.Round(dValue / 1024) >= 1)
            {
                dValue /= 1024;
                i++;
            }

            return $"{dValue:n1} {sizeSuffixes[i]}".Replace(",", ".");
        }


        public static BitmapImage LoadImage(string FileExtension, string myImageFile)
        {
            BitmapImage image = new BitmapImage();

            // Convert to Bitmap if image is of WebP format
            if (FileExtension != ".webp")
                using (FileStream stream = File.OpenRead(myImageFile))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.DecodePixelHeight = 150;
                    image.EndInit();
                }
            else
            {
                // Allows for loading WebP images through an included library
                image = BitmapToImageSource(_dec.DecodeBGRA(myImageFile));
            }

            return image;
        }


        /// <summary>
        /// Credits to Gerret over at StackOverflow for the following method
        /// https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
        /// 
        /// Converts a Bitmap to an easily viewable BitmapImage.
        /// </summary>
        /// <param name="bitmap">The supplied Bitmap to convert.</param>
        /// <returns></returns>
        public static BitmapImage BitmapToImageSource(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();

                bitmapimage.BeginInit();
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.StreamSource = memory;
                //bitmapimage.DecodePixelHeight = 150;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static BitmapImage LoadImageFullRes(string FileExtension, string myImageFile)
        {
            // Convert to Bitmap if image is of WebP format
            if (FileExtension != ".webp")
                using (FileStream stream = File.OpenRead(myImageFile))
                {
                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    // Rescales the image resolution if set
                    if (MainWindow._rescale) image.DecodePixelHeight = MainWindow.imageViewerSize;
                    image.EndInit();
                    return image;
                }

            // Allows for loading WebP images through an included library
            return BitmapToImageSourceFullRes(_dec.DecodeBGRA(myImageFile));
        }


        /// <summary>
        /// Credits to Gerret over at StackOverflow for the following method
        /// https://stackoverflow.com/questions/22499407/how-to-display-a-bitmap-in-a-wpf-image
        /// 
        /// Converts a Bitmap to an easily viewable BitmapImage.
        /// </summary>
        /// <param name="bitmap">The supplied Bitmap to convert.</param>
        /// <returns></returns>
        public static BitmapImage BitmapToImageSourceFullRes(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();

                bitmapimage.BeginInit();
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.StreamSource = memory;
                if (MainWindow._rescale) bitmapimage.DecodePixelHeight = MainWindow.imageViewerSize;
                bitmapimage.EndInit();

                return bitmapimage;
            }
        }

        public static BitmapImage LoadThumbnail(string myThumbnail)
        {
            // The desired resolution
            const int THUMB_SIZE = 150;

            Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(
                myThumbnail, THUMB_SIZE, THUMB_SIZE, ThumbnailOptions.BiggerSizeOk);

            BitmapImage thumbnailImage = BitmapToImageSource(thumbnail);
            thumbnail.Dispose();

            return thumbnailImage;
        }
    }

    /// <summary>
    /// Base class inherited by all viewable types of files.
    /// Contains basic information that all files have.
    /// </summary>
    public abstract class DisplayItem
    {
        public static int ShortLength;

        protected string FilePath;
        protected string PermFilePath;

        protected string FileName;
        protected string ShortenedName;
        protected string FileNameExludingExtension;
        protected string FileExtension;
        protected string FileType;
        protected string FileLocation;
        protected string LocalLocation;
        protected string FileSize;
        protected string InfoBarDefaultContent;
        protected string InfoBarDefaultContentExtra;
        protected bool isNew;

        public bool HasBeenDeleted = false;

        public static string RootFolder;

        /// <summary>
        /// Constructor that sets the values of each item's basic information.
        /// </summary>
        /// <param name="name">The path to this file</param>
        protected DisplayItem(string name)
        {
            FilePath = PermFilePath = name;

            FileName = Path.GetFileName(name);
            FileNameExludingExtension = Path.GetFileNameWithoutExtension(name);
            FileExtension = Path.GetExtension(name).ToLower();
            FileLocation = Path.GetDirectoryName(name);
            FileSize = GetFileSize(name);

            ShortenedName = FileName.Length > ShortLength ?
                FileName.Substring(0, ShortLength) + "..." : FileName;


            // Sets the relative location to the initial rootfolder.
            LocalLocation = FileLocation.Replace(RootFolder, "").TrimStart('\\');
            //InfoBarDefaultContent = FileName + "    -    " + LocalLocation + "    -    " + FileSize;
            InfoBarDefaultContent = $"{MainWindow.Truncate(FileName, 35),-35}" +
                                    $"{LocalLocation}";
            InfoBarDefaultContentExtra = FileSize;
        }

        /// <summary>
        /// Gets what type of file this is, set in each inherited class.
        /// </summary>
        /// <returns>A string describing the nature of this file.</returns>
        public string GetTypeOfFile()
        {
            return FileType;
        }

        public bool IsNew()
        {
            return isNew;
        }

        public void SetIsNew()
        {
            isNew = true;
        }

        /// <summary>
        /// Gets the filename without its extension.
        /// </summary>
        /// <returns>The filename sans extension.</returns>
        public string GetFileNameExcludingExtension()
        {
            return FileNameExludingExtension;
        }

        /// <summary>
        /// Gets the filename.
        /// </summary>
        /// <returns>This object's filename.</returns>
        public string GetFileName()
        {
            return FileName;
        }

        /// <summary>
        /// Gets the entire path to this file including its filename.
        /// </summary>
        /// <returns>Gets the path to this file.</returns>
        public string GetFilePath()
        {
            return FilePath;
        }

        /// <summary>
        /// Gets the initial filepath, allowing for moved files to be moved back to its first location.
        /// </summary>
        /// <returns>The initial filepath.</returns>
        public string GetOldFilePath()
        {
            return PermFilePath;
        }

        /// <summary>
        /// Changes the data of this file in case it has moved or been renamed. 
        /// GetOldFilePath remains unchanged.
        /// </summary>
        /// <param name="newPath">Location of this object's new path and name.</param>
        public void SetFilePath(string newPath)
        {
            FilePath = newPath;
            FileLocation = Path.GetDirectoryName(newPath);

            FileName = Path.GetFileName(newPath);
            FileNameExludingExtension = Path.GetFileNameWithoutExtension(newPath);
            FileExtension = Path.GetExtension(newPath).ToLower();
            FileLocation = Path.GetDirectoryName(newPath);


            InfoBarDefaultContent = $"{MainWindow.Truncate(FileName, 35),-35}" +
                                    $"{LocalLocation}";
            InfoBarDefaultContentExtra = FileSize;
        }

        /// <summary>
        /// Gets the file extension.
        /// </summary>
        /// <returns>The file's extension.</returns>
        public string GetFileExtension()
        {
            return FileExtension;
        }

        /// <summary>
        /// Gets the location of this file, excluding its name.
        /// </summary>
        /// <returns>The path to the folder this file is located in.</returns>
        public string GetLocation()
        {
            return FileLocation;
        }

        /// <summary>
        /// Allows certain files to be preloaded.
        /// </summary>
        public virtual void PreloadContent()
        {
            _thumbnailSource = FlyWeightPointer.LoadThumbnail(FilePath);
        }

        /// <summary>
        /// Allows preloaded content to be dropped.
        /// </summary>
        public virtual void RemovePreloadedContent()
        {
            _thumbnailSource = null;
        }

        /// <summary>
        /// Gets the content that should be displayed in the infobar.
        /// The default is the filename followed by its location.
        /// </summary>
        /// <returns></returns>
        public virtual string GetInfobarContent()
        {
            return InfoBarDefaultContent;
        }

        public virtual string GetInfobarContentExtra()
        {
            return InfoBarDefaultContentExtra;
        }

        /// <summary>
        /// Gets the filepath of this object.
        /// </summary>
        /// <returns>The file complete path.</returns>
        public override string ToString()
        {
            return FilePath;
        }


        public string GetFileSize(string path)
        {
            return FlyWeightPointer.SizeSuffix(new FileInfo(path).Length);
        }

        
        protected BitmapImage _thumbnailSource;

        

        /// <summary>
        /// Loads the thumbnail via an external library and saves it as
        /// a BitmapImage.
        /// </summary>
        /// <param name="myThumbnail">Path to the video file.</param>
        /// <returns>The thumbnail image.</returns>
        public BitmapImage GetThumbnail()
        {
            return _thumbnailSource;
        }
    }


    /// <summary>
    /// Generic item file, containing only a thumbnail.
    /// </summary>
    public class FileItem : DisplayItem
    {
        public FileItem(string name) : base(name)
        {
            FileType = "file";
        }
    }

    /// <inheritdoc/>
    /// <summary>
    /// Contains an image and its metadata.
    /// </summary>
    public class ImageItem : DisplayItem
    {
        private BitmapImage _imageSource;
        private int _imageHeight;
        private int _imageWidth;
        public bool wasConverted;

        

        public ImageItem(string name) : base(name)
        {
            FileType = "image";
            
        }

        public override string GetInfobarContentExtra()
        {
            if (MainWindow._rescale)
            {
                return $"{InfoBarDefaultContentExtra,-10}";
            }
            else
            {
                return $"{InfoBarDefaultContentExtra,-10}{"( " + _imageWidth + " x " + _imageHeight + " )",-20}";
            }
        }

        public override void PreloadContent()
        {
            _thumbnailSource = FlyWeightPointer.LoadImage(FileExtension, FilePath);
        }

        public override void RemovePreloadedContent()
        {
            _thumbnailSource = null;
        }

        /// <summary>
        /// Gets the height of this image.
        /// </summary>
        /// <returns>Height of image in pixels.</returns>
        public int GetSize()
        {
            return _imageHeight;
        }

        /// <summary>
        /// Gets the image that has been preloaded.
        /// </summary>
        /// <returns>A BitmapImage.</returns>
        public BitmapImage GetImage()
        {
            return LoadImage(FilePath);
        }


        /// <summary>
        /// Loads the file and sets its source to this object,
        /// as long as it remains preloaded.
        /// </summary>
        /// <param name="myImageFile">The file path.</param>
        /// <returns>The completed image.</returns>
        private BitmapImage LoadImage(string myImageFile)
        {
            BitmapImage image = FlyWeightPointer.LoadImageFullRes(FileExtension, myImageFile);

            // If the image has been loaded before, it won't have to
            // get the metadata of the image again.
            if (_imageHeight != 0 || _imageWidth != 0) return image;

            _imageHeight = image.PixelHeight;
            _imageWidth = image.PixelWidth;
            return image;
        }
    }


    /// <summary>
    /// Contains a gif file.
    /// </summary>
    public class GifItem : DisplayItem
    {
        public static BitmapImage _gifImage;

        public GifItem(string name) : base(name)
        {
            FileType = "gif";
        }

        /// <summary>
        /// Gets the gif by loading it into memory.
        /// </summary>
        /// <param name="viewer">The element that will display the gif file.</param>
        /// <returns>The ready-made image file.</returns>
        public BitmapImage GetGif(Image viewer)
        {
            return LoadGif(FilePath, viewer);
        }

        public override void RemovePreloadedContent()
        {
            _gifImage = null;
        }

        /// <summary>
        /// Due to limitations of WPF, a gif file can't be stored
        /// into memory in an animated state, meaning the file will
        /// have to be redrawn each time it is loaded and thus
        /// cannot be preloaded either. There may be a not-yet-found
        /// workaround to this problem.
        /// </summary>
        /// <param name="myGifFile">The path to the file.</param>
        /// <param name="viewer">The element that will display the gif file.</param>
        /// <returns></returns>
        public BitmapImage LoadGif(string myGifFile, Image viewer)
        {
            _gifImage = new BitmapImage();
            using (FileStream stream = File.OpenRead(myGifFile))
            {
                _gifImage.BeginInit();
                _gifImage.CacheOption = BitmapCacheOption.OnLoad;
                _gifImage.StreamSource = stream;
                ImageBehavior.SetAnimatedSource(viewer, _gifImage);
                _gifImage.EndInit();
            }

            return _gifImage;
        }
    }


    /// <summary>
    /// A video object containing a thumbnail as well as
    /// various metadata.
    /// </summary>
    public class VideoItem : FileItem
    {
        private int _videoResolutionWidth;
        private int _videoResolutionHeight;
        private string _videoLength;


        public VideoItem(string name) : base(name)
        {
            FileType = "video";
        }

        public override string GetInfobarContentExtra()
        {
            return $"{InfoBarDefaultContentExtra,-10}" +
                   $"{"( " + _videoResolutionWidth + " x " + _videoResolutionHeight + " )",-17}" +
                   $"{"( " + _videoLength + " )",-20}";
        }

        public override void PreloadContent()
        {
            // If the values returned are nonsensical, retry 3 times until 
            // correct values are found.
            /*
            for (int tries = 0; tries < 4; tries++)
            {
                if (_videoResolutionHeight != 0 && _videoResolutionWidth >= -100000 &&
                    _videoResolutionHeight >= -100000 && _videoResolutionWidth <= 100000 &&
                    _videoResolutionHeight <= 100000) continue;
                if ((_videoResolutionHeight == 0 ||
                     _videoResolutionWidth < -100000 || _videoResolutionHeight < -100000 ||
                     _videoResolutionWidth > 100000 || _videoResolutionHeight > 100000) &&
                    tries == 3)
                {
                    _videoResolutionHeight = 0;
                    _videoResolutionWidth = 0;
                    break;
                }

                GetMetaData();
            }*/

            _thumbnailSource = FlyWeightPointer.LoadThumbnail(FilePath);
        }

        public override void RemovePreloadedContent()
        {
            _thumbnailSource = null;
        }

        /// <summary>
        /// Gets the thumbnail as an image file.
        /// </summary>
        /// <returns>An image containing the thumbnail.</returns>
        public new BitmapImage GetThumbnail()
        {
            return _thumbnailSource;
        }

        /// <summary>
        /// Credits to nZeus over at StackOverflow for the following method (with edits)
        /// https://stackoverflow.com/questions/6215185/getting-length-of-video
        /// 
        /// 
        /// </summary>
        public void GetMetaData()
        {
            /*
            var mediaDet = (IMediaDet)new MediaDet();
            DsError.ThrowExceptionForHR(mediaDet.put_Filename(FilePath));

            // Retrieve some measurements from the video
            var mediaType = new AMMediaType();
            mediaDet.get_StreamMediaType(mediaType);
            var videoInfo = (VideoInfoHeader)Marshal.PtrToStructure(mediaType.formatPtr, typeof(VideoInfoHeader));
            DsUtils.FreeAMMediaType(mediaType);

            _videoResolutionWidth = videoInfo.BmiHeader.Width;
            _videoResolutionHeight = videoInfo.BmiHeader.Height;

            mediaDet.get_StreamLength(out double mediaLength);

            // Convert time into readable format
            var parts = new List<string>();

            void Add(int val, string unit)
            {
                if (val > 0) parts.Add(val + unit);
            }

            var t = TimeSpan.FromSeconds((int)mediaLength);

            Add(t.Days, "d");
            Add(t.Hours, "h");
            Add(t.Minutes, "m");
            Add(t.Seconds, "s");

            _videoLength = string.Join(" ", parts);

            mediaDet.put_Filename(null);
            */
        }
    }


    /// <summary>
    /// A text object displaying the contents of a text file.
    /// </summary>
    public class TextItem : DisplayItem
    {
        private string _wordAmount;
        private readonly string _textContent;


        public TextItem(string name) : base(name)
        {
            FileType = "text";
            _textContent = "\n\n" + File.ReadAllText(FilePath);
            CountWords();
        }

        public override string GetInfobarContentExtra()
        {
            return $"{InfoBarDefaultContentExtra,-10}" +
                   $"{"( " + _wordAmount + " words )",-20}";
        }

        /// <summary>
        /// Sets the number of words found in the file.
        /// </summary>
        public void CountWords()
        {
            StreamReader sr = new StreamReader(FilePath);

            int counter = 0;
            const string delim = " ,.!?";

            try
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    line?.Trim();
                    string[] fields = line.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    counter += fields.Length;
                }
            }
            catch
            {
                // If the file contains nothing, set the wordcount to zero.
                _wordAmount = counter.ToString();
            }

            sr.Close();

            _wordAmount = counter.ToString();
        }

        /// <summary>
        /// Gets the text content of the file.
        /// </summary>
        /// <returns>String containing all text</returns>
        public string GetText()
        {
            return _textContent;
        }
    }
}
