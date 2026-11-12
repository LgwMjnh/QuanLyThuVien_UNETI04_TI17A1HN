
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using System.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.ViewModels;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class DangNhapController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<TaiKhoan> _passwordHasher;

        public DangNhapController(
            ApplicationDbContext context,
            IPasswordHasher<TaiKhoan> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
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

            if (taiKhoan == null)
            {
                ModelState.AddModelError(string.Empty,
                    "Tên đăng nhập hoặc mật khẩu không đúng.");

                return View(model);
            }

            if (taiKhoan.TrangThai == TrangThaiTaiKhoan.BiKhoa)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản đã bị khóa. Vui lòng liên hệ Admin.");

                return View(model);
            }

            if (taiKhoan.TrangThai == TrangThaiTaiKhoan.BiBan)
            {
                ModelState.AddModelError(string.Empty,
                    "Tài khoản đã bị ban. Vui lòng liên hệ Admin.");

                return View(model);
            }

            // Kiểm tra mật khẩu
            var ketQua = _passwordHasher.VerifyHashedPassword(
                taiKhoan,
                taiKhoan.MatKhau,
                model.MatKhau);

            bool dangNhapHopLe =
                ketQua != PasswordVerificationResult.Failed;

            // Tương thích với tài khoản cũ đang lưu mật khẩu dạng văn bản thuần
            if (!dangNhapHopLe)
            {
                dangNhapHopLe =
                    taiKhoan.MatKhau == model.MatKhau;
            }

            if (!dangNhapHopLe)
            {
                // Chỉ tự động khóa tài khoản Độc giả.
                if (string.Equals(
                    taiKhoan.VaiTro,
                    "DocGia",
                    StringComparison.OrdinalIgnoreCase))
                {
                    taiKhoan.SoLanDangNhapSai++;

                    if (taiKhoan.SoLanDangNhapSai >= 5)
                    {
                        taiKhoan.TrangThai = TrangThaiTaiKhoan.BiKhoa;

                        await _context.SaveChangesAsync();

                        ModelState.AddModelError(string.Empty,
                            "Bạn đã nhập sai mật khẩu 5 lần. Tài khoản đã bị khóa.");
                    }
                    else
                    {
                        await _context.SaveChangesAsync();

                        ModelState.AddModelError(string.Empty,
                            $"Mật khẩu không đúng. Bạn còn {5 - taiKhoan.SoLanDangNhapSai} lần thử.");
                    }
                }
                else
                {
                    // Admin không bị tự động khóa khi nhập sai mật khẩu.
                    ModelState.AddModelError(string.Empty,
                        "Mật khẩu không đúng.");
                }

                return View(model);
            }

            // Nếu mật khẩu cũ là văn bản thuần hoặc cần nâng cấp,
            // mã hóa lại bằng PasswordHasher.
            if (ketQua == PasswordVerificationResult.Failed ||
                ketQua == PasswordVerificationResult.SuccessRehashNeeded)
            {
                taiKhoan.MatKhau = _passwordHasher.HashPassword(
                    taiKhoan,
                    model.MatKhau);
            }

            // Đăng nhập thành công: đặt lại số lần sai.
            if (taiKhoan.SoLanDangNhapSai > 0)
            {
                taiKhoan.SoLanDangNhapSai = 0;
            }

            await _context.SaveChangesAsync();

            HttpContext.Session.SetInt32(
                "MaTaiKhoan", taiKhoan.MaTaiKhoan);

            HttpContext.Session.SetString(
                "HoTen", taiKhoan.HoTen);

            HttpContext.Session.SetString(
                "VaiTro", taiKhoan.VaiTro);

            return RedirectToAction("Index", "TrangChu");
        }

        // Hiển thị trang đăng ký tài khoản Độc giả
        [HttpGet]
        public IActionResult DangKy()
        {
            return View(new DangKyViewModel());
        }

        // Xử lý đăng ký tài khoản Độc giả
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            // Chuẩn hóa dữ liệu trước khi kiểm tra và lưu
            model.TenDangNhap = model.TenDangNhap?.Trim() ?? string.Empty;
            model.HoTen = model.HoTen?.Trim() ?? string.Empty;
            model.Email = string.IsNullOrWhiteSpace(model.Email)
                ? null : model.Email.Trim();
            model.SoDienThoai = string.IsNullOrWhiteSpace(model.SoDienThoai)
                ? null : model.SoDienThoai.Trim();
            model.GioiTinh = string.IsNullOrWhiteSpace(model.GioiTinh)
                ? null : model.GioiTinh.Trim();
            model.DiaChi = string.IsNullOrWhiteSpace(model.DiaChi)
                ? null : model.DiaChi.Trim();

            // Kiểm tra tên đăng nhập đã tồn tại
            bool trungTenDangNhap = await _context.TaiKhoans
                .AnyAsync(tk => tk.TenDangNhap == model.TenDangNhap);

            if (trungTenDangNhap)
            {
                ModelState.AddModelError(
                    nameof(model.TenDangNhap),
                    "Tên đăng nhập này đã tồn tại.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Transaction đảm bảo tài khoản và hồ sơ độc giả
            // được tạo đồng thời.
            await using var transaction =
                await _context.Database.BeginTransactionAsync(
                    IsolationLevel.Serializable);

            try
            {
                // Kiểm tra lại tên đăng nhập trong Transaction
                trungTenDangNhap = await _context.TaiKhoans
                    .AnyAsync(tk => tk.TenDangNhap == model.TenDangNhap);

                if (trungTenDangNhap)
                {
                    await transaction.RollbackAsync();

                    ModelState.AddModelError(
                        nameof(model.TenDangNhap),
                        "Tên đăng nhập này đã tồn tại.");

                    return View(model);
                }

                // Tạo tài khoản
                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = model.TenDangNhap,
                    HoTen = model.HoTen,
                    VaiTro = "DocGia",
                    TrangThai = TrangThaiTaiKhoan.BinhThuong,
                    SoLanDangNhapSai = 0
                };

                // Mã hóa mật khẩu
                taiKhoan.MatKhau = _passwordHasher.HashPassword(
                    taiKhoan,
                    model.MatKhau);

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                // Tạo hồ sơ độc giả liên kết với tài khoản
                var docGia = new DocGia
                {
                    MaTaiKhoan = taiKhoan.MaTaiKhoan,
                    HoTen = model.HoTen,
                    NgaySinh = model.NgaySinh,
                    GioiTinh = model.GioiTinh,
                    SoDienThoai = model.SoDienThoai,
                    Email = model.Email,
                    DiaChi = model.DiaChi,
                    NgayDangKy = DateTime.Today,
                    TrangThai = "Hoạt động"
                };

                _context.DocGias.Add(docGia);
                await _context.SaveChangesAsync();

                // Hoàn tất đăng ký
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    "Đăng ký tài khoản thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Không thể đăng ký tài khoản. Vui lòng kiểm tra lại thông tin.");

                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DangXuat()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "TrangChu");
        }
    }
}