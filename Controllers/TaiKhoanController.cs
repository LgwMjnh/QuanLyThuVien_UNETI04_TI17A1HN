using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.ViewModels;
using System.Security.Cryptography;
using System.Text;
using System.Data;

public class TaiKhoanController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IPasswordHasher<TaiKhoan> _passwordHasher;
    private readonly IConfiguration _configuration;

    public TaiKhoanController(
        ApplicationDbContext context,
        IPasswordHasher<TaiKhoan> passwordHasher,
        IConfiguration configuration)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
    }

    // Kiểm tra Admin đang đăng nhập từ Session và cơ sở dữ liệu.
    private async Task<TaiKhoan?> LayAdminDangNhapAsync()
    {
        int? maTaiKhoan = HttpContext.Session.GetInt32("MaTaiKhoan");

        if (maTaiKhoan == null)
        {
            return null;
        }

        return await _context.TaiKhoans.FirstOrDefaultAsync(tk =>
            tk.MaTaiKhoan == maTaiKhoan &&
            tk.VaiTro == "Admin" &&
            tk.TrangThai == TrangThaiTaiKhoan.BinhThuong);
    }

    // Kiểm tra hệ thống đã từng có Admin hay chưa.
    private async Task<bool> DaCoAdminAsync()
    {
        return await _context.TaiKhoans
            .AnyAsync(tk => tk.VaiTro == "Admin");
    }

    // Nạp thông tin cần thiết cho giao diện tạo tài khoản.
    private void NapThongTinCreate(bool laKhoiTao)
    {
        ViewData["LaKhoiTao"] = laKhoiTao;
    }

    // Kiểm tra mã bảo mật tạo Admin đầu tiên.
    private bool KiemTraMaBaoMat(string? maNhap)
    {
        string? maDung = _configuration["Security:FirstAdminCode"];

        if (string.IsNullOrWhiteSpace(maDung) ||
            string.IsNullOrWhiteSpace(maNhap))
        {
            return false;
        }

        byte[] maNhapBytes = Encoding.UTF8.GetBytes(maNhap);
        byte[] maDungBytes = Encoding.UTF8.GetBytes(maDung);

        if (maNhapBytes.Length != maDungBytes.Length)
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            maNhapBytes,
            maDungBytes);
    }

    // GET: TaiKhoans
    public async Task<IActionResult> Index(
        string? tuKhoa,
        int? trangThai,
        int trang = 1)
    {
        var admin = await LayAdminDangNhapAsync();

        if (admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        // Số tài khoản hiển thị trên mỗi trang (10)
        const int SoTaiKhoanMoiTrang = 10;

        var query = _context.TaiKhoans.AsNoTracking();

        // Tìm kiếm theo mã tài khoản, tên đăng nhập hoặc họ tên.
        if (!string.IsNullOrWhiteSpace(tuKhoa))
        {
            string tuKhoaTimKiem = tuKhoa.Trim();

            if (int.TryParse(tuKhoaTimKiem, out int maTaiKhoan))
            {
                query = query.Where(tk => tk.MaTaiKhoan == maTaiKhoan);
            }
            else
            {
                query = query.Where(tk =>
                    tk.TenDangNhap.Contains(tuKhoaTimKiem) ||
                    tk.HoTen.Contains(tuKhoaTimKiem));
            }
        }

        // Lọc theo trạng thái tài khoản.
        if (trangThai.HasValue &&
            Enum.IsDefined(typeof(TrangThaiTaiKhoan), trangThai.Value))
        {
            query = query.Where(tk =>
                (int)tk.TrangThai == trangThai.Value);
        }

        // Đếm tổng số tài khoản sau khi áp dụng bộ lọc
        int tongSoTaiKhoan = await query.CountAsync();

        // Tính tổng số trang
        int tongSoTrang = (int)Math.Ceiling(
            tongSoTaiKhoan / (double)SoTaiKhoanMoiTrang);

        // Đảm bảo trang hiện tại hợp lệ
        if (tongSoTrang == 0)
        {
            trang = 1;
        }
        else
        {
            trang = Math.Clamp(trang, 1, tongSoTrang);
        }

        // Chỉ lấy dữ liệu của trang hiện tại
        var danhSach = await query
            .OrderBy(tk => tk.MaTaiKhoan)
            .Skip((trang - 1) * SoTaiKhoanMoiTrang)
            .Take(SoTaiKhoanMoiTrang)
            .ToListAsync();

        var model = new TaiKhoanDanhSachViewModel
        {
            TaiKhoans = danhSach,
            TrangHienTai = trang,
            TongSoTrang = tongSoTrang,
            SoTaiKhoanMoiTrang = SoTaiKhoanMoiTrang,
            TongSoTaiKhoan = tongSoTaiKhoan,
            TuKhoa = tuKhoa,
            TrangThai = trangThai
        };

        return View(model);
    }

    // GET: TaiKhoans/Details/5
    public async Task<IActionResult> Details(int? mataikhoan)
    {
        var admin = await LayAdminDangNhapAsync();

        if (admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == mataikhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        return View(taikhoan);
    }

    // GET: TaiKhoans/Create
    public async Task<IActionResult> Create()
    {
        bool daCoAdmin = await DaCoAdminAsync();
        var admin = await LayAdminDangNhapAsync();

        // Nếu đã có Admin thì chỉ Admin hiện tại mới được tạo tài khoản
        if (daCoAdmin && admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        var model = new TaiKhoanTaoViewModel();

        // Nếu chưa có Admin, bắt buộc tạo Admin đầu tiên
        if (!daCoAdmin)
        {
            model.TaoAdmin = true;
        }

        NapThongTinCreate(!daCoAdmin);

        return View(model);
    }

    // POST: TaiKhoans/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaiKhoanTaoViewModel model)
    {
        bool daCoAdmin = await DaCoAdminAsync();
        var admin = await LayAdminDangNhapAsync();

        bool laKhoiTao = !daCoAdmin && admin == null;

        // Đã có Admin thì bắt buộc người thực hiện phải là Admin.
        if (daCoAdmin && admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        // Khi chưa có Admin, không cho tạo tài khoản thường trước.
        if (laKhoiTao && !model.TaoAdmin)
        {
            ModelState.AddModelError(
                string.Empty,
                "Bạn phải tạo tài khoản Admin đầu tiên.");
        }

        // Kiểm tra mã bảo mật khi tạo Admin đầu tiên.
        if (laKhoiTao && !KiemTraMaBaoMat(model.MaBaoMat))
        {
            ModelState.AddModelError(
                nameof(model.MaBaoMat),
                "Mã bảo mật không chính xác.");
        }

        // Kiểm tra tên đăng nhập đã tồn tại hay chưa.
        bool tenDangNhapDaTonTai = await _context.TaiKhoans
            .AnyAsync(tk => tk.TenDangNhap == model.TenDangNhap);

        if (tenDangNhapDaTonTai)
        {
            ModelState.AddModelError(
                nameof(model.TenDangNhap),
                "Tên đăng nhập này đã tồn tại.");
        }

        NapThongTinCreate(laKhoiTao);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);

        try
        {
            // Kiểm tra lại trong transaction để tránh hai yêu cầu
            // cùng tạo Admin đầu tiên.
            bool hienTaiDaCoAdmin = await DaCoAdminAsync();

            if (laKhoiTao && hienTaiDaCoAdmin)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Admin đầu tiên đã được tạo bởi một yêu cầu khác.");

                return View(model);
            }

            if (!laKhoiTao && !hienTaiDaCoAdmin)
            {
                await transaction.RollbackAsync();

                ModelState.AddModelError(
                    string.Empty,
                    "Hệ thống chưa có Admin. Vui lòng thực hiện tạo Admin đầu tiên.");

                return View(model);
            }

            // Chỉ gán vai trò từ phía máy chủ.
            string vaiTro = model.TaoAdmin ? "Admin" : "DocGia";

            var taikhoan = new TaiKhoan
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                HoTen = model.HoTen.Trim(),
                VaiTro = vaiTro,
                TrangThai = TrangThaiTaiKhoan.BinhThuong,
                SoLanDangNhapSai = 0
            };

            // Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu.
            taikhoan.MatKhau = _passwordHasher.HashPassword(
                taikhoan,
                model.MatKhau);

            _context.TaiKhoans.Add(taikhoan);

            await _context.SaveChangesAsync();

            // Tài khoản DocGia cần có hồ sơ độc giả tương ứng.
            if (vaiTro == "DocGia")
            {
                var docGia = new DocGia
                {
                    MaTaiKhoan = taikhoan.MaTaiKhoan,
                    HoTen = taikhoan.HoTen,
                    NgayDangKy = DateTime.Today,
                    TrangThai = "Hoạt động"
                };

                _context.DocGias.Add(docGia);

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            TempData["ThongBao"] = "Tạo tài khoản thành công.";

            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();

            ModelState.AddModelError(
                string.Empty,
                "Không thể tạo tài khoản. Vui lòng kiểm tra lại dữ liệu.");

            NapThongTinCreate(laKhoiTao);

            return View(model);
        }
    }


    // GET: TaiKhoans/Edit/5
    public async Task<IActionResult> Edit(int? mataikhoan)
    {
        var admin = await LayAdminDangNhapAsync();

        if (admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == mataikhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        var model = new TaiKhoanSuaViewModel
        {
            MaTaiKhoan = taikhoan.MaTaiKhoan,
            TenDangNhap = taikhoan.TenDangNhap,
            HoTen = taikhoan.HoTen,
            TrangThai = taikhoan.TrangThai
        };

        return View(model);
    }

    // POST: TaiKhoans/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? mataikhoan,
        TaiKhoanSuaViewModel model)
    {
        var admin = await LayAdminDangNhapAsync();

        if (admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        if (mataikhoan == null || mataikhoan != model.MaTaiKhoan)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == model.MaTaiKhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        bool tenDangNhapDaTonTai = await _context.TaiKhoans
            .AnyAsync(tk =>
                tk.TenDangNhap == model.TenDangNhap &&
                tk.MaTaiKhoan != model.MaTaiKhoan);

        if (tenDangNhapDaTonTai)
        {
            ModelState.AddModelError(
                nameof(model.TenDangNhap),
                "Tên đăng nhập này đã tồn tại.");
        }

        if (!string.IsNullOrEmpty(model.MatKhauMoi) &&
            model.MatKhauMoi.Length < 6)
        {
            ModelState.AddModelError(
                nameof(model.MatKhauMoi),
                "Mật khẩu mới phải có ít nhất 6 ký tự.");
        }

        if (!Enum.IsDefined(typeof(TrangThaiTaiKhoan), model.TrangThai))
        {
            ModelState.AddModelError(
                nameof(model.TrangThai),
                "Trạng thái tài khoản không hợp lệ.");
        }

        // Không cho Admin tự khóa hoặc tự ban tài khoản của mình.
        if (taikhoan.MaTaiKhoan == admin.MaTaiKhoan &&
            model.TrangThai != TrangThaiTaiKhoan.BinhThuong)
        {
            ModelState.AddModelError(
                nameof(model.TrangThai),
                "Bạn không thể tự khóa hoặc tự ban tài khoản đang đăng nhập.");
        }

        // Không cho vô hiệu hóa Admin cuối cùng đang hoạt động.
        if (taikhoan.VaiTro == "Admin" &&
            taikhoan.TrangThai == TrangThaiTaiKhoan.BinhThuong &&
            model.TrangThai != TrangThaiTaiKhoan.BinhThuong)
        {
            int soAdminHoatDong = await _context.TaiKhoans
                .CountAsync(tk =>
                    tk.VaiTro == "Admin" &&
                    tk.TrangThai == TrangThaiTaiKhoan.BinhThuong);

            if (soAdminHoatDong <= 1)
            {
                ModelState.AddModelError(
                    nameof(model.TrangThai),
                    "Không thể khóa hoặc ban Admin cuối cùng đang hoạt động.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        taikhoan.TenDangNhap = model.TenDangNhap.Trim();
        taikhoan.HoTen = model.HoTen.Trim();
        taikhoan.TrangThai = model.TrangThai;

        // Mở khóa hoặc bỏ ban thì đặt lại số lần đăng nhập sai.
        if (model.TrangThai == TrangThaiTaiKhoan.BinhThuong)
        {
            taikhoan.SoLanDangNhapSai = 0;
        }

        if (!string.IsNullOrWhiteSpace(model.MatKhauMoi))
        {
            taikhoan.MatKhau = _passwordHasher.HashPassword(
                taikhoan,
                model.MatKhauMoi);
        }

        // Đồng bộ họ tên với hồ sơ độc giả nếu có.
        var docGia = await _context.DocGias
            .FirstOrDefaultAsync(dg => dg.MaTaiKhoan == taikhoan.MaTaiKhoan);

        if (docGia != null)
        {
            docGia.HoTen = taikhoan.HoTen;
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TaiKhoanExists(model.MaTaiKhoan))
            {
                return NotFound();
            }

            throw;
        }

        TempData["ThongBao"] = "Cập nhật tài khoản thành công.";

        return RedirectToAction(nameof(Index));
    }


    // GET: TaiKhoans/Delete/5
    public async Task<IActionResult> Delete(int? mataikhoan)
    {
        var admin = await LayAdminDangNhapAsync();

        if (admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == mataikhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        return View(taikhoan);
    }

    // POST: TaiKhoans/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? mataikhoan)
    {
        var admin = await LayAdminDangNhapAsync();

        if (admin == null)
        {
            return RedirectToAction("Index", "DangNhap");
        }

        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == mataikhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        // Không cho xóa Admin cuối cùng đang hoạt động.
        if (taikhoan.VaiTro == "Admin" &&
            taikhoan.TrangThai == TrangThaiTaiKhoan.BinhThuong)
        {
            int soAdminHoatDong = await _context.TaiKhoans
                .CountAsync(tk =>
                    tk.VaiTro == "Admin" &&
                    tk.TrangThai == TrangThaiTaiKhoan.BinhThuong);

            if (soAdminHoatDong <= 1)
            {
                TempData["Loi"] =
                    "Không thể xóa Admin cuối cùng đang hoạt động.";

                return RedirectToAction(nameof(Index));
            }
        }

        // Tìm hồ sơ độc giả liên kết với tài khoản.
        var docGia = await _context.DocGias
            .FirstOrDefaultAsync(dg => dg.MaTaiKhoan == taikhoan.MaTaiKhoan);

        if (docGia != null)
        {
            // Kiểm tra lịch sử mượn sách của độc giả.
            bool coPhieuMuon = await _context.PhieuMuons
                .AnyAsync(pm => pm.MaDocGia == docGia.MaDocGia);

            if (coPhieuMuon)
            {
                TempData["Loi"] =
                    "Không thể xóa tài khoản vì độc giả đã có lịch sử mượn sách.";

                return RedirectToAction(nameof(Index));
            }

            // Xóa hồ sơ độc giả trước khi xóa tài khoản.
            _context.DocGias.Remove(docGia);
        }

        // Xóa tài khoản.
        _context.TaiKhoans.Remove(taikhoan);

        await _context.SaveChangesAsync();

        TempData["ThongBao"] = "Xóa tài khoản thành công.";

        return RedirectToAction(nameof(Index));
    }

    private bool TaiKhoanExists(int mataikhoan)
    {
        return _context.TaiKhoans
            .Any(tk => tk.MaTaiKhoan == mataikhoan);
    }
}