
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using System.Diagnostics;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var homNay = DateTime.Today;

            var model = new DashboardViewModel
            {
                TongPhieuMuon = await _context.PhieuMuons.CountAsync(),

                DangMuon = await _context.PhieuMuons.CountAsync(p =>
                    p.NgayTra == null &&
                    p.TrangThai == "Đang mượn" &&
                    p.HanTra >= homNay),

                DaTra = await _context.PhieuMuons.CountAsync(p =>
                    p.NgayTra != null ||
                    p.TrangThai == "Đã trả"),

                QuaHan = await _context.PhieuMuons.CountAsync(p =>
                    p.NgayTra == null &&
                    p.TrangThai == "Đang mượn" &&
                    p.HanTra < homNay),

                PhieuMuonQuaHan = await _context.PhieuMuons
                    .Include(p => p.DocGia)
                    .Where(p =>
                        p.NgayTra == null &&
                        p.TrangThai == "Đang mượn" &&
                        p.HanTra < homNay)
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
