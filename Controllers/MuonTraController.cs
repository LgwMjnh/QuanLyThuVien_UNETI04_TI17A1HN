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

        // =========================
        // DANH SÁCH PHIẾU MƯỢN
        // =========================
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

            if (!string.IsNullOrEmpty(trangThai))
            {
                if (trangThai == "QuaHan")
                {
                    query = query.Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.HanTra < DateTime.Today);
                }
                else if (trangThai == "DangMuon")
                {
                    query = query.Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.HanTra >= DateTime.Today);
                }
                else if (trangThai == "DaTra")
                {
                    query = query.Where(p =>
                        p.TrangThai == "Đã trả" ||
                        p.NgayTra != null);
                }
                else
                {
                    query = query.Where(p => p.TrangThai == trangThai);
                }
            }

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(p =>
                    p.DocGia != null &&
                    p.DocGia.HoTen.Contains(tuKhoa));
            }

            if (ngayTu.HasValue)
            {
                query = query.Where(p =>
                    p.NgayMuon >= ngayTu.Value);
            }

            if (ngayDen.HasValue)
            {
                DateTime den = ngayDen.Value.Date.AddDays(1);

                query = query.Where(p =>
                    p.NgayMuon < den);
            }

            query = query.OrderByDescending(p => p.NgayMuon);

            int soLuongTrenTrang = 10;

            int tongSoPhieu = await query.CountAsync();

            int tongSoTrang = (int)Math.Ceiling(
                tongSoPhieu / (double)soLuongTrenTrang);

            if (page < 1)
            {
                page = 1;
            }

            if (tongSoTrang > 0 && page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var danhSachPhieu = await query
                .Skip((page - 1) * soLuongTrenTrang)
                .Take(soLuongTrenTrang)
                .ToListAsync();

            // =========================
            // THỐNG KÊ
            // =========================

            ViewBag.TongPhieu = await _context.PhieuMuons
                .CountAsync();

            ViewBag.DangMuon = await _context.PhieuMuons
                .CountAsync(p =>
                    p.NgayTra == null &&
                    p.TrangThai == "Đang mượn" &&
                    p.HanTra >= DateTime.Today);

            ViewBag.QuaHan = await _context.PhieuMuons
                .CountAsync(p =>
                    p.NgayTra == null &&
                    p.TrangThai == "Đang mượn" &&
                    p.HanTra < DateTime.Today);

            ViewBag.DaTra = await _context.PhieuMuons
                .CountAsync(p =>
                    p.NgayTra != null ||
                    p.TrangThai == "Đã trả");

            // =========================
            // GIỮ THÔNG TIN TÌM KIẾM
            // =========================

            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.TrangThai = trangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.NgayTu = ngayTu;
            ViewBag.NgayDen = ngayDen;
            ViewBag.TrangHienTai = page;
            ViewBag.TongSoTrang = tongSoTrang;

            return View(danhSachPhieu);
        }


        // =========================
        // GET: LẬP PHIẾU MƯỢN
        // =========================
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

            ViewBag.DocGias = new SelectList(
                docGias,
                "MaDocGia",
                "HoTen");

            ViewBag.Sach = sach;

            return View();
        }


        // =========================
        // POST: LƯU PHIẾU MƯỢN
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int MaDocGia,
            int MaSach,
            int SoLuong)
        {
            // Kiểm tra độc giả
            var docGia = await _context.DocGias
                .FirstOrDefaultAsync(d => d.MaDocGia == MaDocGia);

            if (docGia == null)
            {
                TempData["Loi"] = "Không tìm thấy độc giả.";
                return RedirectToAction(nameof(Create));
            }

            // Kiểm tra sách
            var sach = await _context.Sachs
                .FirstOrDefaultAsync(s => s.MaSach == MaSach);

            if (sach == null)
            {
                TempData["Loi"] = "Không tìm thấy sách.";
                return RedirectToAction(nameof(Create));
            }

            // Kiểm tra số lượng
            if (SoLuong <= 0)
            {
                TempData["Loi"] = "Số lượng mượn phải lớn hơn 0.";
                return RedirectToAction(nameof(Create));
            }

            if (SoLuong > sach.SoLuongCon)
            {
                TempData["Loi"] =
                    $"Sách \"{sach.TenSach}\" chỉ còn {sach.SoLuongCon} cuốn.";
                return RedirectToAction(nameof(Create));
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // =========================
                // TẠO PHIẾU MƯỢN
                // =========================

                var phieuMuon = new PhieuMuon
                {
                    MaDocGia = MaDocGia,
                    NgayMuon = DateTime.Today,
                    HanTra = DateTime.Today.AddDays(14),
                    TrangThai = "Đang mượn",
                    NgayTra = null
                };

                _context.PhieuMuons.Add(phieuMuon);

                await _context.SaveChangesAsync();

                // =========================
                // TẠO CHI TIẾT PHIẾU
                // =========================

                var chiTiet = new ChiTietPhieuMuon
                {
                    MaPhieuMuon = phieuMuon.MaPhieuMuon,
                    MaSach = MaSach,
                    SoLuong = SoLuong
                };

                _context.ChiTietPhieuMuons.Add(chiTiet);

                // =========================
                // TRỪ SỐ LƯỢNG SÁCH
                // =========================

                sach.SoLuongCon -= SoLuong;

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    $"Lập phiếu mượn thành công! Mã phiếu: {phieuMuon.MaPhieuMuon}";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Có lỗi xảy ra khi lập phiếu mượn.";

                return RedirectToAction(nameof(Create));
            }
        }


        // =========================
        // POST: TRẢ SÁCH
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tra(int id)
        {
            var phieuMuon = await _context.PhieuMuons
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                .FirstOrDefaultAsync(p =>
                    p.MaPhieuMuon == id);

            if (phieuMuon == null)
            {
                TempData["Loi"] =
                    "Không tìm thấy phiếu mượn.";

                return RedirectToAction(nameof(Index));
            }

            if (phieuMuon.NgayTra != null)
            {
                TempData["Loi"] =
                    "Phiếu mượn này đã được trả trước đó.";

                return RedirectToAction(nameof(Index));
            }

            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                foreach (var chiTiet in phieuMuon.ChiTietPhieuMuons)
                {
                    var sach = chiTiet.Sach;

                    if (sach != null)
                    {
                        sach.SoLuongCon += chiTiet.SoLuong;

                        if (sach.SoLuongCon > sach.SoLuong)
                        {
                            sach.SoLuongCon = sach.SoLuong;
                        }
                    }
                }

                phieuMuon.NgayTra = DateTime.Today;
                phieuMuon.TrangThai = "Đã trả";

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    "Trả sách thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Có lỗi xảy ra khi trả sách.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}