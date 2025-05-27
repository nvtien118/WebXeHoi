using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebXeHoi.Models
{
	public class Order
	{
		public int Id { get; set; }
		public string UserId { get; set; }
		public DateTime OrderDate { get; set; }
		public decimal TotalPrice { get; set; }
		[Required(ErrorMessage = "Shipping Address is required")]
		public string ShippingAddress { get; set; }
		[Required(ErrorMessage = "Please enter some notes")]
		public string Notes { get; set; }
		[ForeignKey("UserId")]
		[ValidateNever]
		public ApplicationUser ApplicationUser { get; set; }
		public List<OrderDetail> OrderDetails { get; set; }
	}
}
