using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.Presentation;
using System.Drawing;
using System.Net;
using WhiteLagoon_DataAccess.Repository.IRepository;
using WhiteLagoon_Models.ViewModels;
using WhiteLagoon_Utility;
using Svg;
using System.IO;
using System.Text;

namespace WhiteLagoon.Controllers
{
    public class HomeController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public HomeController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
        {
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
        }
        public IActionResult Index()
        {
            HomeVM homeVM = new HomeVM()
            {
                VillaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList(),
                Nights = 1
            };
            return View(homeVM);
        }

        [HttpPost]
        public IActionResult GetVillasByDate(int nights, DateOnly checkInDate)
        {
            var villaList = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").ToList();

            var villaNumbersList = _unitOfWork.VillaNumber.GetAll().ToList();
            var bookedVillas = _unitOfWork.Booking.GetAll(u => u.Status == SD.StatusApproved ||
            u.Status == SD.StatusCheckedIn).ToList();

            foreach (var villa in villaList)
            {
                int roomsAvailable = SD.VillaRoomsAvailable_Count(villa, villaNumbersList, checkInDate, nights, bookedVillas);
                villa.IsAvailable = roomsAvailable > 0 ? true : false;
            }

            HomeVM homeVM = new()
            {
                CheckInDate = checkInDate,
                VillaList = villaList,
                Nights = nights
            };
            return PartialView("_VillaList", homeVM);
        }

        [HttpPost]
        public IActionResult GeneratePPT(int id)
        {
            var villa = _unitOfWork.Villa.GetAll(includeProperties: "VillaAmenity").FirstOrDefault(x => x.Id == id);

            string basePath = _webHostEnvironment.WebRootPath;
            string dataPath = basePath + @"/Sample/SampleVilla.pptx";

            using (IPresentation presentation = Presentation.Open(dataPath))
            {
                // Modify the presentation as needed
                // Iterate through each slide in the presentation
                foreach (ISlide slide in presentation.Slides)
                {
                    // Find the Villa Name text box shape by its name
                    IShape shape = FindShapeByName(slide, "txtVillaName");
                    if (shape != null)
                        shape.TextBody.Text = villa.Name;

                    // Find the image shape by its id
                    shape = FindShapeByName(slide, "imgVilla");
                    if (shape is IShape)
                    {
                        byte[] imageData;
                        string imageUrl = string.Format("{0}/{1}", basePath, villa.ImageUrl);
                        imageData = System.IO.File.ReadAllBytes(imageUrl);
                        string fileExtension = Path.GetExtension(imageUrl);

                        if (fileExtension.Contains("svg", StringComparison.OrdinalIgnoreCase))
                        {
                            // Image is in SVG format
                            // Convert the SVG image to an image format like PNG or JPEG
                            imageData = ConvertSvgToPng(imageData);
                        }

                        // Remove the existing image shape
                        slide.Shapes.Remove(shape);

                        // Create a memory stream from the new image data
                        using (MemoryStream imageStream = new MemoryStream(imageData))
                        {
                            // Add a new picture shape with the updated image
                            IPicture newPicture = slide.Pictures.AddPicture(imageStream, 60, 120, 300, 200);
                        }
                    }

                    // Find the description shape by its id
                    shape = FindShapeByName(slide, "txtVillaDescription");
                    if (shape != null)
                        shape.TextBody.Text = villa.Description;

                    // Find the amenities shape by its id
                    shape = FindShapeByName(slide, "txtVillaAmenities");
                    if (shape != null)
                    {
                        // Define the list items
                        List<string> listItems = villa.VillaAmenity.Select(x => x.Name).ToList();
                        // Clear the existing text content of the textbox
                        shape.TextBody.Text = string.Empty;

                        // Add each list item as a separate paragraph in the textbox
                        foreach (string listItem in listItems)
                        {
                            // Add a new paragraph
                            IParagraph paragraph = shape.TextBody.AddParagraph();

                            // Add the list item as a text part in the paragraph
                            ITextPart textPart = paragraph.AddTextPart(listItem);

                            // Set the bullet style for the list item
                            paragraph.ListFormat.Type = ListType.Bulleted;
                            paragraph.ListFormat.BulletCharacter = '\u2022'; // Bullet character (Unicode code point)

                            //// Set the font properties for the list item
                            textPart.Font.FontName = "system-ui";
                            textPart.Font.FontSize = 18;

                            textPart.Font.Color = ColorObject.FromArgb(144, 148, 152);
                        }
                        shape.TextBody.Text = shape.TextBody.Text.TrimStart();
                    }

                    // Find the occupancy shape by its id
                    shape = FindShapeByName(slide, "txtOccupancy");
                    if (shape != null)
                        shape.TextBody.Text = string.Format("Max Occupancy : {0} adults", villa.Occupancy);

                    // Find the size shape by its id
                    shape = FindShapeByName(slide, "txtVillaSize");
                    if (shape != null)
                        shape.TextBody.Text = string.Format("Villa Size: {0} sqft", villa.Sqft);

                    // Find the price shape by its id
                    shape = FindShapeByName(slide, "txtPricePerNight");
                    if (shape != null)
                        shape.TextBody.Text = string.Format("USD {0}/night", villa.Price.ToString("C"));
                }

                // Create a memory stream to hold the modified presentation
                MemoryStream memoryStream = new MemoryStream();
                // Save the modified presentation to the memory stream
                presentation.Save(memoryStream);
                memoryStream.Position = 0;
                return File(memoryStream, "application/vnd.openxmlformats-officedocument.presentationml.presentation", "villa.pptx");
            }
        }

