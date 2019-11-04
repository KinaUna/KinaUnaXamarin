﻿using SQLite;

namespace KinaUnaXamarin.Models.SQLite
{
    public class VideoViewModelDto
    {
        [PrimaryKey, AutoIncrement]
        public int DbId { get; set; }
        public int VideoId { get; set; }
        public string VideoViewModelString { get; set; }
    }
}
