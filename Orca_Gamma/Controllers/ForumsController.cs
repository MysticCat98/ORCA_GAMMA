﻿using PagedList;
using System;
using System.Linq;
using System.Web.Mvc;
using Orca_Gamma.Models;
using Orca_Gamma.Models.DatabaseModels;
using System.Net;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Data.Entity;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using System.Web;
using Microsoft.AspNet.Identity.Owin;

namespace Orca_Gamma.Controllers
{
    public class ForumsController : Controller
    {
 
        private ApplicationDbContext _dbContext;

        public ForumsController()
        {
            _dbContext = new ApplicationDbContext();
        }

        public ViewResult Index(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.nameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            var threads = from s in _dbContext.ForumThreads
                          select s;
            ViewBag.CurrentFilter = searchString;
            if (!String.IsNullOrEmpty(searchString))
            {
                threads = threads.Where(t => t.Subject.Contains(searchString));
            }
            switch (sortOrder)
            {
                case "name_desc":
                    threads = threads.OrderByDescending(s => s.Subject);
                    break;
                default:
                    threads = threads.OrderBy(s => s.Subject);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(threads.ToPagedList(pageNumber, pageSize));
        }

        public ApplicationUser getCurrentUser()
        {
            return _dbContext.Users.Find(System.Web.HttpContext.Current.User.Identity.GetUserId());
        }

        //GET: Forums/Create
        public ActionResult Create()
        {
            return View();
        }

        //POST: Forums/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ForumThread model)
        {
			ApplicationUser user = getCurrentUser();

			var post = new ForumThread
            {
				User = user, // This is how you do foreign keys - Cass
                Subject = model.Subject,
                FirstPost = model.FirstPost,
                Date = DateTime.Now

            };
                    
            try
            {
                if (!ModelState.IsValid)
                {
                    _dbContext.ForumThreads.Add(post);
                    _dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }

            return View(post);
        }

        public ViewResult Details(int? page)
        {
            var threads = from s in _dbContext.ThreadMessagePosts
                          select s;
            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(threads.ToPagedList(pageNumber, pageSize));
        }

        //GET: Forums/Reply
        public ActionResult Reply()
        {
            return View();
        }

        //POST: Forums/Reply
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(ThreadMessagePost model)
        {

            ApplicationUser user = getCurrentUser();
            ForumThread thread = new ForumThread();
            var post = new ThreadMessagePost
            {
                CreatedBy = User.Identity.GetUserId(),
                Thread = thread,
                Body = model.Body,
                Date = DateTime.Now
            };

            try
            {
                if (!ModelState.IsValid)
                {
                    _dbContext.ThreadMessagePosts.Add(post);
                    _dbContext.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists see your system administrator.");
            }



            return View(post);
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _dbContext.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}