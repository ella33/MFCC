using ProiectMFCC.Models.Entities;
using ProiectMFCC.Repositories;
using ProiectMFCC.Services;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace ProiectMFCC.Controllers
{ 
    public class DonationsViewModel
    {
        public List<Donation> Donations { get; set; }
        
        [Display(Name = "Enter donor name")]
        public string DonorName { get; set; }
    }

    public class DonationsController : Controller
    {
        private static DonationsRepo _repo = new DonationsRepo(new Scheduler(new Dictionary<object, Models.Transactions.Lock>(), new List<Models.Transactions.Transaction>(), new List<Models.Transactions.WaitFor>()));

        public DonationsController() { }

        public ActionResult GetDonations(DonationsViewModel model)
        {
            var dons = _repo.GetDonationsByDonorName(model.DonorName);
            model.Donations = dons;
            return View("DonationsPortal", model);
        }

        public ActionResult Index()
        {
            var donor = new DonationsViewModel { DonorName = "" };
            return View("DonationsPortal", donor);
        }
    }
}
