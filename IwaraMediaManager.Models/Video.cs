using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IwaraMediaManager.Models
{
    public class Video
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Artist Artist { get; set; }
        public bool IsFavorite { get; set; }
        public string FilePath { get; set; }
        public string ThumbnailUrl { get; set; }
        public string UrlIwara { get; set; }
        
        public DateTime FileCreated { get; set; }
        public DateTime VideoUploadedOnIwara { get; set; }

        public string DownloadUri { get; set; }
    }
}
