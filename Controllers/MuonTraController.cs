
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class MuonTraController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MuonTraController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? trangThai,
            string? tuKhoa,
            DateTime? ngayTu,
            DateTime? ngayDen,
            int page = 1)
        {
            var query = _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                .AsQueryable();

            switch (trangThai)
            {
                case "ChoXacNhan":
                    query = query.Where(p =>
                        p.TrangThai == "Chờ xác nhận");
                    break;

                case "DangMuon":
                    query = query.Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.NgayTra == null &&
                        p.HanTra >= DateTime.Today);
                    break;

                case "QuaHan":
                    query = query.Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.NgayTra == null &&
                        p.HanTra < DateTime.Today);
                    break;

                case "DaTra":
                    query = query.Where(p =>
                        p.TrangThai == "Đã trả" ||
                        p.NgayTra != null);
                    break;

                case "DaHuy":
                    query = query.Where(p =>
                        p.TrangThai == "Đã hủy");
                    break;
            }

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                query = query.Where(p =>
                    p.DocGia != null &&
                    p.DocGia.HoTen.Contains(tuKhoa));
            }

            if (ngayTu.HasValue)
            {
                query = query.Where(p =>
                    p.NgayMuon >= ngayTu.Value.Date);
            }

            if (ngayDen.HasValue)
            {
                var den = ngayDen.Value.Date.AddDays(1);
                query = query.Where(p => p.NgayMuon < den);
            }

            query = query.OrderByDescending(p => p.NgayMuon);

            const int soLuongTrenTrang = 10;
            var tongSoPhieu = await query.CountAsync();
            var tongSoTrang = (int)Math.Ceiling(
                tongSoPhieu / (double)soLuongTrenTrang);

            if (page < 1) page = 1;
            if (tongSoTrang > 0 && page > tongSoTrang)
                page = tongSoTrang;

            var danhSachPhieu = await query
                .Skip((page - 1) * soLuongTrenTrang)
                .Take(soLuongTrenTrang)
                .ToListAsync();

            var homNay = DateTime.Today;

            ViewBag.TongPhieu =
                await _context.PhieuMuons.CountAsync();

            ViewBag.ChoXacNhan =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Chờ xác nhận");

            ViewBag.DangMuon =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.NgayTra == null &&
                    p.HanTra >= homNay);

            ViewBag.QuaHan =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.NgayTra == null &&
                    p.HanTra < homNay);

            ViewBag.DaTra =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Đã trả" ||
                    p.NgayTra != null);

            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.NgayTu = ngayTu;
            ViewBag.NgayDen = ngayDen;
            ViewBag.TrangHienTai = page;
            ViewBag.TongSoTrang = tongSoTrang;

            return View(danhSachPhieu);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var docGias = await _context.DocGias
                .OrderBy(d => d.HoTen)
                .ToListAsync();

            var sach = await _context.Sachs
                .Where(s => s.SoLuongCon > 0)
                .OrderBy(s => s.TenSach)
                .ToListAsync();

            ViewBag.DocGias =
                new SelectList(docGias, "MaDocGia", "HoTen");

            ViewBag.Sach = sach;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int MaDocGia,
            int MaSach,
            int SoLuong)
        {
            var docGia = await _context.DocGias
                .FirstOrDefaultAsync(d => d.MaDocGia == MaDocGia);

            if (docGia == null)
            {
                TempData["Loi"] = "Không tìm thấy độc giả.";
                return RedirectToAction(nameof(Create));
            }

            var sach = await _context.Sachs
                .FirstOrDefaultAsync(s => s.MaSach == MaSach);

            if (sach == null)
            {
                TempData["Loi"] = "Không tìm thấy sách.";
                return RedirectToAction(nameof(Create));
            }

            if (SoLuong <= 0)
            {
                TempData["Loi"] =
                    "Số lượng mượn phải lớn hơn 0.";
                return RedirectToAction(nameof(Create));
            }

            if (SoLuong > sach.SoLuongCon)
            {
                TempData["Loi"] =
                    $"Sách \"{sach.TenSach}\" hiện chỉ còn " +
                    $"{sach.SoLuongCon} cuốn.";
                return RedirectToAction(nameof(Create));
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = MaDocGia,
                NgayMuon = DateTime.Today,
                HanTra = DateTime.Today.AddDays(14),
                TrangThai = "Chờ xác nhận",
                NgayTra = null
            };

            _context.PhieuMuons.Add(phieuMuon);
            await _context.SaveChangesAsync();

            var chiTiet = new ChiTietPhieuMuon
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                MaSach = MaSach,
                SoLuong = SoLuong
            };

            _context.ChiTietPhieuMuons.Add(chiTiet);
            await _context.SaveChangesAsync();

            // Chưa trừ SoLuongCon ở bước gửi yêu cầu.
            // Chỉ trừ khi Admin xác nhận mượn.

            TempData["ThongBao"] =
                $"Đã tạo yêu cầu mượn số {phieuMuon.MaPhieuMuon}. " +
                "Phiếu đang chờ xác nhận.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var phieuMuon = await _context.PhieuMuons
                    .Include(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(p =>
                        p.MaPhieuMuon == id);

                if (phieuMuon == null)
                {
                    TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                    return RedirectToAction(nameof(Index));
                }

                if (phieuMuon.TrangThai != "Chờ xác nhận")
                {
                    TempData["Loi"] =
                        "Chỉ xác nhận được phiếu đang chờ xác nhận.";
                    return RedirectToAction(nameof(Index));
                }

                if (!phieuMuon.ChiTietPhieuMuons.Any())
                {
                    TempData["Loi"] =
                        "Phiếu mượn chưa có chi tiết sách.";
                    return RedirectToAction(nameof(Index));
                }

                // Kiểm tra tồn kho trước khi trừ số lượng.
                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    if (ct.Sach == null ||
                        ct.SoLuong <= 0 ||
                        ct.Sach.SoLuongCon < ct.SoLuong)
                    {
                        TempData["Loi"] =
                            $"Sách trong phiếu {id} không còn đủ số lượng. " +
                            "Vui lòng kiểm tra lại kho.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    ct.Sach!.SoLuongCon -= ct.SoLuong;
                }

                phieuMuon.TrangThai = "Đang mượn";
                phieuMuon.NgayTra = null;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    $"Đã xác nhận phiếu mượn số {id}.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Loi"] =
                    "Có lỗi khi xác nhận phiếu mượn.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id)
        {
            var phieuMuon = await _context.PhieuMuons
                .FirstOrDefaultAsync(p => p.MaPhieuMuon == id);

            if (phieuMuon == null)
            {
                TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                return RedirectToAction(nameof(Index));
            }

            if (phieuMuon.TrangThai != "Chờ xác nhận")
            {
                TempData["Loi"] =
                    "Chỉ được hủy phiếu đang chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            phieuMuon.TrangThai = "Đã hủy";

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                $"Đã hủy yêu cầu mượn số {id}.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tra(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var phieuMuon = await _context.PhieuMuons
                    .Include(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(p =>
                        p.MaPhieuMuon == id);

                if (phieuMuon == null)
                {
                    TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                    return RedirectToAction(nameof(Index));
                }

                if (phieuMuon.TrangThai != "Đang mượn" ||
                    phieuMuon.NgayTra != null)
                {
                    TempData["Loi"] =
                        "Phiếu này không ở trạng thái có thể trả sách.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    if (ct.Sach == null)
                    {
                        TempData["Loi"] =
                            "Không tìm thấy thông tin sách trong phiếu.";
                        return RedirectToAction(nameof(Index));
                    }

                    if (ct.Sach.SoLuongCon + ct.SoLuong >
                        ct.Sach.SoLuong)
                    {
                        TempData["Loi"] =
                            $"Số lượng sách \"{ct.Sach.TenSach}\" " +
                            "không hợp lệ; đã hủy thao tác trả.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    ct.Sach!.SoLuongCon += ct.SoLuong;
                    ct.NgayTra = DateTime.Today;
                    ct.TinhTrangTra = "Đã nhận trả";
                }

                phieuMuon.NgayTra = DateTime.Today;
                phieuMuon.TrangThai = "Đã trả";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    $"Đã ghi nhận trả sách cho phiếu {id}.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Loi"] =
                    "Có lỗi khi trả sách. Vui lòng thử lại.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
