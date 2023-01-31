namespace J00rStore.Models.ViewModels
{
	public class ShoppingCartViewModel
	{
		public IEnumerable<ShoppingCart> ListCart { get; set; }
		public Order Order { get; set; }
	}
}
