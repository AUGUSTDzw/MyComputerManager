﻿using MyComputerManager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Interfaces;
using System.Drawing.Structs;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace System.Drawing
{
    public static class IconHelper
    {
        public static Icon ExtractIcon(string filePath, IconSize size)
        {
            Icon icon = null;
            var fileInfo = new SHFILEINFOW();

            if (NativeMethods.SHGetFileInfoW(filePath, NativeMethods.FILE_ATTRIBUTE_NORMAL, ref fileInfo, Marshal.SizeOf(fileInfo), NativeMethods.SHGFI_SYSICONINDEX) == 0)
                throw new FileNotFoundException();

            var iidImageList = new Guid("46EB5926-582E-4017-9FDF-E8998DAA0950");
            IImageList imageList = null;

            NativeMethods.SHGetImageList((int)size, ref iidImageList, ref imageList);

            if (imageList != null)
            {
                var hIcon = IntPtr.Zero;
                //imageList.GetIcon(fileInfo.iIcon, (int)NativeMethods.ILD_TRANSPARENT, ref hIcon);
                imageList.GetIcon(fileInfo.iIcon, (int)NativeMethods.ILD_IMAGE, ref hIcon);
                icon = Icon.FromHandle(hIcon).Clone() as Icon;

                NativeMethods.DestroyIcon(hIcon);

                if (icon.ToBitmap().PixelFormat != Imaging.PixelFormat.Format32bppArgb)
                {
                    icon.Dispose();

                    imageList.GetIcon(fileInfo.iIcon, (int)NativeMethods.ILD_TRANSPARENT, ref hIcon);
                    icon = Icon.FromHandle(hIcon).Clone() as Icon;

                    NativeMethods.DestroyIcon(hIcon);
                }
            }

            return icon;
        }

        public static ImageSource ReadIcon(string iconPath)
        {
            if (File.Exists(iconPath))
            {
                var f = new FileInfo(iconPath);
                if (f.Extension.ToLower() == ".ico")
                {
                    Stream iconStream = new FileStream(iconPath, FileMode.Open, FileAccess.Read);
                    IconBitmapDecoder decoder = new IconBitmapDecoder(
                            iconStream,
                            BitmapCreateOptions.PreservePixelFormat,
                            BitmapCacheOption.None);
                    var reslist = decoder.Frames.ToList();

                    if (reslist.Count > 0)
                    {
                        return reslist.OrderBy(x => (x.Format == PixelFormats.Bgra32 ? 1 : 2)).ThenByDescending(x => x.PixelHeight).First();
                    }
                }
                else if (f.Extension.ToLower() == ".exe")
                {
                    Icon i = IconHelper.ExtractIcon(iconPath, IconSize.ExtraLarge);
                    return BitmapHelper.ToBitmapSource(i.ToBitmap());
                }
            }
            return null;
        }
    }
}
