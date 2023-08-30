using Id3;
using System;
using System.Text.RegularExpressions;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static Regex titleCheck = new Regex(@"(\S+) - (\d+) - ([a-zA-Z0-9 -_]+)");
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
                        Id3TagFamily family;
                        if (mp3.HasTagOfFamily(Id3TagFamily.Version2x))
                        {

                            tag = mp3.GetTag(Id3TagFamily.Version2x);
                            family = Id3TagFamily.Version2x;
                        }
                        else
                        {
                            tag = mp3.GetTag(Id3TagFamily.Version1x);
                            family = Id3TagFamily.Version1x;
                        }
                        track = int.Parse(tag.Track.Value.ToString()).ToString("D2");
                        title = tag.Title.ToString();
                        artist = tag.Artists.ToString();
                        album = tag.Album.ToString();
                        
                        if (int.Parse(track) < 1)
                        {
                            if (titleCheck.IsMatch(title))
                            {
                                var matches = titleCheck.Match(title);
                                title = matches.Groups[3].Value.ToString();
                                track = matches.Groups[2].Value.ToString();
                            }
                            //tag = mp3.GetTag(Id3TagFamily.Version1x);
                            //track = int.Parse(tag.Track.Value.ToString()).ToString("D2");
                        }
                        process = true;
                    }
                    Console.WriteLine("Track : {0}", track);
                    Console.WriteLine("Title: {0}", title);
                    Console.WriteLine("Artist: {0}", artist);
                    Console.WriteLine("Album: {0}", album);
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
                if (System.IO.File.Exists(destFName))
                {
                    Console.WriteLine($"File {destFName} already exists.");
                }
                else
                {
                    File.Move(file, destFName);
                    Console.WriteLine(destFName);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}