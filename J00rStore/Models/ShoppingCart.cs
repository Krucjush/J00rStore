using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace J00rStore.Models
{
    public class ShoppingCart
    {
        public int Id { get; set; }

        public int? ProductId { get; set; }

        [ValidateNever]
        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        [Range(1, 999, ErrorMessage = "Please enter a value between 1 and 999")]
        public int Count { get; set; }

        public string? UserId { get; set; }

		[NotMapped]
        public double Price { get; set; }
    }
}
