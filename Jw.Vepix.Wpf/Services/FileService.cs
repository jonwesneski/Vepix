﻿using Jw.Vepix.Common;
using Jw.Vepix.Data;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace Jw.Vepix.Wpf.Services
{
    public class FileService : IFileService
    {
        public async Task<List<FileBytes>> GetFilesBytesAsync(List<string> fileNames)
        {
            //todo maybe use parallel foreach here
            var bytes = new List<FileBytes>();
            await Task.Factory.StartNew(() =>
                fileNames.ForEach(async file => bytes.Add(new FileBytes(file,
                                            await GetFileBytesAsync(file)))));

            return bytes;
        }

        public async Task<List<string>> GetFileNamesFromDirectoryAsync(string folderPath,
            List<string> searchPattern, SearchOption option)
        {
            var files = new List<string>();
            if (Directory.Exists(folderPath))
            {
                await Task.Factory.StartNew(() =>
                    searchPattern.ForEach(sp =>
                       files.AddRange(Directory.GetFiles(folderPath, sp, option))));
            }

            return files;
        }

        public byte[] GetFileBytes(string fileName) => File.ReadAllBytes(fileName);

        public Task<byte[]> GetFileBytesAsync(string fileName) =>
            Task.Factory.StartNew(() => File.ReadAllBytes(fileName));

        public async Task<List<FileBytes>> GetFilesBytesFromDirectoryAsync(string folderPath,
            List<string> searchPattern, SearchOption option)
        {
            var files = new List<string>();
            if (Directory.Exists(folderPath))
            {
                await Task.Factory.StartNew(() =>
                    searchPattern.ForEach(sp =>
                       files.AddRange(Directory.GetFiles(folderPath, sp, option))));
            }

            return await GetFilesBytesAsync(files);
        }

        public bool ChangeFileName(string oldName, string newName)
        {
            File.Move(oldName, newName);
            return true;
        }

        public bool DeleteFile(string fileName)
        {
            File.Delete(fileName);
            return true;
        }

        public bool IsValidFileName(string fileName)
        {
            string invalidCharacters = "[" + Regex.Escape(new string(Path.GetInvalidPathChars())) + "]";
            return !(new Regex(invalidCharacters).IsMatch(fileName));
        }

        public bool OverwriteImage(BitmapImage bitmapImage, string fullFileName,
            BitmapEncoderType encoderType)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.FileName = fullFileName;
                var encoder = BitmapService.CreateBitmapEncoder(encoderType);
                encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                using (var fileStream = (FileStream)saveDialog.OpenFile())
                {
                    encoder.Save(fileStream);
                }
            }
            return true;
        }

        public bool SaveImageAs(BitmapImage bitmapImage, BitmapEncoderType encoderType)
        {
            using (var saveDialog = new SaveFileDialog())
            {
                saveDialog.Title = "Vepix: Save Image As...";
                saveDialog.Filter = "Jpeg Image|*.jpg|Bitmap Image|*.bmp|Gif Image|*.gif";
                saveDialog.ShowDialog();
                if (saveDialog.FileName != null && saveDialog.FileName != "")
                {
                    var encoder = BitmapService.CreateBitmapEncoder(encoderType);
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    using (var fileStream = (FileStream)saveDialog.OpenFile())
                    {
                        encoder.Save(fileStream);
                    }
                }
            }

            return true;
        }
    }
}
