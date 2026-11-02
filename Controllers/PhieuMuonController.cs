using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.Helpers;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class PhieuMuonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhieuMuonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Xem danh sách phiếu mượn (Admin xem tất cả, Độc giả xem phiếu của mình)
        public async Task<IActionResult> Index(string? trangThai)
        {
            var query = _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                .AsQueryable();

            // Nếu người dùng là Độc giả, chỉ lấy dữ liệu của chính mình
            if (QuyenTruyCap.LaDocGia(HttpContext))
            {
                var maTaiKhoan = QuyenTruyCap.LayMaTaiKhoan(HttpContext);
                var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan);

                if (docGia != null)
                {
                    query = query.Where(p => p.MaDocGia == docGia.MaDocGia);
                }
                else
                {
                    query = query.Where(p => false);
                }
            }

            // Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            ViewBag.TrangThai = trangThai;

            // Thống kê số lượng theo từng trạng thái
            var tatCaPhieu = await query.ToListAsync();
            ViewBag.TongSo = tatCaPhieu.Count;
            ViewBag.SoChoXacNhan = tatCaPhieu.Count(p => p.TrangThai == "Chờ xác nhận");
            ViewBag.SoDangMuon = tatCaPhieu.Count(p => p.TrangThai == "Đang mượn");
            ViewBag.SoDaTra = tatCaPhieu.Count(p => p.TrangThai == "Đã trả");
            ViewBag.SoDaHuy = tatCaPhieu.Count(p => p.TrangThai == "Đã hủy");

            var danhSach = tatCaPhieu
                .OrderByDescending(p => p.NgayMuon)
                .ToList();

            return View(danhSach);
        }

        // Xem chi tiết phiếu mượn & danh sách sách trong phiếu
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var phieu = await _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                        .ThenInclude(s => s!.TheLoai)
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieu == null)
            {
                return NotFound();
            }

            // Bảo mật dữ liệu - Độc giả không được xem phiếu của người khác
            if (QuyenTruyCap.LaDocGia(HttpContext))
            {
                var maTaiKhoan = QuyenTruyCap.LayMaTaiKhoan(HttpContext);
                var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan);

                if (docGia == null || phieu.MaDocGia != docGia.MaDocGia)
                {
                    TempData["Error"] = "Bạn không có quyền xem thông tin phiếu mượn của độc giả khác.";
                    return RedirectToAction(nameof(Index));
                }
            }

            return View(phieu);
        }

        // Tạo phiếu mượn trực tiếp tại quầy (Admin)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await NapDuLieuChonAsync();

            return View(new PhieuMuon
            {
                NgayMuon = DateTime.Today,
                HanTra = DateTime.Today.AddDays(14),
                TrangThai = "Chờ xác nhận"
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PhieuMuon phieuMuon,
            List<int>? danhSachMaSach,
            List<int>? danhSachSoLuong,
            int maSach = 0,
            int soLuongMuon = 1)
        {
            if (phieuMuon.MaDocGia <= 0)
            {
                ModelState.AddModelError(nameof(PhieuMuon.MaDocGia), "Vui lòng chọn độc giả mượn sách.");
            }

            if (phieuMuon.HanTra.Date <= phieuMuon.NgayMuon.Date)
            {
                ModelState.AddModelError(nameof(PhieuMuon.HanTra), "Hạn trả phải sau ngày mượn.");
            }

            // Thu thập danh sách sách được chọn (hỗ trợ cả chọn nhiều sách hoặc 1 sách đơn)
            var sachChonList = new List<(int MaSach, int SoLuong)>();
            if (danhSachMaSach != null && danhSachMaSach.Any())
            {
                for (int i = 0; i < danhSachMaSach.Count; i++)
                {
                    int mSach = danhSachMaSach[i];
                    int sLuong = (danhSachSoLuong != null && i < danhSachSoLuong.Count) ? danhSachSoLuong[i] : 1;
                    if (mSach > 0)
                    {
                        sachChonList.Add((mSach, sLuong <= 0 ? 1 : sLuong));
                    }
                }
            }
            else if (maSach > 0)
            {
                sachChonList.Add((maSach, soLuongMuon <= 0 ? 1 : soLuongMuon));
            }

            if (!sachChonList.Any())
            {
                ModelState.AddModelError(string.Empty, "Vui lòng chọn ít nhất một cuốn sách cần mượn.");
            }

            // Kiểm tra trùng lặp sách trong cùng một phiếu
            var duplicateSach = sachChonList
                .GroupBy(x => x.MaSach)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateSach.Any())
            {
                ModelState.AddModelError(string.Empty, "Có sách bị chọn trùng lặp nhiều lần trong phiếu. Vui lòng gộp số lượng vào một dòng.");
            }

            // Kiểm tra tổng số sách mượn không vượt quá 5 quyển
            int tongSoLuong = sachChonList.Sum(x => x.SoLuong);
            if (tongSoLuong > 5)
            {
                ModelState.AddModelError(string.Empty, "Tổng số lượng sách mượn trong một phiếu không được vượt quá 5 quyển.");
            }

            // Kiểm tra số lượng sách còn trong kho
            foreach (var item in sachChonList)
            {
                var sach = await _context.Sachs.FirstOrDefaultAsync(s => s.MaSach == item.MaSach);
                if (sach == null)
                {
                    ModelState.AddModelError(string.Empty, $"Sách mã #{item.MaSach} không tồn tại.");
                }
                else if (sach.TrangThai != "Đang phục vụ")
                {
                    ModelState.AddModelError(string.Empty, $"Cuốn '{sach.TenSach}' hiện không còn phục vụ cho mượn.");
                }
                else if (item.SoLuong > sach.SoLuongCon)
                {
                    ModelState.AddModelError(string.Empty, $"Cuốn '{sach.TenSach}' chỉ còn {sach.SoLuongCon} quyển trong kho (yêu cầu {item.SoLuong}).");
                }
            }

            if (ModelState.IsValid)
            {
                phieuMuon.TrangThai = "Chờ xác nhận"; // Mặc định luôn là Chờ xác nhận khi lập phiếu

                foreach (var item in sachChonList)
                {
                    phieuMuon.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                    {
                        MaSach = item.MaSach,
                        SoLuongMuon = item.SoLuong
                    });
                }

                _context.PhieuMuons.Add(phieuMuon);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Tạo phiếu mượn #{phieuMuon.MaPhieuMuon} thành công với {sachChonList.Count} đầu sách ({tongSoLuong} quyển)!";
                return RedirectToAction(nameof(Details), new { id = phieuMuon.MaPhieuMuon });
            }

            await NapDuLieuChonAsync(phieuMuon.MaDocGia);
            return View(phieuMuon);
        }

        // Độc giả đăng ký mượn sách trực tuyến
        [HttpGet]
        public async Task<IActionResult> DangKy()
        {
            var maTaiKhoan = QuyenTruyCap.LayMaTaiKhoan(HttpContext);
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đăng ký mượn sách.";
                return RedirectToAction("Index", "DangNhap");
            }

            var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
            if (docGia == null)
            {
                TempData["Error"] = "Tài khoản của bạn chưa được liên kết với hồ sơ độc giả.";
                return RedirectToAction("Index", "Home");
            }

            if (docGia.TrangThai == "Khóa" || docGia.TrangThai == "Tạm khóa")
            {
                TempData["Error"] = "Thẻ độc giả của bạn đang bị khóa. Vui lòng liên hệ thủ thư để được mở lại.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.TenDocGia = docGia.HoTen;
            ViewBag.MaDocGia = docGia.MaDocGia;
            await NapDanhSachSachDangPhucVu();

            return View(new DangKyMuonViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyMuonViewModel model)
        {
            var maTaiKhoan = QuyenTruyCap.LayMaTaiKhoan(HttpContext);
            if (maTaiKhoan == null)
            {
                TempData["Error"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại.";
                return RedirectToAction("Index", "DangNhap");
            }

            var docGia = await _context.DocGias.FirstOrDefaultAsync(d => d.MaTaiKhoan == maTaiKhoan.Value);
            if (docGia == null)
            {
                TempData["Error"] = "Không tìm thấy hồ sơ độc giả.";
                return RedirectToAction("Index", "Home");
            }

            // Kiểm tra thẻ độc giả
            if (docGia.TrangThai == "Khóa" || docGia.TrangThai == "Tạm khóa")
            {
                ModelState.AddModelError(string.Empty, "Thẻ độc giả của bạn đang bị khóa, không thể đăng ký mượn sách.");
            }

            // Kiểm tra hạn trả
            if (model.HanTra.Date <= DateTime.Today)
            {
                ModelState.AddModelError(nameof(model.HanTra), "Hạn trả phải sau ngày hôm nay.");
            }

            // Kiểm tra sách tồn tại và đang phục vụ
            var sach = await _context.Sachs.FirstOrDefaultAsync(s => s.MaSach == model.MaSach);
            if (sach == null)
            {
                ModelState.AddModelError(nameof(model.MaSach), "Sách được chọn không tồn tại.");
            }
            else
            {
                if (sach.TrangThai != "Đang phục vụ")
                {
                    ModelState.AddModelError(nameof(model.MaSach), "Sách này hiện tạm ngừng hoặc không còn phục vụ.");
                }

                if (model.SoLuongMuon > sach.SoLuongCon)
                {
                    ModelState.AddModelError(nameof(model.SoLuongMuon), $"Số lượng mượn vượt quá số lượng sách còn (Chỉ còn {sach.SoLuongCon} quyển).");
                }
            }

            // Kiểm tra không tạo yêu cầu trùng không hợp lệ
            if (sach != null)
            {
                bool daCoYeuCauTrung = await _context.PhieuMuons
                    .Where(p => p.MaDocGia == docGia.MaDocGia &&
                               (p.TrangThai == "Chờ xác nhận" || p.TrangThai == "Đang mượn"))
                    .AnyAsync(p => p.ChiTietPhieuMuons.Any(ct => ct.MaSach == model.MaSach && ct.NgayTra == null));

                if (daCoYeuCauTrung)
                {
                    ModelState.AddModelError(string.Empty,
                        $"Bạn đang có phiếu mượn hoặc yêu cầu chờ duyệt cho cuốn '{sach.TenSach}'. Vui lòng không tạo yêu cầu trùng lặp.");
                }
            }

            if (ModelState.IsValid)
            {
                var phieuMuon = new PhieuMuon
                {
                    MaDocGia = docGia.MaDocGia,
                    NgayMuon = DateTime.Today,
                    HanTra = model.HanTra,
                    TrangThai = "Chờ xác nhận"
                };

                phieuMuon.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                {
                    MaSach = model.MaSach,
                    SoLuongMuon = model.SoLuongMuon
                });

                _context.PhieuMuons.Add(phieuMuon);
                await _context.SaveChangesAsync();

                TempData["Success"] =
                    $"Đăng ký mượn sách thành công! Yêu cầu #{phieuMuon.MaPhieuMuon} đang ở trạng thái 'Chờ xác nhận'.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.TenDocGia = docGia.HoTen;
            ViewBag.MaDocGia = docGia.MaDocGia;
            await NapDanhSachSachDangPhucVu(model.MaSach);

            return View(model);
        }

        // Chuyển trạng thái phiếu mượn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhatTrangThai(int id, string trangThai)
        {
            var phieu = await _context.PhieuMuons.FindAsync(id);

            if (phieu == null)
            {
                return NotFound();
            }

            var hopLe =
                trangThai == "Chờ xác nhận" ||
                trangThai == "Đang mượn" ||
                trangThai == "Đã trả" ||
                trangThai == "Đã hủy";

            if (!hopLe)
            {
                TempData["Error"] = "Trạng thái phiếu mượn không hợp lệ.";
                return RedirectToAction(nameof(Index));
            }

            phieu.TrangThai = trangThai;
            await _context.SaveChangesAsync();

            TempData["Success"] =
                $"Đã cập nhật trạng thái phiếu mượn #{id} thành '{trangThai}'.";

            return RedirectToAction(nameof(Details), new { id });
        }

        // Tiện ích nạp dữ liệu chọn
        private async Task NapDuLieuChonAsync(
            int? maDocGiaChon = null,
            int? maSachChon = null)
        {
            var docGiaList = await _context.DocGias
                .Where(d => d.TrangThai == "Hoạt động")
                .OrderBy(d => d.HoTen)
                .Select(d => new
                {
                    d.MaDocGia,
                    d.HoTen,
                    d.SoDienThoai
                })
                .ToListAsync();

            var docGias = docGiaList.Select(d => new
            {
                d.MaDocGia,
                TenHienThi = $"[DG{d.MaDocGia:D3}] {d.HoTen} (SĐT: {d.SoDienThoai ?? "Chưa có"})"
            }).ToList();

            var sachs = await _context.Sachs
                .Where(s => s.TrangThai == "Đang phục vụ" && s.SoLuongCon > 0)
                .OrderBy(s => s.TenSach)
                .Select(s => new
                {
                    s.MaSach,
                    TenHienThi = s.TenSach + " (Còn: " +
                        s.SoLuongCon + " quyển)"
                })
                .ToListAsync();

            ViewBag.MaDocGia = new SelectList(
                docGias, "MaDocGia", "TenHienThi", maDocGiaChon);

            ViewBag.MaSach = new SelectList(
                sachs, "MaSach", "TenHienThi", maSachChon);

            ViewBag.DanhSachSach = sachs;
        }

        private async Task NapDanhSachSachDangPhucVu(int? maSachChon = null)
        {
            var sachs = await _context.Sachs
                .Where(s => s.TrangThai == "Đang phục vụ" && s.SoLuongCon > 0)
                .OrderBy(s => s.TenSach)
                .Select(s => new
                {
                    s.MaSach,
                    TenHienThi = s.TenSach + " - Tác giả: " + s.TacGia + " (Còn " + s.SoLuongCon + " quyển)"
                })
                .ToListAsync();

            ViewBag.MaSach = new SelectList(
                sachs, "MaSach", "TenHienThi", maSachChon);
        }
    }
}
