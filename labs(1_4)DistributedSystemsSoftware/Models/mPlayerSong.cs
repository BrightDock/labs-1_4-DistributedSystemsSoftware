using System;

namespace labs_1_4_DistributedSystemsSoftware.Models
{
    public class mPlayerSong
    {
        public string SongName { get; set; }
        public string SongDescription { get; set; }
        public short? SongYear { get; set; }
        public TimeSpan? SongLength { get; set; }
        public string SongPath { get; set; }
        public string AuthorName { get; set; }
        public string AuthorCountry { get; set; }
        public string AuthorDescription { get; set; }
        public string SongBckgImage { get; set; }
        public long SongId { get; set; }
    }
}