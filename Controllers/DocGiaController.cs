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

        // Tiện ích: Tạo nhanh dữ liệu mẫu phục vụ kiểm thử Mục 7.1, 7.2, 7.3
        public async Task<IActionResult> SeedDuLieuMau()
        {
            // 1. Kiểm tra / thêm thể loại mẫu nếu chưa có
            if (!await _context.TheLoais.AnyAsync())
            {
                var tl1 = new TheLoai { TenTheLoai = "Công nghệ thông tin", MoTa = "Sách lập trình, phần mềm", TrangThai = true };
                var tl2 = new TheLoai { TenTheLoai = "Kinh tế - Quản trị", MoTa = "Sách tài chính, quản trị kinh doanh", TrangThai = true };
                var tl3 = new TheLoai { TenTheLoai = "Văn học nghệ thuật", MoTa = "Tiểu thuyết, truyện ngắn", TrangThai = true };
                _context.TheLoais.AddRange(tl1, tl2, tl3);
                await _context.SaveChangesAsync();
            }

            var theLoaiCntt = await _context.TheLoais.FirstOrDefaultAsync();

            // 2. Kiểm tra / thêm sách mẫu nếu chưa có
            if (!await _context.Sachs.AnyAsync() && theLoaiCntt != null)
            {
                var s1 = new Sach
                {
                    TenSach = "Lập trình C# 10 và .NET",
                    MaTheLoai = theLoaiCntt.MaTheLoai,
                    TacGia = "Nguyễn Văn A",
                    NhaXuatBan = "NXB Giáo Dục",
                    NamXuatBan = 2024,
                    SoLuong = 10,
                    SoLuongCon = 8,
                    GiaSach = 95000,
                    TrangThai = "Đang phục vụ",
                    MoTa = "Giáo trình C# cơ bản và nâng cao"
                };
                var s2 = new Sach
                {
                    TenSach = "Lập trình ASP.NET Core MVC",
                    MaTheLoai = theLoaiCntt.MaTheLoai,
                    TacGia = "Trần Thị B",
                    NhaXuatBan = "NXB Thông Tin và Truyền Thông",
                    NamXuatBan = 2023,
                    SoLuong = 5,
                    SoLuongCon = 4,
                    GiaSach = 120000,
                    TrangThai = "Đang phục vụ",
                    MoTa = "Kiến trúc MVC và EF Core"
                };
                var s3 = new Sach
                {
                    TenSach = "Cấu trúc dữ liệu và giải thuật",
                    MaTheLoai = theLoaiCntt.MaTheLoai,
                    TacGia = "Lê Văn C",
                    NhaXuatBan = "NXB Khoa Học Kỹ Thuật",
                    NamXuatBan = 2022,
                    SoLuong = 15,
                    SoLuongCon = 15,
                    GiaSach = 80000,
                    TrangThai = "Đang phục vụ",
                    MoTa = "Các thuật toán cơ bản"
                };
                _context.Sachs.AddRange(s1, s2, s3);
                await _context.SaveChangesAsync();
            }

            // 3. Kiểm tra / thêm độc giả mẫu nếu chưa có
            if (!await _context.DocGias.AnyAsync())
            {
                var tk1 = new TaiKhoan { TenDangNhap = "docgia01", MatKhau = "123456", HoTen = "Trần Văn Nam", VaiTro = "DocGia", TrangThai = true };
                var tk2 = new TaiKhoan { TenDangNhap = "docgia02", MatKhau = "123456", HoTen = "Lê Thị Mai", VaiTro = "DocGia", TrangThai = true };
                var tk3 = new TaiKhoan { TenDangNhap = "docgia03", MatKhau = "123456", HoTen = "Phạm Hoàng Long", VaiTro = "DocGia", TrangThai = true };
                _context.TaiKhoans.AddRange(tk1, tk2, tk3);
                await _context.SaveChangesAsync();

                var dg1 = new DocGia
                {
                    MaTaiKhoan = tk1.MaTaiKhoan,
                    HoTen = tk1.HoTen,
                    NgaySinh = new DateTime(2003, 5, 12),
                    GioiTinh = "Nam",
                    SoDienThoai = "0912345678",
                    Email = "nam.tv@gmail.com",
                    DiaChi = "Hà Nội",
                    NgayDangKy = DateTime.Today.AddMonths(-2),
                    TrangThai = "Hoạt động"
                };
                var dg2 = new DocGia
                {
                    MaTaiKhoan = tk2.MaTaiKhoan,
                    HoTen = tk2.HoTen,
                    NgaySinh = new DateTime(2004, 8, 20),
                    GioiTinh = "Nữ",
                    SoDienThoai = "0987654321",
                    Email = "mai.lt@gmail.com",
                    DiaChi = "Hà Nam",
                    NgayDangKy = DateTime.Today.AddMonths(-1),
                    TrangThai = "Hoạt động"
                };
                var dg3 = new DocGia
                {
                    MaTaiKhoan = tk3.MaTaiKhoan,
                    HoTen = tk3.HoTen,
                    NgaySinh = new DateTime(2002, 11, 5),
                    GioiTinh = "Nam",
                    SoDienThoai = "0901234567",
                    Email = "long.ph@gmail.com",
                    DiaChi = "Nam Định",
                    NgayDangKy = DateTime.Today.AddDays(-10),
                    TrangThai = "Hoạt động"
                };
                _context.DocGias.AddRange(dg1, dg2, dg3);
                await _context.SaveChangesAsync();

                // 4. Thêm phiếu mượn mẫu (Mục 7.2) và chi tiết phiếu mượn gồm nhiều sách (Mục 7.3)
                var sachList = await _context.Sachs.ToListAsync();
                if (sachList.Count >= 2)
                {
                    // Phiếu 1: Đang mượn (gồm 2 cuốn sách)
                    var pm1 = new PhieuMuon
                    {
                        MaDocGia = dg1.MaDocGia,
                        NgayMuon = DateTime.Today.AddDays(-5),
                        HanTra = DateTime.Today.AddDays(9),
                        TrangThai = "Đang mượn"
                    };
                    pm1.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon { MaSach = sachList[0].MaSach, SoLuongMuon = 1 });
                    pm1.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon { MaSach = sachList[1].MaSach, SoLuongMuon = 1 });

                    // Phiếu 2: Chờ xác nhận (gồm 1 cuốn sách)
                    var pm2 = new PhieuMuon
                    {
                        MaDocGia = dg2.MaDocGia,
                        NgayMuon = DateTime.Today,
                        HanTra = DateTime.Today.AddDays(14),
                        TrangThai = "Chờ xác nhận"
                    };
                    pm2.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon { MaSach = sachList[0].MaSach, SoLuongMuon = 1 });

                    // Phiếu 3: Đã trả (gồm 1 cuốn sách)
                    var pm3 = new PhieuMuon
                    {
                        MaDocGia = dg3.MaDocGia,
                        NgayMuon = DateTime.Today.AddDays(-20),
                        HanTra = DateTime.Today.AddDays(-6),
                        TrangThai = "Đã trả"
                    };
                    pm3.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                    {
                        MaSach = sachList[1].MaSach,
                        SoLuongMuon = 1,
                        NgayTra = DateTime.Today.AddDays(-7),
                        TinhTrangTra = "Sách nguyên vẹn, trả đúng hạn"
                    });

                    _context.PhieuMuons.AddRange(pm1, pm2, pm3);
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Đã nạp thành công dữ liệu mẫu (Độc giả, Sách, Phiếu mượn và Chi tiết phiếu mượn) để kiểm thử!";
            }
            else
            {
                TempData["Success"] = "Cơ sở dữ liệu đã có sẵn độc giả.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

