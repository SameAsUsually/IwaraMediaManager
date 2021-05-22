using System;
using System.Collections.Generic;
using System.Text;

namespace IwaraMediaManager.Models 
{ 
    public class Artist
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FolderPath { get; set; }
        public string Url { get; set; }

        public virtual List<Video> Videos { get; set; } = new List<Video>();
    }
}
