using Chello.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using TrelloCFD.Configuration;
using TrelloCFD.Domain;
using TrelloCFD.Factories;
using TrelloCFD.Models;

namespace TrelloCFD.Controllers
{
    [Authorize]
    public class DiagramsController : Controller
    {
        //
        // GET: /Diagrams/

        public ActionResult Index()
        {
            ChelloClient client = TrelloClientFactory.Get();

            var boards = client.Boards.ForUser("me");

            ViewBag.Boards = boards.Where(b => !b.Closed);
            ViewBag.ClosedBoards = boards.Where(b => b.Closed);

            return View();
        }

        public ActionResult Board(string id, string s = "", string e = "")
        {
            ChelloClient client = TrelloClientFactory.Get();
            Board board = client.Boards.Single(id);
            ViewBag.BoardName = board.Name;
            ViewBag.BoardId = id;

            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;

            DateTime.TryParseExact(s, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startDate);

            if (!DateTime.TryParseExact(e, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endDate))
            {
                endDate = DateTime.MaxValue;
            }

            ActivityRepository repository = new ActivityRepository(id);
            CumulativeFlowModel cards = new CumulativeFlowModel(repository, startDate, endDate);
            CumulativeFlowModel points = null;

            if (repository.Lists.SelectMany(l => l.Cards).Any(c => c.Points.HasValue))
            {
                points = new CumulativeFlowModel(repository, startDate, endDate, (cs) => { return cs.Where(c => c.Points.HasValue).Sum(c => c.Points.Value); }, "Points");
            }

            return View(new DiagramsModel(cards, points));
        }

        public ActionResult ExportBoard(string id)
        {
            var exporter = new JsonExporter(id, TrelloConfigurationManager.ApiKey, HttpContext.User.Identity.Name);

            var csv = exporter.BuildCsv();

            if (string.IsNullOrWhiteSpace(csv))
            {
                return HttpNotFound("Unable to build CSV");
            }

            var result = new FileContentResult(Encoding.UTF8.GetBytes(exporter.BuildCsv()), "application/CSV");
            result.FileDownloadName = String.Format("{0:ddMMMHHmmss}.csv", DateTime.Now);

            return result;
        }
    }
}
