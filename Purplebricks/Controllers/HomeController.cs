﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Purplebricks.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Purplebricks";

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Purplebricks";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Purplebricks";

            return View();
        }
    }
}
