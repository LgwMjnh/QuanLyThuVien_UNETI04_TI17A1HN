using Microsoft.AspNetCore.Mvc;
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

        // GET: DocGia Mục 7.1: Xem danh sách và tìm kiếm độc giả
        // Vẫn chưa cần đăng nhập
        public async Task<IActionResult> Index(string? keyword, string? trangThai)
        {
            var query = _context.DocGias
                .Include(d => d.TaiKhoan)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                keyword = keyword.Trim();
                query = query.Where(d =>
                    d.HoTen.Contains(keyword) ||
                    (d.Email != null && d.Email.Contains(keyword)) ||
                    (d.SoDienThoai != null && d.SoDienThoai.Contains(keyword)) ||
                    (d.TaiKhoan != null && d.TaiKhoan.TenDangNhap.Contains(keyword)));
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

        // GET: DocGia/Details/5 Mục 7.1: Xem chi tiết độc giả & danh sách phiếu mượn của độc giả đó
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .Include(d => d.PhieuMuons)
                    .ThenInclude(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // GET: DocGia/Create Tạo mới độc giả để dễ dàng nạp dữ liệu kiểm thử
        public IActionResult Create()
        {
            return View(new DocGia
            {
                NgayDangKy = DateTime.Today,
                TrangThai = "Hoạt động"
            });
        }

        // POST: DocGia/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DocGia docGia, string? tenDangNhap, string? matKhau)
        {
            if (docGia.NgaySinh.HasValue && docGia.NgaySinh.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(DocGia.NgaySinh), "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            // Nếu người dùng nhập tên đăng nhập thì kiểm tra trùng
            if (!string.IsNullOrWhiteSpace(tenDangNhap))
            {
                var tonTaiTk = await _context.TaiKhoans.AnyAsync(t => t.TenDangNhap == tenDangNhap.Trim());
                if (tonTaiTk)
                {
                    ModelState.AddModelError("tenDangNhap", "Tên đăng nhập này đã tồn tại, vui lòng chọn tên khác.");
                }
            }

            if (ModelState.IsValid)
            {
                // Tự động tạo tài khoản đi kèm với độc giả nếu chưa có
                var username = string.IsNullOrWhiteSpace(tenDangNhap)
                    ? "docgia_" + DateTime.Now.Ticks.ToString()[^6..]
                    : tenDangNhap.Trim();

                var pass = string.IsNullOrWhiteSpace(matKhau) ? "123456" : matKhau;

                var taiKhoan = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhau = pass,
                    HoTen = docGia.HoTen.Trim(),
                    VaiTro = "DocGia",
                    TrangThai = (docGia.TrangThai == "Hoạt động")
                };

                _context.TaiKhoans.Add(taiKhoan);
                await _context.SaveChangesAsync();

                docGia.MaTaiKhoan = taiKhoan.MaTaiKhoan;
                docGia.NgayDangKy = DateTime.Today;

                _context.DocGias.Add(docGia);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Thêm độc giả '{docGia.HoTen}' thành công! (Tài khoản: {username})";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TenDangNhap = tenDangNhap;
            return View(docGia);
        }

        // GET: DocGia/Edit/5 Mục 7.1: Cập nhật thông tin cá nhân được phép
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
            {
                return NotFound();
            }

            return View(docGia);
        }

        // POST: DocGia/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MaDocGia,HoTen,NgaySinh,GioiTinh,SoDienThoai,Email,DiaChi")] DocGia input)
        {
            if (id != input.MaDocGia)
            {
                return NotFound();
            }

            if (input.NgaySinh.HasValue && input.NgaySinh.Value.Date > DateTime.Today)
            {
                ModelState.AddModelError(nameof(DocGia.NgaySinh), "Ngày sinh không được lớn hơn ngày hiện tại.");
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                docGia.HoTen = input.HoTen.Trim();
                docGia.NgaySinh = input.NgaySinh;
                docGia.GioiTinh = input.GioiTinh;
                docGia.SoDienThoai = string.IsNullOrWhiteSpace(input.SoDienThoai) ? null : input.SoDienThoai.Trim();
                docGia.Email = string.IsNullOrWhiteSpace(input.Email) ? null : input.Email.Trim();
                docGia.DiaChi = string.IsNullOrWhiteSpace(input.DiaChi) ? null : input.DiaChi.Trim();

                // Đồng bộ tên vào bảng TaiKhoan
                if (docGia.TaiKhoan != null)
                {
                    docGia.TaiKhoan.HoTen = docGia.HoTen;
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cập nhật thông tin độc giả thành công!";
                return RedirectToAction(nameof(Details), new { id = docGia.MaDocGia });
            }

            return View(docGia);
        }

        // POST: DocGia/CapNhatTrangThai Mục 7.1: Cập nhật trạng thái Hoạt động / Khóa
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai)
        {
            var hopLe = trangThai == "Hoạt động" || trangThai == "Khóa";
            if (!hopLe)
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            var docGia = await _context.DocGias
                .Include(d => d.TaiKhoan)
                .FirstOrDefaultAsync(d => d.MaDocGia == id);

            if (docGia == null)
            {
                return NotFound();
            }

            docGia.TrangThai = trangThai;

            // Đồng bộ khóa/mở tài khoản đăng nhập tương ứng
            if (docGia.TaiKhoan != null)
            {
                docGia.TaiKhoan.TrangThai = (trangThai == "Hoạt động");
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật trạng thái độc giả thành '{trangThai}'.";
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}

