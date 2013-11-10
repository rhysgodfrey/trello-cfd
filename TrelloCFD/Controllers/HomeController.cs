using Chello.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TrelloCFD.Domain;
using TrelloCFD.Models;

namespace TrelloCFD.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Diagrams");
            }
            
            return View();
        }

        public ActionResult About()
        {
            return View();
        }
        
        public ActionResult Contact()
        {
            return RedirectToActionPermanent("About");
        }

        public ActionResult IdeasAndBugs()
        {
            return RedirectPermanent("https://trello.com/board/trello-cumulative-flow/507c13969b6db2ed27007f53");
        }

        public ActionResult Github()
        {
            return RedirectPermanent("https://github.com/rhysgodfrey/trello-cfd");
        }
    }
}
