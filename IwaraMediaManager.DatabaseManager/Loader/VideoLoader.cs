using IwaraMediaManager.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IwaraMediaManager.DatabaseManager.Loader
{
    public class VideoLoader
    {
        public Video GetVideoFromDirectory(FileInfo fileInfo)
        {          
            Video video = new Video();

            video.FilePath = fileInfo.FullName;
            video.Name = fileInfo.Name.Replace(fileInfo.Extension, String.Empty);
            video.FileCreated = fileInfo.CreationTime;

            return video;
        }

        public async Task SetVideo(Video video)
        {
            using (var db = new DataBaseContext())
            {
                var existingVideo = db.Videos.FirstOrDefault(x => x.Name == video.Name && x.Artist == video.Artist);

                if (existingVideo == null)
                    db.Videos.Add(video);
                else
                {
                    existingVideo.IsFavorite = video.IsFavorite;
                    existingVideo.Name = video.Name;
                    existingVideo.UrlIwara = video.UrlIwara;
                }
                
                await db.SaveChangesAsync();
            }
        }
    }
}
