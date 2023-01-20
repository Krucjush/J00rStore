using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace J00rStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public int BrandId { get; set; }

        [ValidateNever]
        public Brand Brand { get; set; }

        [Required]
        public double Price { get; set; }

        [ValidateNever]
        public string ImageUrl { get; set; }
        [NotMapped]
        [ValidateNever]
        public IFormFile Image { get; set; }
    }
}
