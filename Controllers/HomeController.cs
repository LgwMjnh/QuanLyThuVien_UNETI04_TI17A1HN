using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using System.Diagnostics;
using QuanLyThuVien_UNETI04_TI17A1HN.Helpers;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [YeuCauAdmin]
        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;

            var phieuMuon = _context.PhieuMuons
                .AsNoTracking();

            var model = new DashboardViewModel
            {
                // Tổng số phiếu mượn
                TongPhieuMuon = await phieuMuon.CountAsync(),

                // Phiếu đang mượn và chưa quá hạn
                DangMuon = await phieuMuon.CountAsync(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.HanTra >= homNay &&
                    p.ChiTietPhieuMuons.Any(ct => ct.NgayTra == null)),

                // Phiếu đã trả hết sách
                DaTra = await phieuMuon.CountAsync(p =>
                    p.TrangThai == "Đã trả" ||
                    (p.ChiTietPhieuMuons.Any() &&
                     p.ChiTietPhieuMuons.All(ct => ct.NgayTra != null))),

                // Phiếu đang mượn và đã quá hạn
                QuaHan = await phieuMuon.CountAsync(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.HanTra < homNay &&
                    p.ChiTietPhieuMuons.Any(ct => ct.NgayTra == null)),

                // Danh sách tối đa 10 phiếu quá hạn
                PhieuMuonQuaHan = await phieuMuon
                    .Include(p => p.DocGia)
                    .Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.HanTra < homNay &&
                        p.ChiTietPhieuMuons.Any(ct => ct.NgayTra == null))
                    .OrderBy(p => p.HanTra)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(
            Duration = 0,
            Location = ResponseCacheLocation.None,
            NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
                    ?? HttpContext.TraceIdentifier
            });
        }
    }
}