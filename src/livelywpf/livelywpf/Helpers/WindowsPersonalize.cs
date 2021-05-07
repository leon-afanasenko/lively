﻿using ImageMagick;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;

namespace livelywpf.Helpers
{
    class WindowsPersonalize
    {
        public static Color GetAccentColor()
        {
            var uiSettings = new UISettings();
            var accentColor = uiSettings.GetColorValue(UIColorType.Accent);
            return Color.FromArgb(accentColor.A, accentColor.R, accentColor.G, accentColor.B);
        }

        public static void SetAccentColor(Color color)
        {
            throw new NotImplementedException();
        }

        public static string GetLockScreenWallpaper()
        {
            throw new NotImplementedException();
        }

        public static async Task SetLockScreenWallpaper(string imgPath)
        {
            var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(imgPath);
            await Windows.System.UserProfile.LockScreen.SetImageFileAsync(file);
        }


        public static string GetDesktopWallpaper()
        {
            throw new NotImplementedException();
        }

        //todo: look into multiple monitor support, ref: https://stackoverflow.com/questions/1540337/how-to-set-multiple-desktop-backgrounds-dual-monitor
        public static async Task<bool> SetDesktopWallpaper(string imgPath)
        {
            bool result = false;
            if (Windows.System.UserProfile.UserProfilePersonalizationSettings.IsSupported())
            {
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(imgPath);
                result = await Windows.System.UserProfile.UserProfilePersonalizationSettings.Current.TrySetWallpaperImageAsync(file);
            }
            return result;
        }

        public static void SetSystemTheme(SystemTheme theme, SystemThemeType type)
        {
            throw new NotImplementedException();
        }

        public static SystemTheme GetSystemTheme(SystemThemeType type)
        {
            throw new NotImplementedException();
        }

        #region enums

        public enum SystemTheme
        {
            Light,
            Dark
        }

        public enum SystemThemeType
        {
            app,
            system
        }

        #endregion //enums
    }
}