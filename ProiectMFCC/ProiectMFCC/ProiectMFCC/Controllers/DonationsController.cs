using ProiectMFCC.Models.Entities;
using ProiectMFCC.Repositories;
using ProiectMFCC.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Web.Mvc;

namespace ProiectMFCC.Controllers
{
    public class DonationsViewModel
    {
        public List<Donation> Donations { get; set; }

        [Display(Name = "Enter donor name")]
        public string DonorName { get; set; }

        [Display(Name = "Enter new donor name")]
        public string NewDonorName { get; set; }

        public IEnumerable<SelectListItem> Countries { get; set; }

        public List<Project> Projects { get; set; }

        public string CountryId { get; set; }

        [Display(Name = "Enter country name")]
        public string NewCountryName { get; set; }

        [Display(Name = "Enter project name")]
        public string ProjectName { get; set; }

        [Display(Name = "Enter amount")]
        public string Amount { get; set; }

        public int TotalDonations { get; set; }
    }

    public class DonationsController : Controller
    {
        private static DonationsRepo _repo = new DonationsRepo(new Scheduler(new Dictionary<object, Models.Transactions.Lock>(), new List<Models.Transactions.Transaction>(), new List<Models.Transactions.WaitFor>()));
        private static CountriesRepo _countriesRepo = new CountriesRepo(new Scheduler(new Dictionary<object, Models.Transactions.Lock>(), new List<Models.Transactions.Transaction>(), new List<Models.Transactions.WaitFor>()));
        private static ProjectsRepo _projectRepo = new ProjectsRepo();
        private static string donorName = "";

        public DonationsController() { }

        private void DecorateModel(DonationsViewModel model)
        {
            model.Countries = GetCountriesListItem();
            model.Projects = GetProjects();
            model.NewCountryName = "";
            model.CountryId = "";
            model.Donations = (model.Donations != null) ? model.Donations : new List<Donation>();
            model.TotalDonations = 0;
            if ((model.Donations != null) && (model.Donations.Count > 0))
            {
                model.TotalDonations = model.Donations.Sum(d => d.Amount);
            }
        }

        public void GetDonationsDelegate(DonationsViewModel model)
        {
            Debug.WriteLine("get donations called");
            var dons = _repo.GetDonationsByDonorName(model.DonorName);
            model.Donations = dons;
            donorName = model.DonorName;
            DecorateModel(model);
            Debug.WriteLine("get donations ended");
        }

        public List<Donation> GetDonationsList(string donorName)
        {
            return _repo.GetDonationsByDonorName(donorName);
        }

        public ActionResult GetDonations(DonationsViewModel model)
        {
            model.Donations = GetDonationsList(model.DonorName);
            donorName = model.DonorName;
            DecorateModel(model);
            return View("DonationsPortal", model);
        }

        public ActionResult AddDonor(DonationsViewModel model)
        {
            int countryId;
            if ((model.NewCountryName != null) && (model.NewCountryName.Length > 0))
            {
                countryId = _countriesRepo.AddCountry(model.NewCountryName);
            }
            else
            {
                countryId = int.Parse(model.CountryId);
            }
            _repo.AddDonor(model.NewDonorName, countryId);
            DecorateModel(model);
            return View("DonationsPortal", model);
        }

        public ActionResult AddDonation(DonationsViewModel model)
        {
            _repo.AddDonation(donorName, model.ProjectName, int.Parse(model.Amount));
            model.Donations = GetDonationsList(donorName);
            DecorateModel(model);
            return View("DonationsPortal", model);
        }

        public ActionResult DeleteDonation(DonationsViewModel model)
        {
            int id = int.Parse(Request["donationId"]);
            _repo.DeleteDonation(id);

            ModelState.Remove("DonorName");
            model.DonorName = donorName;
            return GetDonations(model);
        }


        private IEnumerable<SelectListItem> GetCountriesListItem()
        {
            var selectList = new List<SelectListItem>();
            List<Country> countries = _countriesRepo.GetCountries();

            foreach (var c in countries)
            {
                selectList.Add(new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                });
            }

            return selectList;
        }

        private List<Project> GetProjects()
        {
            return _projectRepo.GetProjects();
        }


        public ActionResult Index()
        {
            var model = new DonationsViewModel()
            {
                DonorName = ""
            };
            DecorateModel(model);
            return View("DonationsPortal", model);
        }
    }
}
