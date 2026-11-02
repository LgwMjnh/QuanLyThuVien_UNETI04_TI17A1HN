using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.Helpers;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class DocGiaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DocGiaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem danh sách và tìm kiếm độc giả (Admin)
        public async Task<IActionResult> Index(string? keyword, string? trangThai)
        {
            var query = _context.DocGias
                .Include(d => d.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();

                // Hỗ trợ tìm kiếm theo họ tên, email, SĐT, username hoặc mã độc giả (DG001, DG1, 1)
                int maTimKiem = 0;
                string normalized = keyword.ToUpper();
                if (normalized.StartsWith("DG"))
                {
                    int.TryParse(normalized.Substring(2), out maTimKiem);
                }
                else if (int.TryParse(normalized, out int parsedDirect) && normalized.Length <= 8)
                {
                    maTimKiem = parsedDirect;
                }

                query = query.Where(d =>
                    d.HoTen.Contains(keyword) ||
                    (d.Email != null && d.Email.Contains(keyword)) ||
                    (d.SoDienThoai != null && d.SoDienThoai.Contains(keyword)) ||
                    (d.TaiKhoan != null && d.TaiKhoan.TenDangNhap.Contains(keyword)) ||
                    (maTimKiem > 0 && d.MaDocGia == maTimKiem));
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(d => d.TrangThai == trangThai);
            }

            ViewBag.Keyword = keyword;
            ViewBag.TrangThai = trangThai;

            var danhSach = await query
                .OrderByDescending(d => d.NgayDangKy)
                .ToListAsync();

            return View(danhSach);
        }

        // Xem chi tiết độc giả và lịch sử mượn (Admin)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .Include(d => d.PhieuMuons)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
                return NotFound();

            return View(docGia);
        }

        // Thêm độc giả mới (Admin)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachTaiKhoan();

            return View(new DocGia
            {
                NgayDangKy = DateTime.Today,
                TrangThai = "Hoạt động"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai")]
            DocGia docGia,
            string? tenDangNhap,
            string? matKhau)
        {
            if (docGia.NgaySinh.HasValue &&
                docGia.NgaySinh.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(docGia.NgaySinh),
                    "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            if (docGia.MaTaiKhoan > 0)
            {
                bool taiKhoanTonTai = await _context.TaiKhoans
                    .AnyAsync(t => t.MaTaiKhoan == docGia.MaTaiKhoan);

                if (!taiKhoanTonTai)
                {
                    ModelState.AddModelError(
                        nameof(docGia.MaTaiKhoan),
                        "Tài khoản được chọn không tồn tại.");
                }

                bool taiKhoanDaSuDung = await _context.DocGias
                    .AnyAsync(d => d.MaTaiKhoan == docGia.MaTaiKhoan);

                if (taiKhoanDaSuDung)
                {
                    ModelState.AddModelError(
                        nameof(docGia.MaTaiKhoan),
                        "Tài khoản này đã được liên kết với độc giả khác.");
                }
            }
            else if (!string.IsNullOrWhiteSpace(tenDangNhap))
            {
                tenDangNhap = tenDangNhap.Trim();

                bool trungTen = await _context.TaiKhoans
                    .AnyAsync(t => t.TenDangNhap == tenDangNhap);

                if (trungTen)
                {
                    ModelState.AddModelError(
                        nameof(tenDangNhap),
                        "Tên đăng nhập này đã tồn tại.");
                }
            }

            if (string.IsNullOrWhiteSpace(docGia.HoTen))
            {
                ModelState.AddModelError(
                    nameof(docGia.HoTen),
                    "Họ tên không được để trống.");
            }

            if (ModelState.IsValid)
            {
                docGia.HoTen = docGia.HoTen.Trim();
                docGia.NgayDangKy = DateTime.Today;

                if (string.IsNullOrWhiteSpace(docGia.TrangThai))
                    docGia.TrangThai = "Hoạt động";

                // Nếu chưa chọn tài khoản thì tự tạo tài khoản mới
                if (docGia.MaTaiKhoan <= 0)
                {
                    string username = string.IsNullOrWhiteSpace(tenDangNhap)
                        ? "docgia_" + Guid.NewGuid().ToString("N")[..10]
                        : tenDangNhap.Trim();

                    bool trungTen = await _context.TaiKhoans
                        .AnyAsync(t => t.TenDangNhap == username);

                    if (trungTen)
                    {
                        ModelState.AddModelError(
                            nameof(tenDangNhap),
                            "Tên đăng nhập này đã tồn tại.");

                        ViewBag.TenDangNhap = tenDangNhap;
                        await NapDanhSachTaiKhoan();
                        return View(docGia);
                    }

                    var taiKhoan = new TaiKhoan
                    {
                        TenDangNhap = username,
                        MatKhau = string.IsNullOrWhiteSpace(matKhau)
                            ? "123456"
                            : matKhau,
                        HoTen = docGia.HoTen,
                        VaiTro = "DocGia",
                        TrangThai = docGia.TrangThai == "Hoạt động"
                    };

                    _context.TaiKhoans.Add(taiKhoan);
                    await _context.SaveChangesAsync();

                    docGia.MaTaiKhoan = taiKhoan.MaTaiKhoan;
                }

                _context.DocGias.Add(docGia);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Thêm độc giả '{docGia.HoTen}' thành công!";

                TempData["ThongBao"] =
                    $"Thêm độc giả '{docGia.HoTen}' thành công!";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.TenDangNhap = tenDangNhap;
            await NapDanhSachTaiKhoan(docGia.MaTaiKhoan);

            return View(docGia);
        }

        // Sửa thông tin độc giả (Admin)
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
                return NotFound();

            await NapDanhSachTaiKhoan(docGia.MaTaiKhoan);

            return View(docGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaDocGia,MaTaiKhoan,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi,NgayDangKy,TrangThai")]
            DocGia input)
        {
            if (id != input.MaDocGia)
                return NotFound();

            if (input.NgaySinh.HasValue &&
                input.NgaySinh.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(
                    nameof(input.NgaySinh),
                    "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            bool taiKhoanDaSuDung = await _context.DocGias
                .AnyAsync(d =>
                    d.MaTaiKhoan == input.MaTaiKhoan &&
                    d.MaDocGia != input.MaDocGia);

            if (taiKhoanDaSuDung)
            {
                ModelState.AddModelError(
                    nameof(input.MaTaiKhoan),
                    "Tài khoản này đã được liên kết với độc giả khác.");
            }

            if (ModelState.IsValid)
            {
                var docGia = await _context.DocGias
                    .Include(d => d.TaiKhoan)
                    .FirstOrDefaultAsync(d => d.MaDocGia == id);

                if (docGia == null)
                    return NotFound();

                docGia.HoTen = input.HoTen.Trim();
                docGia.NgaySinh = input.NgaySinh;
                docGia.GioiTinh = input.GioiTinh;
                docGia.SoDienThoai =
                    string.IsNullOrWhiteSpace(input.SoDienThoai)
                        ? null
                        : input.SoDienThoai.Trim();
                docGia.Email =
                    string.IsNullOrWhiteSpace(input.Email)
                        ? null
                        : input.Email.Trim();
                docGia.DiaChi =
                    string.IsNullOrWhiteSpace(input.DiaChi)
                        ? null
                        : input.DiaChi.Trim();

                if (docGia.TaiKhoan != null)
                    docGia.TaiKhoan.HoTen = docGia.HoTen;

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "Cập nhật thông tin độc giả thành công!";

                return RedirectToAction(
                    nameof(Details),
                    new { id = docGia.MaDocGia });
            }

            var docGiaHienTai = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGiaHienTai == null)
                return NotFound();

            input.NgayDangKy = docGiaHienTai.NgayDangKy;
            input.TrangThai = docGiaHienTai.TrangThai;

            await NapDanhSachTaiKhoan(input.MaTaiKhoan);

            return View(input);
        }

        // Khóa / Mở thẻ độc giả (Admin)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(
            int id,
            string trangThai)
        {
            if (trangThai != "Hoạt động" && trangThai != "Khóa")
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
                return NotFound();

            docGia.TrangThai = trangThai;

            if (docGia.TaiKhoan != null)
            {
                docGia.TaiKhoan.TrangThai = trangThai == "Hoạt động";
            }

            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã cập nhật trạng thái độc giả thành '{trangThai}'.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // Độc giả xem hồ sơ cá nhân
        [HttpGet]
        public async Task<IActionResult> HoSo()
        {
            var maTaiKhoan = QuyenTruyCap.LayMaTaiKhoan(HttpContext);
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để xem thông tin cá nhân.";
                return RedirectToAction("Index", "DangNhap");
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .Include(d => d.PhieuMuons)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);

            if (docGia == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin hồ sơ độc giả tương ứng.";
                return RedirectToAction("Index", "Home");
            }

            return View(docGia);
        }

        // Độc giả cập nhật thông tin cá nhân
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoSo(
            [Bind("HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi")] DocGia input)
        {
            var maTaiKhoan = QuyenTruyCap.LayMaTaiKhoan(HttpContext);
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index", "DangNhap");
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);

            if (docGia == null)
            {
                return NotFound();
            }

            if (input.NgaySinh.HasValue && input.NgaySinh.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(input.NgaySinh), "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            if (string.IsNullOrWhiteSpace(input.HoTen))
            {
                ModelState.AddModelError(nameof(input.HoTen), "Họ tên không được để trống.");
            }

            if (ModelState.IsValid)
            {
                docGia.HoTen = input.HoTen.Trim();
                docGia.NgaySinh = input.NgaySinh;
                docGia.GioiTinh = input.GioiTinh;
                docGia.SoDienThoai = string.IsNullOrWhiteSpace(input.SoDienThoai) ? null : input.SoDienThoai.Trim();
                docGia.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
                docGia.DiaChi = string.IsNullOrWhiteSpace(input.DiaChi) ? null : input.DiaChi.Trim();

                if (docGia.TaiKhoan != null)
                {
                    docGia.TaiKhoan.HoTen = docGia.HoTen;
                    HttpContext.Session.SetString("HoTen", docGia.HoTen);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction(nameof(HoSo));
            }

            // Nạp lại các trường không cho sửa để View render đúng
            input.MaDocGia = docGia.MaDocGia;
            input.MaTaiKhoan = docGia.MaTaiKhoan;
            input.TaiKhoan = docGia.TaiKhoan;
            input.NgayDangKy = docGia.NgayDangKy;
            input.TrangThai = docGia.TrangThai;

            return View(input);
        }

        // Nạp danh sách tài khoản chưa liên kết
        private async Task NapDanhSachTaiKhoan(int? taiKhoanDaChon = null)
        {
            var taiKhoanDaSuDung = await _context.DocGias
                .AsNoTracking()
                .Where(d => d.MaTaiKhoan > 0)
                .Select(d => d.MaTaiKhoan)
                .ToListAsync();

            var danhSachTaiKhoan = await _context.TaiKhoans
                .AsNoTracking()
                .Where(t =>
                    !taiKhoanDaSuDung.Contains(t.MaTaiKhoan) ||
                    (taiKhoanDaChon.HasValue &&
                     t.MaTaiKhoan == taiKhoanDaChon.Value))
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
