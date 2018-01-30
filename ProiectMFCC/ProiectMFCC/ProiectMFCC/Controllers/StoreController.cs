using ProiectMFCC.Models.Main;
using ProiectMFCC.Repositories;
using System.Collections.Generic;
using System.Web.Mvc;

namespace ProiectMFCC.Controllers
{
    public class ViewModel
    {
        public string ClientName { get; set; }
        public string InstrumentName { get; set; }
        public string AccessoryName { get; set; }
        public string DeletedItemName { get; set; }
        public string EmailValue { get; set; }
        public Cart Cart { get; set; }
    }

    public class StoreController : Controller
    {
        private static Repository _repository = new Repository(new Services.Scheduler(new Dictionary<object, Models.Transactions.Lock>(), new List<Models.Transactions.Transaction>(), new List<Models.Transactions.WaitFor>()));

        public StoreController()
        {

        }

        public ActionResult GetCart(ViewModel model)
        {
            var cart = _repository.GetCartByUser(model.ClientName);
            model.Cart = cart;
            return View("StoreManagement", model);
        }

        public ActionResult AddInstrumentToCart(ViewModel model)
        {
            var status = _repository.AddToCart(model.ClientName, model.InstrumentName, "instrument");
            if (status == "aborted")
                ViewBag.ErrorMessage = "Deadlock occured. Your insert action was aborted.";
            var cart = _repository.GetCartByUser(model.ClientName);
            model.Cart = cart;
            return View("StoreManagement", model);
        }

        public ActionResult AddAccessoryToCart(ViewModel model)
        {
            _repository.AddToCart(model.ClientName, model.AccessoryName, "accessory");
            var cart = _repository.GetCartByUser(model.ClientName);
            model.Cart = cart;
            return View("StoreManagement", model);
        }

        public ActionResult DeleteItem(ViewModel model)
        {
            _repository.DeleteItem(model.ClientName, model.DeletedItemName);
            var cart = _repository.GetCartByUser(model.ClientName);
            model.Cart = cart;
            return View("StoreManagement", model);
        }

        public ActionResult ChangeEmail(ViewModel model)
        {
            var client = _repository.ChangeEmail(model.ClientName, model.EmailValue);
            if (client == null)
            {
                ViewBag.ErrorMessage = "Deadlock occured. Your change email action was aborted.";
            }
            else
            {
                ViewBag.Email = client.Email;
            }
           
            var cart = _repository.GetCartByUser(model.ClientName);
            model.Cart = cart;
            return View("StoreManagement", model);
        }

        public ActionResult Deadlock(ViewModel model)
        {            
            var cl = _repository.SimulateDeadlockWithInsert("Maria", "Piano");
            var cart = _repository.GetCartByUser(model.ClientName);
            model.Cart = cart;
            return View("StoreManagement", model);
       } 

        // GET: Store
        public ActionResult Index()
        {
            var client = new ViewModel { Cart = new Cart() };
            return View( "StoreManagement", client);
        }

      
    }
}
