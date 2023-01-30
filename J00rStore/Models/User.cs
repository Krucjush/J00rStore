using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace J00rStore.Models
{
	public class User : IdentityUser
	{
		[Required]
		[DisplayName("First Name")]
		public string FirstName { get; set; }
		[Required]
		public string Surname { get; set; }
		public string? Street { get; set; }
		public string? City { get; set; }
		public string? State { get; set; }
		[DisplayName("Zip Code")]
		public string? ZipCode { get; set; }
	}
}
