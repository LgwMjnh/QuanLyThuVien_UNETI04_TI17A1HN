
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class TrangChuController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public TrangChuController(
            IWebHostEnvironment env,
            ApplicationDbContext context)
        {
            _env = env;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // =============================================
            // 1. LẤY DANH SÁCH 4 SÁCH MỚI BỔ SUNG
            // =============================================

            var danhSachSachMoi = await _context.Sachs
                .Include(s => s.TheLoai)
                .Where(s => s.TrangThai == "Đang phục vụ")
                .OrderByDescending(s => s.MaSach)
                .Take(4)
                .AsNoTracking()
                .ToListAsync();

            // =============================================
            // 2. LẤY MÃ 4 SÁCH NỔI BẬT
            // Dựa trên tổng số lượng sách đã được mượn
            // =============================================

            var maSachNoiBat = await _context.ChiTietPhieuMuons
                .AsNoTracking()
                .GroupBy(ct => ct.MaSach)
                .Select(g => new
                {
                    MaSach = g.Key,
                    TongSoLuong = g.Sum(ct => ct.SoLuongMuon)
                })
                .Join(
                    _context.Sachs
                        .AsNoTracking()
                        .Where(s => s.TrangThai == "Đang phục vụ"),
                    thongKe => thongKe.MaSach,
                    sach => sach.MaSach,
                    (thongKe, sach) => new
                    {
                        thongKe.MaSach,
                        thongKe.TongSoLuong,
                        sach.TenSach
                    })
                .OrderByDescending(x => x.TongSoLuong)
                .ThenBy(x => x.MaSach)
                .Take(4)
                .Select(x => x.MaSach)
                .ToListAsync();

            // Lấy thông tin chi tiết của các sách nổi bật
            var sachNoiBatTheoMa = await _context.Sachs
                .Include(s => s.TheLoai)
                .Where(s => maSachNoiBat.Contains(s.MaSach))
                .AsNoTracking()
                .ToListAsync();

            // Sắp xếp lại theo thứ tự lượt mượn
            var danhSachSachNoiBat = maSachNoiBat
                .Select(ma => sachNoiBatTheoMa
                    .FirstOrDefault(s => s.MaSach == ma))
                .Where(s => s != null)
                .Cast<Sach>()
                .ToList();

            // =============================================
            // 3. ĐỌC THÔNG TIN ẢNH BÌA TỪ FILE JSON
            // =============================================

            var duongDanMetadata = Path.Combine(
                _env.WebRootPath,
                "images",
                "books",
                "book_meta.json"
            );

            var metadataSach = new Dictionary<int, BookExtraMeta>();

            if (System.IO.File.Exists(duongDanMetadata))
            {
                try
                {
                    var json = await System.IO.File.ReadAllTextAsync(
                        duongDanMetadata
                    );

                    metadataSach =
                        JsonSerializer.Deserialize<Dictionary<int, BookExtraMeta>>(json)
                        ?? new Dictionary<int, BookExtraMeta>();
                }
                catch
                {
                    metadataSach = new Dictionary<int, BookExtraMeta>();
                }
            }

            // =============================================
            // 4. GÁN ẢNH BÌA CHO CÁC CUỐN SÁCH
            // =============================================

            foreach (var sach in danhSachSachMoi.Concat(danhSachSachNoiBat))
            {
                if (metadataSach.TryGetValue(sach.MaSach, out var metadata)
                    && !string.IsNullOrWhiteSpace(metadata.HinhAnh))
                {
                    sach.HinhAnh = metadata.HinhAnh;
                }
                else
                {
                    sach.HinhAnh = "/images/books/default-cover.svg";
                }
            }

            // =============================================
            // 5. LẤY DANH SÁCH THỂ LOẠI GỢI Ý
            // Chỉ lấy thể loại đang hoạt động và có sách phục vụ
            // =============================================

            var danhSachTheLoaiGoiY = await _context.TheLoais
                .AsNoTracking()
                .Where(t => t.TrangThai
                    && _context.Sachs.Any(s =>
                        s.MaTheLoai == t.MaTheLoai
                        && s.TrangThai == "Đang phục vụ"))
                .OrderBy(t => t.TenTheLoai)
                .Take(10)
                .ToListAsync();

            // =============================================
            // 6. GỬI DANH SÁCH DỮ LIỆU SANG VIEW
            // =============================================

            ViewBag.DanhSachSachMoi = danhSachSachMoi;
            ViewBag.DanhSachSachNoiBat = danhSachSachNoiBat;
            ViewBag.DanhSachTheLoaiGoiY = danhSachTheLoaiGoiY;

            // =============================================
            // 7. ĐỌC DANH SÁCH ẢNH SLIDESHOW
            // =============================================

            var thuMucSlides = Path.Combine(
                _env.WebRootPath,
                "images",
                "trang_chu",
                "slides"
            );

            // Tự tạo thư mục nếu chưa tồn tại
            Directory.CreateDirectory(thuMucSlides);

            // Các định dạng ảnh được hỗ trợ
            var dinhDangAnh = new[]
            {
                ".jpg",
                ".jpeg",
                ".png",
                ".webp"
            };

            // Đọc và sắp xếp danh sách ảnh theo tên
            var danhSachAnh = Directory
                .EnumerateFiles(
                    thuMucSlides,
                    "*.*",
                    SearchOption.TopDirectoryOnly
                )
                .Where(duongDan =>
                    dinhDangAnh.Contains(
                        Path.GetExtension(duongDan),
                        System.StringComparer.OrdinalIgnoreCase
                    )
                )
                .OrderBy(
                    duongDan => Path.GetFileName(duongDan),
                    System.StringComparer.OrdinalIgnoreCase
                )
                .Select(duongDan =>
                {
                    var tenAnh = Path.GetFileName(duongDan);

                    return Url.Content(
                        "~/images/trang_chu/slides/" +
                        System.Uri.EscapeDataString(tenAnh)
                    );
                })
                .ToList();

            // Gửi danh sách ảnh sang View
            ViewBag.DanhSachSlides = danhSachAnh;

            return View();
        }
    }
}
