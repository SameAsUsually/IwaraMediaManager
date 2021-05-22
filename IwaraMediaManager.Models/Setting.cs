using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace IwaraMediaManager.Models
{
    public class Setting
    {
        public const string VIDEOPATH = "VideoPath";
        public const string ACCOUNT_NAME = "AccountName";
        public const string ACCOUNT_PASSWORD = "AccountPassword";
        public const string MAXVIDEOSPERPAGE = "MaxVideosPerPage";
        public const string THUMPNAILSIZE = "ThumpnailSize";
        public const string COLORTHEME = "ColorTheme";

        [Key]
        public string Key { get; set; }
        public string Value { get; set; }

        public Setting(string key, string value)
        {
            Key = key;
            Value = value;
        }
    }
}
