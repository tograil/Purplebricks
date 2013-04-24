using Purplebricks.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;

namespace Purplebricks.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        public ActionResult Index()
        {
            var onSale = Property.OnSaleWithOffers(User.Identity.Name);

            return View(onSale);
        }

        [HttpGet]
        public ActionResult AddProperty(int? PropertyID)
        {
            if (PropertyID.HasValue)
            {
                return View(PropertyModel.ByID(PropertyID));
            }

            return View();
        }

        [HttpPost]
        public ActionResult FilteredList(string filter)
        {
            string correctedString = filter.ToLowerInvariant();
            var onSale = Property.OnSaleWithOffers(User.Identity.Name).Where(x =>
                 (Regex.IsMatch(x.Name, filter, RegexOptions.IgnoreCase) || Regex.IsMatch(x.Type, filter, RegexOptions.IgnoreCase)
                 || Regex.IsMatch(x.Description, filter, RegexOptions.IgnoreCase))).ToArray();

            return Json(onSale);
        }

        [HttpPost]
        public ActionResult AddProperty(PropertyModel model)
        {
            
            model.PushToDB(User.Identity.Name);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult ImportProperties()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Offers(int? PropertyID)
        {
            if (!PropertyID.HasValue)
                return RedirectToAction("Index");

            var offers = OfferModel.ByPropertyID(PropertyID);

            return View(offers);
        }

        [HttpGet]
        public ActionResult AcceptOrReject(int? PropertyID, int? OfferID, bool Accept)
        {
            if (Accept)
            {
                OfferModel.Accept(OfferID, PropertyID);
                return RedirectToAction("Index");
            }
            else
            {
                OfferModel.Reject(OfferID);
                return RedirectToAction("Offers", new { PropertyID = PropertyID });
            }
        }

        [HttpPost]
        public ActionResult ImportProperties(HttpPostedFileBase excel)
        {
            try
            {
                // Verify that the user selected a file
                if (excel != null && excel.ContentLength > 0)
                {
                    string CONNECTION_STRING = "Provider=Microsoft.ACE.OLEDB.12.0; data source={0}; Extended Properties=Excel 12.0;";
                    // extract only the fielname
                    var fileName = Path.GetFileName(excel.FileName);
                    // store the file inside ~/App_Data/uploads folder
                    var path = Path.Combine(Server.MapPath("~/App_Data"), fileName);
                    excel.SaveAs(path);


                    var connectionString = string.Format(CONNECTION_STRING, path);

                    using (var objConnection = new OleDbConnection(connectionString))
                    {
                        objConnection.Open();

                        DataTable dtSchema = objConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                        var res = (from DataRow row in dtSchema.Rows select row.Field<string>("TABLE_NAME")).ToList();

                        var adapter = new OleDbDataAdapter(string.Format("SELECT * FROM [{0}]", res[0]), objConnection);

                        var ds = new DataSet();

                        adapter.Fill(ds, "Properties");

                        var values = (from DataRow row in ds.Tables[0].Rows
                                      select new PropertyModel
                                      {
                                          Name = row.Field<string>("Name"),
                                          Type = row.Field<string>("Type"),
                                          Description = row.Field<string>("Description")
                                      }).ToList();

                        PropertyModel.ImportToDB(User.Identity.Name, values);

                    }


                }
            }
            catch
            {
                //TODO: Future error message and logger
            }

            return RedirectToAction("Index");
        }

    }
}
