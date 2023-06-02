using System.ComponentModel.DataAnnotations;

namespace WhiteLagoon.Models
{
    public class Villa
    {
        [Key]
        public int Id { get; set; }

        public required string Name { get; set; }
        public string? Description { get; set; }
        [Display(Name = "Price per night")]
        public double Price { get; set; }
        public int Sqft { get; set; }
        public int Occupancy { get; set; }
        [Display(Name = "Image Url")]
        public required string ImageUrl { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }
}
