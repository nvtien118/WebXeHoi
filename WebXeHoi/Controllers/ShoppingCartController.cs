using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WebXeHoi.DataAccess;
using WebXeHoi.Helpers;
using WebXeHoi.Models;
using WebXeHoi.Repositories;

namespace WebXeHoi.Controllers
{
	
	public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
		private readonly ApplicationDbContext _context;
		private readonly UserManager<ApplicationUser> _userManager;
		public ShoppingCartController(IProductRepository productRepository, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
			_context = context;
			_userManager = userManager;
		}
        public async Task<IActionResult> AddToCart(int productId, int quantity)
        {
            // Giả sử bạn có phương thức lấy thông tin sản phẩm từ productId
            var product = await GetProductFromDatabase(productId);
            var cartItem = new CartItem
            {
                ProductId = productId,
                Name = product.Name,
                Price = product.Price,
                Quantity = quantity
            };
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ??
            new ShoppingCart();
            cart.AddItem(cartItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);
            return RedirectToAction("Index");
        }
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
			// Lấy giỏ hàng từ session
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

			// Tìm sản phẩm trong giỏ hàng và cập nhật số lượng
			var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
			if (item != null)
			{
				item.Quantity = quantity;
			}

			// Lưu lại giỏ hàng vào session
			HttpContext.Session.SetObjectAsJson("Cart", cart);

			// Quay lại trang giỏ hàng
			return RedirectToAction("Index");
		}
		private async Task<Product> GetProductFromDatabase(int productId)
        {
            // Truy vấn cơ sở dữ liệu để lấy thông tin sản phẩm
            var product = await _productRepository.GetByIdAsync(productId);
            return product;
        }
		public IActionResult RemoveFromCart(int productId)
		{
			var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
			if (cart is not null)
			{
				cart.RemoveItem(productId);

				if (cart.Items.Count == 0)
				{
					HttpContext.Session.Remove("Cart");
					return RedirectToAction("Index", "Product");
				}

				HttpContext.Session.SetObjectAsJson("Cart", cart);
			}

			return RedirectToAction("Index");
		}
		[Authorize]
		public IActionResult Checkout()
		{
			return View(new Order());
		}
		[Authorize]
		[HttpPost]
		public async Task<IActionResult> Checkout(Order order)
		{
			var cart =
			HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
			if (cart == null || !cart.Items.Any())
			{
				// Xử lý giỏ hàng trống...
				return RedirectToAction("Index");
			}
			var user = await _userManager.GetUserAsync(User);
			order.UserId = user.Id;
			order.OrderDate = DateTime.UtcNow;
			order.TotalPrice = cart.Items.Sum(i => i.Price *
			i.Quantity);
			order.OrderDetails = cart.Items.Select(i => new OrderDetail
			{
				ProductId = i.ProductId,
				Quantity = i.Quantity,
				Price = i.Price
			}).ToList();
            // Lưu đơn hàng và chi tiết đơn hàng
			_context.Orders.Add(order);
			await _context.SaveChangesAsync();
			HttpContext.Session.Remove("Cart");
			return View("OrderCompleted", order.Id); // Trang xác nhận hoàn thành đơn hàng
		}
	}
}
