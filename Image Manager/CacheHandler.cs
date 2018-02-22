﻿using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace Image_Manager
{/*
    internal class CacheHandler
    {
        private const int NUM_OF_CACHED_IMAGES = 15;
        public int lastPos = 0;

        public void UpdateCache()
        {
            bool isGoingRight = true;
            int currentImageNum = MainWindow.ReturnCurrentImageNum();
            
            // Find direction moved in gallery
            if (currentImageNum - lastPos < 0)
            {
                isGoingRight = false;
            }

            // Load images NUM_OF_CACHED_IMAGES steps
            if (isGoingRight)
            {
                for (int i = currentImageNum;
                    i < currentImageNum + NUM_OF_CACHED_IMAGES && i < MainWindow.filepaths.Count;
                    i++)
                {
                    AddCache(i);
                }
            }
            else
            {
                for (int i = currentImageNum - NUM_OF_CACHED_IMAGES + 1; i <= currentImageNum && i >= 0; i++)
                {
                    AddCache(i);
                }
            }

            DropCache();
        }

        public void AddCache(int i)
        {
            if (MainWindow.cache.ContainsKey(MainWindow.filepaths[i])) return;
            if (MainWindow.FileType(MainWindow.filepaths[i]) == "image")
            {
                BitmapImage imageToCache = LoadImage(MainWindow.filepaths[i]);
                MainWindow.cache.Add(MainWindow.filepaths[i], imageToCache);
            }
            else if (MainWindow.FileType(MainWindow.filepaths[i]) == "video")
            {
                // Grab thumbnail from video and cache it
                int THUMB_SIZE = 1024;
                Bitmap thumbnail = WindowsThumbnailProvider.GetThumbnail(
                    MainWindow.filepaths[i], THUMB_SIZE, THUMB_SIZE, ThumbnailOptions.BiggerSizeOk);

                //BitmapImage thumbnailImage = MainWindow.BitmapToImageSource(thumbnail);
                thumbnail.Dispose();

                //MainWindow.cache.Add(MainWindow.filepaths[i], thumbnailImage);

            }
        }

        public BitmapImage LoadImage(string myImageFile)
        {
            BitmapImage image = new BitmapImage();
            using (FileStream stream = File.OpenRead(myImageFile))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
            }
            BitmapImage retImage = image;
            return retImage;
        }

        public void DropCache()
        {
            int currentImageNum = MainWindow.ReturnCurrentImageNum();

            // Remove image N steps back
            if (currentImageNum - NUM_OF_CACHED_IMAGES >= 0 &&
                MainWindow.cache.ContainsKey(MainWindow.filepaths[currentImageNum - NUM_OF_CACHED_IMAGES]))
            {
                MainWindow.cache.Remove(MainWindow.filepaths[currentImageNum - NUM_OF_CACHED_IMAGES]);
            }

            // Remove image N steps forward
            if (currentImageNum + NUM_OF_CACHED_IMAGES + 1 < MainWindow.filepaths.Count && 
                MainWindow.cache.ContainsKey(MainWindow.filepaths[currentImageNum + NUM_OF_CACHED_IMAGES + 1]))
            {
                MainWindow.cache.Remove(MainWindow.filepaths[currentImageNum + NUM_OF_CACHED_IMAGES + 1]);
            }
        }

    }*/
}
