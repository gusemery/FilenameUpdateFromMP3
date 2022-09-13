using Id3;
using System;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static string srcRoot;
        static string destRoot;
        static int fileCount = 0;
        static void Main(string[] args)
        {
            if (args.Length != 2 || string.IsNullOrEmpty(args[0]) || string.IsNullOrEmpty(args[1]))
            {
                Console.WriteLine("Source and destingation directory must be specified.");
                Console.ReadKey();
                return;
            }
            srcRoot = args[0];
            destRoot = args[1];

            ProcessDirectory(srcRoot);

            Console.WriteLine("Finished");
            Console.ReadKey();
        }

        static void ProcessDirectory(string src)
        {
            var dirs = Directory.GetDirectories(src);
            foreach (var dir in dirs)
            {
                var children = Directory.GetDirectories(dir);
                if (children.Length > 0)
                {
                    ProcessDirectory(dir);
                }
                UpdateFiles(dir);
            }
            UpdateFiles(src);
        }

        private static void UpdateFiles(string dir)
        {
            var files = Directory.GetFiles(dir);

            foreach (string file in files)
            {
                bool process = false;
                string track = String.Empty;
                string title = String.Empty;
                string artist = String.Empty;
                string album = String.Empty;
                using (var mp3 = new Mp3File(file))
                {
                    if (mp3.HasTags)
                    {
                        Id3Tag tag;
                        if (mp3.AvailableTagVersions.All(ver => ver.Equals(Id3TagFamily.Version2x)))
                        {

                            tag = mp3.GetTag(Id3TagFamily.Version2x);

                        }
                        else
                        {
                            tag = mp3.GetTag(Id3TagFamily.Version1x);
                        }
                        track = int.Parse(tag.Track.Value.ToString()).ToString("D2");
                        title = tag.Title.ToString();
                        artist = tag.Artists.ToString();
                        album = tag.Album.ToString();
                        process = true;
                    }
                    //Console.WriteLine("Track : {0}", tag.Track.ToString());
                    //Console.WriteLine("Title: {0}", tag.Title.Value);
                    //Console.WriteLine("Artist: {0}", tag.Artists.ToString());
                    //Console.WriteLine("Album: {0}", tag.Album.Value);
                }
                    if (process)
                        RenameFile(file, track, title, artist, album);
                
            }
        }

        private static void RenameFile(string file, string track, string title, string artist, string album)
        {
            if (String.IsNullOrEmpty(album))
                album = "unknown";
            if (String.IsNullOrEmpty(artist))
                artist = "unknown";
            if (string.IsNullOrEmpty(title))
                title = "unknown";

            var postFix = Path.GetExtension(file);
            //var path = Path.GetDirectoryName(file);
            var dir = Path.GetFullPath(destRoot);
            try
            {
                if (!Directory.Exists($"{dir}\\{artist}"))
                {
                    Directory.CreateDirectory($"{dir}\\{artist}");
                }
                if (!Directory.Exists($"{dir}\\{artist}\\{album}"))
                {
                    Directory.CreateDirectory($"{dir}\\{artist}\\{album}");
                }
                string destFName = string.Format($"{dir}\\{artist}\\{album}\\{track}-{title}{postFix}");
                File.Move(file, destFName);
                Console.WriteLine(destFName);
            }catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}