        public IActionResult Details(int villaId)
        {
            DetailsVM detailsVM = new()
            {
                Villa = _unitOfWork.Villa.Get(u => u.Id == villaId),
                Nights = 1
            };
            return View(detailsVM);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }

        #region PPT Library
        private string GetImageContentType(string imageUrl)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(imageUrl);
            request.Method = "HEAD";

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                return response.ContentType;
            }
        }

        private byte[] ConvertSvgToPng(byte[] svgImageData)
        {
            // Load the SVG image data into an SVG document
            string svgString = System.Text.Encoding.UTF8.GetString(svgImageData);
            SvgDocument svgDocument = SvgDocument.FromSvg<SvgDocument>(svgString);

            // Set the size of the rendered image
            int width = 600;
            int height = 400;

            // Render the SVG image to a bitmap
            Bitmap bitmap = svgDocument.Draw(width, height);

            // Convert the bitmap to a PNG byte array
            MemoryStream memoryImageStream = new MemoryStream();
            bitmap.Save(memoryImageStream, System.Drawing.Imaging.ImageFormat.Png);
            return memoryImageStream.ToArray();
        }

        private IShape FindShapeByName(ISlide slide, string shapeName)
        {
            foreach (IShape shape in slide.Shapes)
            {
                // Check if the shape has the specified name
                if (shape.ShapeName == shapeName)
                {
                    return shape;
                }

                // Recursively search inside group shapes
                if (shape is IGroupShape groupShape)
                {
                    IShape foundShape = FindShapeByName(groupShape, shapeName);
                    if (foundShape != null)
                    {
                        return foundShape;
                    }
                }
            }

            return null; // Shape with the specified name not found
        }

        private IShape FindShapeByName(IGroupShape groupShape, string shapeName)
        {
            foreach (IShape shape in groupShape.Shapes)
            {
                // Check if the shape has the specified name
                if (shape.ShapeName == shapeName)
                {
                    return shape;
                }

                // Recursively search inside nested group shapes
                if (shape is IGroupShape nestedGroupShape)
                {
                    IShape foundShape = FindShapeByName(nestedGroupShape, shapeName);
                    if (foundShape != null)
                    {
                        return foundShape;
                    }
                }
            }

            return null; // Shape with the specified name not found
        }
        #endregion
    }
}