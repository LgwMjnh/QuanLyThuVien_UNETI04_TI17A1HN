using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class ThongKeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ThongKeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;

            var phieuMuon = _context.PhieuMuons.AsNoTracking();

            var model = new ThongKeViewModel
            {
                // Các chỉ số tổng quan
                TongPhieuMuon = await phieuMuon.CountAsync(),

                DangMuon = await phieuMuon.CountAsync(
                    p => p.TrangThai == "Đang mượn"),

                DaTra = await phieuMuon.CountAsync(
                    p => p.TrangThai == "Đã trả"),

                QuaHan = await phieuMuon.CountAsync(
                    p => p.HanTra < homNay
                         && p.TrangThai != "Đã trả"
                         && p.TrangThai != "Đã hủy"),

                TongDocGia = await _context.DocGias
                    .AsNoTracking()
                    .CountAsync(),

                TongSach = await _context.Sachs
                    .AsNoTracking()
                    .CountAsync()
            };

            // Báo cáo 1: Số đầu sách theo thể loại
            model.SachTheoTheLoai = await _context.TheLoais
                .AsNoTracking()
                .Select(t => new SachTheoTheLoaiThongKe
                {
                    TenTheLoai = t.TenTheLoai,
                    SoDauSach = _context.Sachs.Count(
                        s => s.MaTheLoai == t.MaTheLoai)
                })
                .OrderByDescending(x => x.SoDauSach)
                .ToListAsync();

            // Báo cáo 2: Lượt mượn theo sách
            var thongKeSach = await _context.ChiTietPhieuMuons
                .AsNoTracking()
                .GroupBy(ct => ct.MaSach)
                .Select(g => new
                {
                    MaSach = g.Key,
                    SoLuotMuon = g.Count(),
                    TongSoLuong = g.Sum(ct => ct.SoLuongMuon)
                })
                .Join(
                    _context.Sachs.AsNoTracking(),
                    tk => tk.MaSach,
                    s => s.MaSach,
                    (tk, s) => new LuotMuonTheoSachThongKe
                    {
                        TenSach = s.TenSach,
                        SoLuotMuon = tk.SoLuotMuon,
                        TongSoLuong = tk.TongSoLuong
                    })
                .OrderByDescending(x => x.TongSoLuong)
                .ToListAsync();

            model.LuotMuonTheoSach = thongKeSach;

            // Báo cáo 3: Số phiếu theo trạng thái
            model.PhieuTheoTrangThai = await _context.PhieuMuons
                .AsNoTracking()
                .GroupBy(p => p.TrangThai)
                .Select(g => new PhieuTheoTrangThaiThongKe
                {
                    TrangThai = g.Key,
                    SoPhieu = g.Count()
                })
                .OrderByDescending(x => x.SoPhieu)
                .ToListAsync();

            // Báo cáo 4: Top 10 sách được mượn nhiều nhất
            model.TopSachMuonNhieuNhat = thongKeSach
                .Take(10)
                .ToList();

            // Giữ thuộc tính dạng chuỗi để tương thích với Module 3.
            model.SachMuonNhieuNhat = thongKeSach
                .Select(x => x.TenSach)
                .FirstOrDefault() ?? "Chưa có dữ liệu";

            // Báo cáo 5: Số phiếu mượn theo độc giả
            model.LuotMuonTheoDocGia = await _context.PhieuMuons
                .AsNoTracking()
                .GroupBy(p => p.MaDocGia)
                .Select(g => new
                {
                    MaDocGia = g.Key,
                    SoLuotMuon = g.Count()
                })
                .Join(
                    _context.DocGias.AsNoTracking(),
                    tk => tk.MaDocGia,
                    dg => dg.MaDocGia,
                    (tk, dg) => new LuotMuonTheoDocGiaThongKe
                    {
                        HoTen = dg.HoTen,
                        SoLuotMuon = tk.SoLuotMuon
                    })
                .OrderByDescending(x => x.SoLuotMuon)
                .ToListAsync();

            // Báo cáo 6: Số phiếu mượn theo tháng
            model.PhieuMuonTheoThang = await _context.PhieuMuons
                .AsNoTracking()
                .GroupBy(p => new
                {
                    p.NgayMuon.Year,
                    p.NgayMuon.Month
                })
                .Select(g => new PhieuMuonTheoThangThongKe
                {
                    Nam = g.Key.Year,
                    Thang = g.Key.Month,
                    SoPhieu = g.Count()
                })
                .OrderByDescending(x => x.Nam)
                .ThenByDescending(x => x.Thang)
                .ToListAsync();

            return View(model);
        }
    }
}