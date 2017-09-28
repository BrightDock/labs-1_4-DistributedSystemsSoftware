using System;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using labs_1_4_DistributedSystemsSoftware.Hubs;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Collections.Generic;
using Google.Apis.Services;
using labs_1_4_DistributedSystemsSoftware.Models;

namespace labs_1_4_DistributedSystemsSoftware.Controllers
{
    public class HomeController : Controller
    {

        labDB db = new labDB();

        public ActionResult Index(){
            return View();
        }

        public ActionResult pris() {

            return PartialView();
        }

        public ActionResult oedCS(){

            return PartialView();
        }

        public ActionResult kpp()
        {

            return PartialView();
        }

        public ActionResult getSongsList() {
            List<mPlayerSong> MPS = db.Songs.Join(db.Authors,
                song => song.Id_author,
                author => author.Id,
                (song, author) => new { Song = song, Author = author })
                .Join(db.Songs_descriptions,
                SA => SA.Song.Id,
                sD => sD.Id_song,
                (SA, sD) => new { SA.Author, SA.Song, SD = sD.Description }                
                )
                .Select( s => new mPlayerSong
                {
                    SongName = s.Song.Name,
                    SongDescription = s.SD,
                    SongYear = s.Song.Year,
                    SongLength = s.Song.Length,
                    SongPath = s.Song.Path_location,
                    AuthorName = s.Author.Name,
                    AuthorCountry = s.Author.Country,
                    AuthorDescription = s.Author.Description,
                    SongBckgImage = s.Song.Path_image,
                    SongId = s.Song.Id
                }).ToArray().Reverse().ToList();

            return PartialView("getSongsList", MPS);                  
        }

        private string GetGoogleSongImage(string searchString) {
            string result = string.Empty;

            return result;
        }

        public static object ChangeType(object value, Type conversion)
        {
            var t = conversion;

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                if (value == null)
                {
                    return null;
                }

                t = Nullable.GetUnderlyingType(t);
            }

            return Convert.ChangeType(value, t);
        }

        [HttpPost]
        public JsonResult DataUpload() {
            var file = @Request.Files["file"];
            string filePath;
            var userID = @Request.Params.Get("userID");
            if (!file.Equals(null))
            {
                string fileName;
                fileName = file.FileName;
                Directory.CreateDirectory(Server.MapPath("~/Files/"));
                file.SaveAs(Path.Combine(Server.MapPath("~/Files/" + fileName)));
                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
            }
            else
            {
                throw new DirectoryNotFoundException();
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                mainRHub.NotifyByID("Всё отлично", userID);
                return Json("true", JsonRequestBehavior.AllowGet);
            }
            mainRHub.NotifyByID("Всё очень плохо", userID);
            return Json("false", JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult SongUpload() {
            //var description = String.IsNullOrEmpty(Request.Params["description"].ToString()) ? String.Empty : Request.Params["description"].ToString();
            var file = @Request.Files["file"];
            var userID = @Request.Params.Get("userID");
            var SongDescription = @Request.Params.Get("SDescription").ToString() ?? String.Empty;
            string filePath;
            if (!file.Equals(null))
            {
                int isAdded = 0;
                string fileName;
                fileName = file.FileName;
                Directory.CreateDirectory(Server.MapPath("~/Files/"));
                file.SaveAs(Path.Combine(Server.MapPath("~/Files/" + fileName)));
                filePath = Path.Combine(Server.MapPath("~/Files/" + fileName));
                isAdded = addSong(filePath, userID, SongDescription);
            }
            else
            {
                throw new DirectoryNotFoundException();
            }

            if (!String.IsNullOrEmpty(filePath))
            {
                return Json("true", JsonRequestBehavior.AllowGet);
            }
            return Json("false", JsonRequestBehavior.AllowGet);
        }

        public int addSong(string filePath, string userID, string SongDescription) {
            try
            {
                TagLib.File file = TagLib.File.Create(filePath);
                String title = file.Tag.Title;
                String author = file.Tag.FirstPerformer;
                String album = file.Tag.Album;
                String length = file.Properties.Duration.ToString(@"mm\:ss");

                filePath = filePath.Replace(Request.ServerVariables["APPL_PHYSICAL_PATH"], String.Empty);

                Authors performer = new Authors() { Country = file.Tag.MusicBrainzReleaseCountry, Description = file.Tag.JoinedGenres, Name = file.Tag.FirstPerformer };
                db.Authors.Add(performer);
                Songs song = new Songs() { Length = file.Properties.Duration, Name = file.Tag.Title, Year = Convert.ToInt16(file.Tag.Year), Authors = performer, Path_location = filePath };
                db.Songs.Add(song);
                Songs_descriptions sDescription = new Songs_descriptions() { Songs = song, Description = SongDescription ?? file.Tag.Lyrics };
                db.Songs_descriptions.Add(sDescription);

                song.Songs_descriptions = new List<Songs_descriptions>(){ sDescription };
                performer.Songs.Add(song);

                db.SaveChanges();

                sendNotificationSound(string.Format("<h3>Добавлена песня</h3><div class='notRow'>Название: {0}</div><div class='notRow'>Автор: {1}</div><div class='notRow'>Длина: {2}</div>", 
                    song.Name, performer.Name, length ), userID);
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Entity: {0}\nProperty: {1}\nError: {2}\n",
                                                validationErrors.Entry.Entity,
                                                validationError.PropertyName,
                                                validationError.ErrorMessage);
                        sendNotificationSound(string.Format("Entity: {0}<br />Property: {1}<br />Error: {2}<br />",
                                                validationErrors.Entry.Entity,
                                                validationError.PropertyName,
                                                validationError.ErrorMessage), userID);
                    }
                }
            }
            return 0;
        }

        public static void sendNotificationSound(string message, string userID)
        {
            mainRHub.SongAdded(message, userID);
        }
    }
}