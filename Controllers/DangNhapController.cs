using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;


namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DangNhapController(ApplicationDbContext context)
        {
            _context = context;

        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(DangNhap model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var taiKhoan = await _context.TaiKhoans
                .FirstOrDefaultAsync(tk =>
                    tk.TenDangNhap == model.TenDangNhap);

            if (taiKhoan == null || taiKhoan.MatKhau != model.MatKhau)
            {
                ModelState.AddModelError(string.Empty,
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }

            if (!taiKhoan.TrangThai)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản đã bị khóa.");

                return View(model);
            }

            HttpContext.Session.SetInt32(
                "MaTaiKhoan", taiKhoan.MaTaiKhoan);

            HttpContext.Session.SetString(
                "HoTen", taiKhoan.HoTen);

            HttpContext.Session.SetString(
                "VaiTro", taiKhoan.VaiTro);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
    }
}