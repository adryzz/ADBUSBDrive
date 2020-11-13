using DokanNet;
using Serilog;
using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ADBUSBDrive
{
    static class DriveHelpers
    {
        public static char GetNextAvailableDriveLetter()
        {
            List<char> letters = "EFGHIJKLMNOPQRSTUVXYZ".ToCharArray().ToList();
            foreach(DriveInfo i in  DriveInfo.GetDrives())
            {
                char letter = i.Name.ToUpper().ToCharArray().First();
                letters.Remove(letter);
            }

            return letters.First();
        }

        public static string ConvertToUnixPath(string path)
        {
            return path.Replace('\\', '/');
        }

        public static string ConvertToWindowsPath(string path)
        {
            return path.Replace('/', '\\');
        }

        public static FileInformation ConvertToWindowsFile(FileStatistics file)
        {
            FileInformation info = new FileInformation();
            info.FileName = ConvertToWindowsPath(file.Path);
            FileAttributes attributes = FileAttributes.Normal;
            if (file.Path.ToCharArray()[file.Path.LastIndexOf('/') + 1] == '.')
            {
                attributes |= FileAttributes.Hidden;
            }
            if (file.FileMode == UnixFileMode.Directory)
            {
                attributes |= FileAttributes.Directory;
            }
            else if (file.FileMode != UnixFileMode.Regular)
            {
                attributes |= FileAttributes.Device;
            }
            info.CreationTime = file.Time.DateTime;
            return info;
        }

        public static IEnumerable<FileInformation> ConvertToWindowsFile(IEnumerable<FileStatistics> file)
        {
            List<FileInformation> infos = new List<FileInformation>();
            foreach(FileStatistics s in file)
            {
                infos.Add(ConvertToWindowsFile(s));
            }
            return infos;
        }

        public static FileStatistics ConvertToUnixFile(FileInformation file)
        {
            FileStatistics s = new FileStatistics();
            s.Path = ConvertToUnixPath(file.FileName);
            return s;
        }

        public static IEnumerable<FileStatistics> ConvertToUnixFile(IEnumerable<FileInformation> file)
        {
            return null;
        }
    }
}
