
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class DocGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // DANH SÁCH ĐỘC GIẢ
        public async Task<IActionResult> Index()
        {
            var danhSach = await _context.DocGias
                .AsNoTracking()
                .OrderBy(d => d.MaDocGia)
                .ToListAsync();

            return View(danhSach);
        }

        // HIỂN THỊ FORM THÊM ĐỘC GIẢ
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachTaiKhoan();

            var docGia = new DocGia
            {
                NgayDangKy = DateTime.Today,
                TrangThai = "Hoạt động"
            };

            return View(docGia);
        }

        // LƯU ĐỘC GIẢ MỚI
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai")]
            DocGia docGia)
        {
            // Kiểm tra tài khoản đã được liên kết với độc giả khác chưa
            bool taiKhoanDaSuDung = await _context.DocGias
                .AnyAsync(d => d.MaTaiKhoan == docGia.MaTaiKhoan);

            if (taiKhoanDaSuDung)
            {
                ModelState.AddModelError(
                    nameof(docGia.MaTaiKhoan),
                    "Tài khoản này đã được liên kết với một độc giả khác.");
            }

            if (string.IsNullOrWhiteSpace(docGia.TrangThai))
            {
                docGia.TrangThai = "Hoạt động";
            }

            if (ModelState.IsValid)
            {
                _context.DocGias.Add(docGia);
                await _context.SaveChangesAsync();

                TempData["ThongBao"] = "Thêm độc giả thành công!";
                return RedirectToAction(nameof(Index));
            }

            // Nạp lại danh sách tài khoản nếu dữ liệu không hợp lệ
            await NapDanhSachTaiKhoan(docGia.MaTaiKhoan);

            return View(docGia);
        }

        // XEM CHI TIẾT ĐỘC GIẢ
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGias
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // HIỂN THỊ FORM SỬA ĐỘC GIẢ
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGias.FindAsync(id);

            if (docGia == null)
            {
                return NotFound();
            }

            await NapDanhSachTaiKhoan(docGia.MaTaiKhoan);

            return View(docGia);
        }

        // LƯU THAY ĐỔI ĐỘC GIẢ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaDocGia,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai")]
            DocGia docGia)
        {
            if (id != docGia.MaDocGia)
            {
                return NotFound();
            }

            bool taiKhoanDaSuDung = await _context.DocGias
                .AnyAsync(d => d.MaTaiKhoan == docGia.MaTaiKhoan
                            && d.MaDocGia != docGia.MaDocGia);

            if (taiKhoanDaSuDung)
            {
                ModelState.AddModelError(
                    nameof(docGia.MaTaiKhoan),
                    "Tài khoản này đã được liên kết với một độc giả khác.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.DocGias.Update(docGia);
                    await _context.SaveChangesAsync();

                    TempData["ThongBao"] = "Cập nhật độc giả thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    bool tonTai = await _context.DocGias
                        .AnyAsync(d => d.MaDocGia == docGia.MaDocGia);

                    if (!tonTai)
                    {
                        return NotFound();
                    }

                    throw;
                }
            }

            await NapDanhSachTaiKhoan(docGia.MaTaiKhoan);

            return View(docGia);
        }

        // NẠP DANH SÁCH TÀI KHOẢN CHƯA ĐƯỢC LIÊN KẾT
        private async Task NapDanhSachTaiKhoan(
            int? taiKhoanDaChon = null)
        {
            var taiKhoanDaSuDung = await _context.DocGias
                .AsNoTracking()
                .Select(d => d.MaTaiKhoan)
                .ToListAsync();

            var danhSachTaiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .Where(t => !taiKhoanDaSuDung.Contains(t.MaTaiKhoan)
                         || (taiKhoanDaChon.HasValue
                             && t.MaTaiKhoan == taiKhoanDaChon.Value))
                .OrderBy(t => t.MaTaiKhoan)
                .ToListAsync();

            ViewBag.MaTaiKhoan = new SelectList(
                danhSachTaiKhoan,
                "MaTaiKhoan",
                "MaTaiKhoan",
                taiKhoanDaChon);
        }
    }
}
