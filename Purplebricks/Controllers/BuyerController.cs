using Purplebricks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Purplebricks.Controllers
{
    [Authorize(Roles = "Buyer")]
    public class BuyerController : Controller
    {
        public ActionResult Index()
        {
            var onSale = Property.OnSale();

            return View(onSale);
        }

        [HttpPost]
        public ActionResult FilteredList(string filter)
        {
            string correctedString = filter.ToLowerInvariant();
            var onSale = Property.OnSale().Where(x =>
                 (Regex.IsMatch(x.Name, filter, RegexOptions.IgnoreCase) || Regex.IsMatch(x.Type, filter, RegexOptions.IgnoreCase)
                 || Regex.IsMatch(x.Description, filter, RegexOptions.IgnoreCase))).ToArray();

            return Json(onSale);
        }

        [HttpGet]
        public ActionResult MakeOffer(int? PropertyID)
        {
            if (!PropertyID.HasValue)
                return RedirectToAction("Index");

            return View(new OfferModel() { PropertyID = PropertyID });
        }

        [HttpPost]
        public ActionResult MakeOffer(OfferModel model)
        {
            model.PushToDB(User.Identity.Name);

            return RedirectToAction("Index");
        }

        public ActionResult RecentOffers()
        {
            return View();
        }

    }
}
