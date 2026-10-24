using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class PhieuMuonController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PhieuMuonController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: PhieuMuon
        public async Task<IActionResult> Index(string? trangThai)
        {
            var query = _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(p => p.TrangThai == trangThai);
            }

            ViewBag.TrangThai = trangThai;

            var tatCaPhieu = await _context.PhieuMuons.ToListAsync();
            ViewBag.TongSo = tatCaPhieu.Count;
            ViewBag.SoChoXacNhan = tatCaPhieu.Count(p => p.TrangThai == "Chờ xác nhận");
            ViewBag.SoDangMuon = tatCaPhieu.Count(p => p.TrangThai == "Đang mượn");
            ViewBag.SoDaTra = tatCaPhieu.Count(p => p.TrangThai == "Đã trả");
            ViewBag.SoDaHuy = tatCaPhieu.Count(p => p.TrangThai == "Đã hủy");

            var danhSach = await query
                .OrderByDescending(p => p.NgayMuon)
                .ToListAsync();

            return View(danhSach);
        }

        // GET: PhieuMuon/Details/5
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

            return View(phieu);
        }

        // GET: PhieuMuon/Create
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

        // POST: PhieuMuon/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            PhieuMuon phieuMuon,
            int maSach,
            int soLuongMuon)
        {
            if (phieuMuon.HanTra.Date <= phieuMuon.NgayMuon.Date)
            {
                ModelState.AddModelError(
                    nameof(PhieuMuon.HanTra),
                    "Hạn trả phải sau ngày mượn.");
            }

            if (maSach <= 0)
            {
                ModelState.AddModelError(
                    "maSach",
                    "Vui lòng chọn sách cần mượn.");
            }

            if (soLuongMuon <= 0)
            {
                ModelState.AddModelError(
                    "soLuongMuon",
                    "Số lượng mượn phải lớn hơn 0.");
            }

            if (ModelState.IsValid)
            {
                var sach = await _context.Sachs
                    .FirstOrDefaultAsync(s => s.MaSach == maSach);

                if (sach == null)
                {
                    ModelState.AddModelError("maSach", "Sách không tồn tại.");
                }
                else if (soLuongMuon > sach.SoLuongCon)
                {
                    ModelState.AddModelError(
                        "soLuongMuon",
                        "Số lượng mượn vượt quá số lượng sách còn.");
                }
                else
                {
                    phieuMuon.TrangThai = "Chờ xác nhận";

                    phieuMuon.ChiTietPhieuMuons.Add(new ChiTietPhieuMuon
                    {
                        MaSach = maSach,
                        SoLuongMuon = soLuongMuon
                    });

                    _context.PhieuMuons.Add(phieuMuon);
                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        $"Tạo phiếu mượn #{phieuMuon.MaPhieuMuon} thành công!";

                    return RedirectToAction(
                        nameof(Details),
                        new { id = phieuMuon.MaPhieuMuon });
                }
            }

            await NapDuLieuChonAsync(phieuMuon.MaDocGia, maSach);
            ViewBag.SoLuongMuon = soLuongMuon;

            return View(phieuMuon);
        }

        // POST: PhieuMuon/CapNhatTrangThai/5
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

        private async Task NapDuLieuChonAsync(
            int? maDocGiaChon = null,
            int? maSachChon = null)
        {
            var docGias = await _context.DocGias
                .Where(d => d.TrangThai == "Hoạt động")
                .OrderBy(d => d.HoTen)
                .Select(d => new
                {
                    d.MaDocGia,
                    TenHienThi = d.HoTen + " (SĐT: " +
                        (d.SoDienThoai ?? "Chưa có") + ")"
                })
                .ToListAsync();

            var sachs = await _context.Sachs
                .Where(s => s.TrangThai == "Đang phục vụ")
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
        }
    }
}