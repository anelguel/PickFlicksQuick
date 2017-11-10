using MovieQuizApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Web.Security;


namespace MovieQuizApp.Controllers
{
    public class HomeController : Controller
    {

        private MovieQuizDbEntities db = new MovieQuizDbEntities();//reference to our MovieQuiz Databse
      Registration user; // global user variable of registration
       
              

        public ActionResult Index()
        {
            return View();
        }
        
        public ActionResult Register()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Registration reg)
        {
            if (ModelState.IsValid)
            {
                db.Registrations.Add(reg);
                db.SaveChanges();
                return RedirectToAction("Index");
            }


            return View();
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Registration reg)
        {
                var details = (from userlist in db.Registrations
                               where userlist.Username == reg.Username && userlist.Password == userlist.Password
                               select new
                               {
                                   userlist.UserID,
                                   userlist.Username
                               }).ToList();
                if(details.FirstOrDefault()!= null)
                {
                    Session["UserID"] = details.FirstOrDefault().UserID;
                    Session["Username"] = details.FirstOrDefault().Username;
               // user = db.Registrations.Find(Session["UserId"]);
           


                return RedirectToAction("Welcome", "Home");
                }
            
            
               // ModelState.AddModelError("", "Invalid Credentials");
            

            return View(reg);
        }


        //[HttpPost]

        public ActionResult LogOut()

        {

            FormsAuthentication.SignOut();

            Session.Abandon();

            return View();

            //return RedirectToAction("Index", "Home");

        }


        public ActionResult Welcome()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }


        public ActionResult GetData(Movy m)
        {
            Registration u = new Registration();
            string genreID = m.genre;
            string primaryreleasedate = m.primaryReleaseDate;
            string voteaverage = m.VoteAverage;
            //ViewBag.GID = genreID;
            //ViewBag.userid = u.UserID;

            
            //Make a request but don't send it yet
            //string urlprefix = "http://api.themoviedb.org/3/genre/";
            //string urlpost = "/movies?sort_by=created_at.asc&language=en&api_key=92d7084568b97fb382838cc03254d49e&type=json";

            
            HttpWebRequest request = WebRequest.CreateHttp("http://api.themoviedb.org/3/discover/movie?api_key=92d7084568b97fb382838cc03254d49e&language=en-US&with_genres=" + (genreID) + "&" + (primaryreleasedate) + "&" + (voteaverage) + "&type=Json");



            //Tell it the list of browsers we're using
            //request.UserAgent = @"User-Agent: Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/53.0.2785.116 Safari/537.36";

            //If you need to use OAuth or Keys there will be a few extra steps right around here you go on to 
            //grab a response.
            //push the request over to the remote server 
              HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //Parse the response data (this looks a lot like reading in a text file, file I/O)
             StreamReader rd = new StreamReader(response.GetResponseStream());

            //Return the data in string format 
             String data = rd.ReadToEnd();

            //This is where things change based upon whether we're using XML or Json
            //Personally I prefer JSON, but they're equivalent to each other
            JObject o = JObject.Parse(data);

            //Now we can step through the JSON data 
            //the way to approach this is to think of every tag either contains a string array or points 
            //to another list. As you try to construct this always always have the JSON viewer open
            //With the array portion you can use  the .ToList() or ToArray() methods to make a collection
            //of JTokens
            //ViewBag.Production = o["operationalMode"];
            List<string> genrelist = new List<string>();         
            List<string> languagelist = new List<string>();
            List<string> picturelist = new List<string>();
            //List<string> genrelist1 = new List<string>();
            for (int i = 0; i < o["results"].Count(); i++)
            {
                string genres = o["results"][i]["original_title"].ToString();
                string language = o["results"][i]["original_language"].ToString();
                string picture = o["results"][i]["poster_path"].ToString();
                //string genres1 = o["results"][i]["release_date"].ToString();
                genrelist.Add(genres);
                languagelist.Add(language);
                picturelist.Add("http://image.tmdb.org/t/p/w300" + picture);
                // genrelist1.Add(genres1);
            }
            ViewBag.AllGenres = genrelist;
            ViewBag.AllLanguages = languagelist;
            ViewBag.AllPictures = picturelist;
            Random r = new Random();
            int rando = r.Next(0, 20);
            for (int i = 0; i < genrelist.Count(); i++)
            {
                ViewBag.MovieTitle = genrelist.ElementAt(rando);
                ViewBag.AllLanguages = languagelist.ElementAt(rando);
                ViewBag.AllPictures = picturelist.ElementAt(rando);
            }
            ////Code to save movie title in database begins... yet to be tested. 
            ////*******************************************************************
            m.Title = ViewBag.MovieTitle;

            if (ModelState.IsValid)
            {

                m.UserID = int.Parse(Session["UserID"].ToString());                            
                m.Title = ViewBag.MovieTitle;
                db.Movies.Add(m);

                db.SaveChanges();
                return View();

            }
            //*******************************************************************

            ////ViewBag.AllGenres1 = genrelist1;

            //for (int i = 0; i < o["Search"].Count(); i++)
            //{
            //    ViewBag.Production += o["Search"][i]["Title"];
            //}

            //ViewBag.genreCount = o["genres"].Count();

            //ViewBag.Time = o["time"]["startPeriodName"][0];
            //ViewBag.ApiText = "Tomorrow's Temperature is " + o["data"]["temperature"][2] + " as of " + o["creationDateLocal"];
            //ViewBag.JSONData = "" + o["productionCenter"];
            ////You can step through data just like an array
            //ViewBag.Temp = o["data"]["temperature"][4];

            //ViewBag.Json = o.ToString();
            //List<JToken> times = o["time"].ToList();
            //List<string> temps = new List<string>();
            //https://stackoverflow.com/questions/9198426/mvc3-putting-a-newline-in-viewbag-text
            //You want the front end to care about presenting data, so we do our newlines there
            //for (int i = 0; i < o["data"]["temperature"].Count(); i++)
            //{
            //    //string timeLabel = times[i].ToString();
            //    //string input = o["time"]["startPeriodName"][i] + " " + o["time"]["tempLabel"][i] + " " + o["data"]["temperature"][i].ToString();

            //    temps.Add(input);
            //}
            //ViewBag.AllTemps = temps;

            return View();
        }





        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}