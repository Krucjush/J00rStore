using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace J00rStore.Models
{
	public class Order
	{
		public int Id { get; set; }
		[ValidateNever]
		public IEnumerable<ShoppingCart> ShoppingCarts { get; set; }
		public int Count { get; set; }
		public string? UserId { get; set; }
		[Required] 
		public double TotalPrice { get; set; }
		[Required] 
		public string Name { get; set; }
		[Required] 
		public string PhoneNumber { get; set; }
		[Required] 
		public string Street { get; set; }
		[Required] 
		public string City { get; set; }
		[Required] 
		public string State { get; set; }
		[Required] 
		public string ZipCode { get; set; }
	}
}
