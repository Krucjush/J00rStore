using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations.Schema;
using J00rStore.Data;
using Microsoft.EntityFrameworkCore;

namespace J00rStore.Models
{
	public class OrderHeader
	{
		public int Id { get; set; }

		public string ApplicationUserId { get; set; }
		[ValidateNever]
		[ForeignKey("ApplicationUserId")]
		public User User { get; set; }

		[Required]
		public DateTime OrderDate { get; set; }
		public DateTime ShippingDate { get; set; }
		public double TotalPrice { get; set; }
		public string OrderStatus { get; set; }
		public string PaymentStatus { get; set; }
		public DateTime PaymentDate { get; set; }

		public string SessionId { get; set; }
		public string PaymentIntentId { get; set; }

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
		public string PostalCode { get; set; }

	}
}
