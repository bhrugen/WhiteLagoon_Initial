using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Stripe;
using Stripe.Checkout;
using Stripe.Terminal;
using System.Security.Claims;
using System.Text;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;
using WhiteLagoon_Models.ViewModels;
using WhiteLagoon_Utility;
using WhiteLagoon_Utility.Helper.Email;
using WhiteLagoon_Utility.Helper.Export;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace WhiteLagoon.Controllers
{
    public class BookingController : Controller
    {
        private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
        private readonly IUnitOfWork _unitOfWork;
        private ICompositeViewEngine _viewEngine;
        private readonly string _rootDirectory;

        public BookingController(IUnitOfWork unitOfWork, ICompositeViewEngine viewEngine, IHostingEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _viewEngine = viewEngine;
            _rootDirectory = env.WebRootPath;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, string checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);
            BookingDetail booking = new()
            {
                Villa=_unitOfWork.Villa.Get(u=>u.Id==villaId, includeProperties: "VillaAmenity"),
                CheckInDate=DateOnly.FromDateTime(Convert.ToDateTime(checkInDate)),
                Nights=nights,
                CheckOutDate= DateOnly.FromDateTime(Convert.ToDateTime(checkInDate)).AddDays(nights),
                UserId=userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,
            
            };
            booking.TotalCost = booking.Villa.Price * nights;


            //booking.VillaNumber = AssignAvailableVillaNumberByVilla(villaId, DateOnly.FromDateTime(Convert.ToDateTime(checkInDate)), nights);


            return View(booking);
        }
        [Authorize]
        [HttpPost]
        public IActionResult FinalizeBooking(BookingDetail bookingDetail)
        {

            var villa = _unitOfWork.Villa.Get(u => u.Id == bookingDetail.VillaId);

            bookingDetail.TotalCost = (villa.Price * bookingDetail.Nights);
            bookingDetail.Status = SD.StatusPending;
            bookingDetail.BookingDate = DateTime.Now;

            _unitOfWork.Booking.Add(bookingDetail);
            _unitOfWork.Save();



                //it is a regular customer account and we need to capture payment
                //stripe logic
                var domain = Request.Scheme + "://" + Request.Host.Value + "/";
                var options = new SessionCreateOptions
                {
                    SuccessUrl = domain + $"booking/BookingConfirmation?bookingId={bookingDetail.Id}",
                    CancelUrl = domain + $"booking/finalizeBooking?villaId={bookingDetail.VillaId}&checkInDate={bookingDetail.CheckInDate}&nights={bookingDetail.Nights}",
                    LineItems = new List<SessionLineItemOptions>(),
                    Mode = "payment",
                };

            options.LineItems.Add(new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(bookingDetail.TotalCost * 100), // $20.50 => 2050
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = villa.Name,
                        //Images = new List<string>()
                        //        {
                        //            Request.Scheme + "://" + Request.Host.Value + villa.ImageUrl.Replace('\\','/')
                        //        },
                        
                    }
                    
                },
                Quantity=1
            });


                var service = new SessionService();
                Session session = service.Create(options);
                _unitOfWork.Booking.UpdateStripePaymentID(bookingDetail.Id, session.Id, session.PaymentIntentId);
                _unitOfWork.Save();
                Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);

        }

        [Authorize]
        public IActionResult BookingDetails(int bookingId)
        {
            BookingDetail bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

            if (bookingFromDb.VillaNumber == 0)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId, bookingFromDb.CheckInDate);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId
                            && availableVillaNumbers.Any(x => x == m.Villa_Number)).ToList();
            }
            else
            {
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId && m.Villa_Number == bookingFromDb.VillaNumber).ToList();
            }

            return View(bookingFromDb);
        }

        [Authorize]
        public IActionResult BookingConfirmation(int bookingId)
        {
            BookingDetail bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.Status == SD.StatusPending)
            {
                //this is a pending order

                var service = new SessionService();
                Session session = service.Get(bookingFromDb.StripeSessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.Booking.UpdateStripePaymentID(bookingId, session.Id, session.PaymentIntentId);
                    _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved, bookingFromDb.VillaNumber);
                    _unitOfWork.Save();
                    //BookingConfirmationTemplate(bookingDetail.Email); // Enable when email is configured
                }

            }
            return View(bookingId);
        }

        [HttpPost]
        //[Authorize(Roles = SD.Role_Admin + "," + SD.Role_Employee)]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckIn(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCheckedIn, bookingDetail.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id});
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCompleted, bookingDetail.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCancelled, bookingDetail.VillaNumber);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status="")
        {
            IEnumerable<BookingDetail> objBookings;


            if (User.IsInRole(SD.Role_Admin))
            {
                objBookings = _unitOfWork.Booking.GetAll(includeProperties: "User,Villa").ToList();
            }
            else
            {

                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                objBookings = _unitOfWork.Booking
                    .GetAll(u => u.UserId == userId, includeProperties: "User,Villa");
            }

            if(!string.IsNullOrWhiteSpace(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower() == status.ToLower());
            }
            
            return Json(new { data = objBookings });
        }


        #endregion

        public bool BookingConfirmationTemplate(string EmailTo)
        {
            StringBuilder bookedTemplate = new StringBuilder();
            bookedTemplate.Append("<h4 style='text-align:center;'><img src='https://cdn.logojoy.com/wp-content/uploads/2018/05/01112536/5_big12.png' width='89' height='25' alt='eTracker'></h4>");
            bookedTemplate.Append("<p style='margin-left:15px;'>You booking has been confirmed.</p>");
            bookedTemplate.Append("<p style='margin-left:15px;'>Now you can manage your booking.</p>");
            bookedTemplate.Append("<p><h4 style='margin-left:15px;'>Thank You<h4><p>");

            EmailData data = new EmailData();
            data.To = EmailTo;
            data.Subject = "Booking Confirmation";
            data.Body = bookedTemplate.ToString();

            return SendMail.CommonMailFormat(data);
        }

        public List<int> AssignAvailableVillaNumberByVilla(int villaId, DateOnly checkInDate)
        {
            List<int> availableVillaNumbers = new List<int>();

            var VillaNumumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == villaId).ToList();

            var checkViaStatus = _unitOfWork.Booking.GetAll().Where(m => (_bookedStatus.Any(i => i.ToString() == m.Status)) && m.VillaId == villaId).ToList();

            var bookedVillas = _unitOfWork.Booking.GetAll().Where(m => (m.CheckInDate <= checkInDate && m.CheckOutDate >= checkInDate) &&
                              (_bookedStatus.Any(i => i.ToString() == m.Status)) && m.VillaId == villaId).ToList();

            var aoccupiedroom = checkViaStatus.Select(m => m.VillaNumber).ToList();

            foreach (var villa in VillaNumumbers)
            {
                if (!aoccupiedroom.Any(i => i == villa.Villa_Number))
                {
                    availableVillaNumbers.Add(villa.Villa_Number);
                }
                else if (aoccupiedroom.Count() == 0)
                {
                    availableVillaNumbers.Add(villa.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }

        public async Task<IActionResult> ExportPdfAsync(int bookingId)
        {
            BookingDetail bookingModel = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");

            bookingModel.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingModel.VillaId && m.Villa_Number == bookingModel.VillaNumber).ToList();

            StringBuilder bookedTemplate = new StringBuilder();
            bookedTemplate.Append("<html><head><link rel='stylesheet' href='site.css'/> <link rel='stylesheet' href='https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css'/></head><body>");
            bookedTemplate.Append(await RenderViewToString("BookingDetails", bookingModel));
            bookedTemplate.Append("</body>");

            string baseUrl = Path.Combine(_rootDirectory, "css");
            MemoryStream memoryStream = ExportInPdf.DownloadPdf(bookedTemplate.ToString(), baseUrl);
            return File(memoryStream.ToArray(), System.Net.Mime.MediaTypeNames.Application.Pdf, "BookingDetails.pdf");
        }

        private async Task<string> RenderViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            using (var writer = new StringWriter())
            {
                ViewEngineResult viewResult =
                    _viewEngine.FindView(ControllerContext, viewName, false);

                ViewContext viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    writer,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);

                return writer.GetStringBuilder().ToString();
            }
        }
    }
}
