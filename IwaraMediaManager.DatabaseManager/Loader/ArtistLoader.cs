using IwaraMediaManager.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IwaraMediaManager.DatabaseManager.Loader
{
    public class ArtistLoader
    {
        private VideoLoader VideoLoader;

        public ArtistLoader()
        {
            VideoLoader = new VideoLoader();
        }

        public async Task<List<Artist>> GetArtists(string iwaraVideosFolder)
        {
            var artists = await GetArtistsFromDb();
            if (artists.Count == 0)
            {
                artists = GetArtistsFromDirectory(iwaraVideosFolder);

                // First time database inserts
                for (int i = 0; i < artists.Count; i++)
                    artists[i] = await SetArtistAsync(artists[i]);
            }

            return artists;
        }

        public async Task<List<Artist>> GetArtistsFromDb()
        {
            List<Artist> artists = new List<Artist>();
            using (var dbContext = new DatabaseManager.DataBaseContext())
            {
                var artistsDb = dbContext.Artists.Include(x => x.Videos);
                artists = await artistsDb.ToListAsync();
            }

            return artists;
        }

        public async Task<Artist> GetDbArtistByName(string name)
        {
            using (var dbContext = new DataBaseContext())
            {
                return await dbContext.Artists.FirstOrDefaultAsync(x => x.Name == name);
            }
        }

        public async Task<Artist> SetArtistAsync(Artist artist)
        {
            Artist existingArtist = null;

            using (var dbContext = new DatabaseManager.DataBaseContext())
            {
                existingArtist = dbContext.Artists.Where(x => x.Id == artist.Id || x.FolderPath == artist.FolderPath || x.Name == artist.Name).FirstOrDefault();

                if (existingArtist != null)
                {
                    existingArtist.Name = artist.Name;
                    existingArtist.Url = artist.Url;

                    // Add Videos to db Artist which are not yet in db
                    foreach (var video in artist.Videos.Where(x => x.Id == 0))
                    {
                        var existingVideo = existingArtist.Videos.FirstOrDefault(x => x.FilePath == video.FilePath && x.Id != 0);
                        if (existingVideo != null)
                        {
                            existingVideo.ThumbnailUrl = video.ThumbnailUrl;
                            existingVideo.UrlIwara = video.UrlIwara;
                        }
                        else
                        {
                            existingArtist.Videos.Add(video);
                        }
                    }
                }
                else
                    existingArtist = (await dbContext.AddAsync(artist)).Entity;

                await dbContext.SaveChangesAsync();
            }

            return existingArtist;
        }

        public Artist GetArtistFromDirectory(string folderPath)
        {
            var directory = new DirectoryInfo(folderPath);

            Artist artist = new Artist();
            artist.Name = directory.Name;
            artist.FolderPath = directory.FullName;

            var videoPaths = directory.GetFiles("*.mp4");
            var videos = videoPaths.Select(path => VideoLoader.GetVideoFromDirectory(path));

            artist.Videos.AddRange(videos);

            return artist;
        }

        public List<Artist> GetArtistsFromDirectory(string folderPath)
        {
            var artists = new List<Artist>();
            var artistFolders = Directory.GetDirectories(folderPath);

            foreach (string folder in artistFolders)
            {
                try
                {
                    artists.Add(GetArtistFromDirectory(folder));
                }
                catch
                {

                }
            }

            return artists;
        }
    }
}
