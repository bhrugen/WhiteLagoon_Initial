using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;

using System.Security.Claims;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models;
using WhiteLagoon_Models.ViewModels;
using WhiteLagoon_Utility;

using Syncfusion.Drawing;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Interactive;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Syncfusion.Pdf.Parsing;
using NuGet.Protocol;
using System.Xml;
using System.Collections;
using Microsoft.AspNetCore.Mvc.Rendering;


using System.Data;
using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Collections.Generic;
using WhiteLagoon_DataAccess.Repository;
using System.Reflection;
using System.Globalization;
using WhiteLagoon_Models.DTO;

using Syncfusion.DocIO;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;

namespace WhiteLagoon.Controllers
{
    public class BookingController : Controller
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly List<string> _bookedStatus = new List<string> { "Approved", "CheckedIn" };
        private readonly IUnitOfWork _unitOfWork;
        public BookingController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }

        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult FinalizeBooking(int villaId, DateOnly checkInDate, int nights)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            ApplicationUser user = _unitOfWork.User.Get(u => u.Id == userId);
            BookingDetail booking = new()
            {
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId, includeProperties: "VillaAmenity"),
                CheckInDate = checkInDate,
                Nights = nights,
                CheckOutDate = checkInDate.AddDays(nights),
                UserId = userId,
                Phone = user.PhoneNumber,
                Email = user.Email,
                Name = user.Name,

            };
            booking.TotalCost = booking.Villa.Price * nights;
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
                Quantity = 1
            });


            var service = new SessionService();

            //RAVI check availability again to be double sure
            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            int roomsAvailable = SD.VillaRoomsAvailable_Count(villa, villaNumbersList,
                bookingDetail.CheckInDate, bookingDetail.Nights, bookedVillas);
            if (roomsAvailable == 0)
            {
                TempData["error"] = "Room has been sold out!";
                //no rooms available
                return RedirectToAction(nameof(FinalizeBooking), new
                {
                    villaId = bookingDetail.VillaId,
                    checkInDate = bookingDetail.CheckInDate,
                    nights = bookingDetail.Nights
                });
            }


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
            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
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
                    _unitOfWork.Booking.UpdateStatus(bookingId, SD.StatusApproved, 0);
                    _unitOfWork.Save();
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
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CheckOut(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCompleted, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelBooking(BookingDetail bookingDetail)
        {
            _unitOfWork.Booking.UpdateStatus(bookingDetail.Id, SD.StatusCancelled, 0);
            _unitOfWork.Save();
            TempData["Success"] = "Booking Updated Successfully.";
            return RedirectToAction(nameof(BookingDetails), new { bookingId = bookingDetail.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult GeneratePDF(int id)
        {
            string basePath = _webHostEnvironment.WebRootPath;

            // Create a new document
            WordDocument doc = new WordDocument();

            // Load the template.
            string dataPathSales = basePath + @"/Sample/SampleVilla.docx";
            FileStream fileStream = new FileStream(dataPathSales, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            doc.Open(fileStream, FormatType.Automatic);

            //Get Villa Booking Details
            VillaBookingModel villaBookingDetail = GetWordDocumentBindingData(id);
            MailMergeDataTable villaBookingDetailDataSource = new MailMergeDataTable("VillaBookingDetails", villaBookingDetail.VillaBookingDetails);
            doc.MailMerge.ExecuteGroup(villaBookingDetailDataSource);

            //Get Villa Details
            MailMergeDataTable villaDetailsDataSource = new MailMergeDataTable("VillaDetails", villaBookingDetail.VillaDetails);
            doc.MailMerge.ExecuteGroup(villaDetailsDataSource);

            //Get Villa Payment Details
            MailMergeDataTable villaPaymentDetailsDataSource = new MailMergeDataTable("VillaPaymentDetails", villaBookingDetail.VillaPaymentDetails);
            doc.MailMerge.ExecuteGroup(villaPaymentDetailsDataSource);

            // Using Merge events to do conditional formatting during runtime.
            doc.MailMerge.MergeField += new MergeFieldEventHandler(MailMerge_MergeField);

            using (DocIORenderer render = new DocIORenderer())
            {
                //Converts Word document into PDF document
                PdfDocument pdfDocument = render.ConvertToPDF(doc);

                //Saves the PDF document to MemoryStream.
                MemoryStream stream = new MemoryStream();
                pdfDocument.Save(stream);
                stream.Position = 0;

                //Download PDF document in the browser.
                return File(stream, "application/pdf", "VillaDetails.pdf");
            }
        }

        private VillaBookingModel GetWordDocumentBindingData(int bookingId)
        {
            VillaBookingModel bookingVillaModel = new VillaBookingModel();

            //Get data from DB
            BookingDetail bookingFromDb = GetBookingDetails(bookingId);

            //Bind Booking model
            bookingVillaModel.VillaBookingDetails = GetCustomerBookingData(bookingFromDb);
            bookingVillaModel.VillaDetails = GetVillaData(bookingFromDb);
            bookingVillaModel.VillaPaymentDetails = GetPaymentData(bookingFromDb);

            return bookingVillaModel;
        }

        private BookingDetail GetBookingDetails(int bookingId)
        {
            BookingDetail bookingFromDb = _unitOfWork.Booking.Get(u => u.Id == bookingId, includeProperties: "User,Villa");
            if (bookingFromDb.VillaNumber == 0 && bookingFromDb.Status == SD.StatusApproved)
            {
                var availableVillaNumbers = AssignAvailableVillaNumberByVilla(bookingFromDb.VillaId, bookingFromDb.CheckInDate);

                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId
                            && availableVillaNumbers.Any(x => x == m.Villa_Number)).ToList();
            }
            else
                bookingFromDb.VillaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == bookingFromDb.VillaId && m.Villa_Number == bookingFromDb.VillaNumber).ToList();
            return bookingFromDb;
        }

        private List<VillaBookingDetailsModel> GetCustomerBookingData(BookingDetail bookingFromDb)
        {
            List<VillaBookingDetailsModel> customerBookings = new List<VillaBookingDetailsModel>();
            VillaBookingDetailsModel customerBooking = new VillaBookingDetailsModel();
            customerBooking.BookingNo = bookingFromDb.Id.ToString();
            customerBooking.BookingDate = bookingFromDb.BookingDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            customerBooking.CustomerName = bookingFromDb.Name;
            customerBooking.CustomerPhone = bookingFromDb.Phone;
            customerBooking.CustomerEmail = bookingFromDb.Email;
            customerBookings.Add(customerBooking);
            return customerBookings;
        }

        private List<VillaDetailsModel> GetVillaData(BookingDetail bookingFromDb)
        {
            List<VillaDetailsModel> villaDetails = new List<VillaDetailsModel>();
            VillaDetailsModel villaDetail = new VillaDetailsModel();
            villaDetail.Villa = bookingFromDb.Villa.Name;
            villaDetail.Nights = Convert.ToString(bookingFromDb.Nights);
            villaDetail.PricePerNight = Convert.ToString(bookingFromDb.Villa.Price.ToString("c"));
            villaDetail.Total = bookingFromDb.TotalCost.ToString("c");
            villaDetail.RoomNumber = bookingFromDb.VillaNumber > 0 ? string.Format("Room Number-#{0}", bookingFromDb.VillaNumber) : "";
            villaDetails.Add(villaDetail);
            return villaDetails;
        }

        private List<VillaPaymentDetailsModel> GetPaymentData(BookingDetail bookingFromDb)
        {
            List<VillaPaymentDetailsModel> villaPaymentDetails = new List<VillaPaymentDetailsModel>();
            VillaPaymentDetailsModel villaPaymentDetail = new VillaPaymentDetailsModel();
            villaPaymentDetail.PaymentDate = Convert.ToDateTime(bookingFromDb.PaymentDate).ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            villaPaymentDetail.CheckInDate = bookingFromDb.CheckInDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            villaPaymentDetail.CheckOutDate = bookingFromDb.CheckOutDate.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);
            villaPaymentDetail.BookingTotal = bookingFromDb.TotalCost.ToString("c");
            villaPaymentDetails.Add(villaPaymentDetail);
            return villaPaymentDetails;
        }

        private void MailMerge_MergeField(object sender, MergeFieldEventArgs args)
        {
            // Conditionally format data during Merge.
            if (args.RowIndex % 2 == 0)
            {
                args.CharacterFormat.TextColor = Syncfusion.Drawing.Color.DarkBlue;
            }

        }

        #region API CALLS

        [HttpGet]
        public IActionResult GetAll(string status = "")
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

            if (!string.IsNullOrWhiteSpace(status))
            {
                objBookings = objBookings.Where(u => u.Status.ToLower() == status.ToLower());
            }

            return Json(new { data = objBookings });
        }

        public List<int> AssignAvailableVillaNumberByVilla(int villaId, DateOnly checkInDate)
        {
            List<int> availableVillaNumbers = new List<int>();

            var villaNumbers = _unitOfWork.VillaNumber.GetAll().Where(m => m.VillaId == villaId).ToList();

            var checkedInVilla = _unitOfWork.Booking.GetAll().Where(m => m.Status == SD.StatusCheckedIn && m.VillaId == villaId).Select(u => u.VillaNumber);


            foreach (var villaNumber in villaNumbers)
            {
                if (!checkedInVilla.Contains(villaNumber.Villa_Number))
                {
                    //Villa is not checked in
                    availableVillaNumbers.Add(villaNumber.Villa_Number);
                }
            }
            return availableVillaNumbers;
        }
        #endregion
    }


    public class TestOrderDetail
    {
        #region Fields

        private string m_orderID;
        private string m_productID;
        private string m_productName;
        private string m_unitPrice;
        private string m_quantity;
        private string m_discount;
        private string m_extendedPrice;
        #endregion

        #region Properties

        public string OrderID
        {
            get { return m_orderID; }
            set { m_orderID = value; }
        }

        public string ProductID
        {
            get { return m_productID; }
            set { m_productID = value; }
        }
        public string ProductName
        {
            get { return m_productName; }
            set { m_productName = value; }
        }
        public string UnitPrice
        {
            get { return m_unitPrice; }
            set { m_unitPrice = value; }
        }
        public string Quantity
        {
            get { return m_quantity; }
            set { m_quantity = value; }
        }
        public string Discount
        {
            get { return m_discount; }
            set { m_discount = value; }
        }
        public string ExtendedPrice
        {
            get { return m_extendedPrice; }
            set { m_extendedPrice = value; }
        }

        #endregion

        #region Constructor       
        public TestOrderDetail()
        { }
        #endregion
    }
    public class TestOrderTotal
    {
        #region Fields

        private string m_orderID;
        private string m_subTotal;
        private string m_freight;
        private string m_total;
        #endregion

        #region Properties

        public string OrderID
        {
            get { return m_orderID; }
            set { m_orderID = value; }
        }

        public string Subtotal
        {
            get { return m_subTotal; }
            set { m_subTotal = value; }
        }
        public string Freight
        {
            get { return m_freight; }
            set { m_freight = value; }
        }
        public string Total
        {
            get { return m_total; }
            set { m_total = value; }
        }
        #endregion

        #region Constructor       
        public TestOrderTotal()
        { }
        #endregion
    }
}
