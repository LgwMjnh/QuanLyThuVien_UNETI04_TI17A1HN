using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.Helpers;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class BookExtraMeta
    {
        public string MaTienTo { get; set; } = "T9LS5";
        public string HinhAnh { get; set; } = "/images/books/default-cover.svg";
    }

    public class SachController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private const string DefaultCoverPath = "/images/books/default-cover.svg";

        public SachController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index(
            string? search,
            string? theLoai,
            string? nhaXuatBan,
            string? trangThai,
            int? namTu,
            int? namDen,
            string? sort = "name_asc",
            int page = 1,
            int pageSize = 6,
            bool isAjax = false)
        {
            var metaDict = DocMetadataSach();

            var rawList = await _context.Sachs
                .Include(s => s.TheLoai)
                .AsNoTracking()
                .ToListAsync();

            foreach (var item in rawList)
            {
                ApDungMetadataChoSach(item, metaDict);
            }

            IEnumerable<Sach> query = rawList;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var clean = search.Trim().ToLower();
                query = query.Where(s =>
                    s.TenSach.ToLower().Contains(clean) ||
                    s.TacGia.ToLower().Contains(clean) ||
                    s.NhaXuatBan.ToLower().Contains(clean) ||
                    s.MaSach.ToString() == clean ||
                    s.MaDinhDanh.ToLower().Contains(clean) ||
                    (s.MaTienTo ?? "").ToLower().Contains(clean) ||
                    s.MaHauTo.ToLower().Contains(clean));
            }

            if (!string.IsNullOrWhiteSpace(theLoai))
            {
                query = query.Where(s => s.TheLoai != null && s.TheLoai.TenTheLoai == theLoai);
            }

            if (!string.IsNullOrWhiteSpace(nhaXuatBan))
            {
                query = query.Where(s => s.NhaXuatBan == nhaXuatBan);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                query = query.Where(s => s.TrangThai == trangThai);
            }

            if (namTu.HasValue)
            {
                query = query.Where(s => s.NamXuatBan >= namTu.Value);
            }
            if (namDen.HasValue)
            {
                query = query.Where(s => s.NamXuatBan <= namDen.Value);
            }

            switch (sort)
            {
                case "name_desc":
                    query = query.OrderByDescending(s => s.TenSach);
                    break;
                case "price_asc":
                    query = query.OrderBy(s => s.GiaSach);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(s => s.GiaSach);
                    break;
                case "year_asc":
                    query = query.OrderBy(s => s.NamXuatBan);
                    break;
                case "year_desc":
                    query = query.OrderByDescending(s => s.NamXuatBan);
                    break;
                case "stock_desc":
                    query = query.OrderByDescending(s => s.SoLuongCon);
                    break;
                case "stock_asc":
                    query = query.OrderBy(s => s.SoLuongCon);
                    break;
                default:
                    query = query.OrderBy(s => s.TenSach);
                    break;
            }

            var filteredList = query.ToList();
            var totalItems = filteredList.Count;
            var totalPages = (int)Math.Ceiling((double)totalItems / (pageSize > 0 ? pageSize : 1));

            if (page < 1) page = 1;
            if (totalPages > 0 && page > totalPages) page = totalPages;

            var danhSachSach = filteredList
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var danhSachTheLoai = await _context.TheLoais
                .Select(t => t.TenTheLoai)
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();

            var danhSachNXB = rawList
                .Select(s => s.NhaXuatBan)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .OrderBy(n => n)
                .ToList();

            var viewModel = new SachFilterViewModel
            {
                DanhSachSach = danhSachSach,
                PageIndex = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                Search = search,
                TheLoai = theLoai,
                NhaXuatBan = nhaXuatBan,
                TrangThai = trangThai,
                NamTu = namTu,
                NamDen = namDen,
                SortOrder = sort ?? "name_asc",
                DanhSachTheLoai = danhSachTheLoai,
                DanhSachNhaXuatBan = danhSachNXB
            };

            var isAjaxRequest = isAjax || Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (isAjaxRequest)
            {
                return PartialView("_DanhSachSachPartial", viewModel);
            }

            return View(viewModel);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }

            var metaDict = DocMetadataSach();
            ApDungMetadataChoSach(sach, metaDict);

            return View(sach);
        }

        [YeuCauAdmin]
        public async Task<IActionResult> Create()
        {
            await NapDanhSachTheLoaiAsync();
            var nextId = await LaySoThuTuTiepTheoAsync();
            ViewBag.MaTienToHienTai = SinhTienToNgauNhien();
            ViewBag.MaHauToTiepTheo = nextId.ToString("D5");
            return View();
        }

        [YeuCauAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaSach,TenSach,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach,
            string? tenTheLoai,
            string? maTienTo,
            IFormFile? anhBiaFile)
        {
            sach.SoLuong = sach.SoLuongCon;

            await GanTheLoaiAsync(sach, tenTheLoai);
            await KiemTraDuLieuSachAsync(sach);

            XoaRaoCanModelState();

            var cleanPrefix = ChuanHoaTienTo(maTienTo);

            if (ModelState.IsValid)
            {
                var tenChuan = (sach.TenSach ?? "").Trim().ToLower();
                var tacGiaChuan = (sach.TacGia ?? "").Trim().ToLower();
                var nxbChuan = (sach.NhaXuatBan ?? "").Trim().ToLower();

                var sachTonTai = await _context.Sachs
                    .FirstOrDefaultAsync(s =>
                        s.TenSach.ToLower() == tenChuan &&
                        s.TacGia.ToLower() == tacGiaChuan &&
                        s.MaTheLoai == sach.MaTheLoai &&
                        s.NhaXuatBan.ToLower() == nxbChuan &&
                        s.NamXuatBan == sach.NamXuatBan);

                if (sachTonTai != null)
                {
                    var soLuongNhapThem = sach.SoLuong > 0 ? sach.SoLuong : sach.SoLuongCon;
                    var soLuongConThem = sach.SoLuongCon > 0 ? sach.SoLuongCon : soLuongNhapThem;

                    sachTonTai.SoLuong += soLuongNhapThem;
                    sachTonTai.SoLuongCon += soLuongConThem;
                    if (sachTonTai.SoLuongCon > sachTonTai.SoLuong)
                    {
                        sachTonTai.SoLuongCon = sachTonTai.SoLuong;
                    }

                    if (sach.GiaSach > 0)
                    {
                        sachTonTai.GiaSach = sach.GiaSach;
                    }

                    if (!string.IsNullOrWhiteSpace(sach.MoTa))
                    {
                        sachTonTai.MoTa = sach.MoTa;
                    }

                    sachTonTai.TrangThai = "Đang phục vụ";

                    await _context.SaveChangesAsync();

                    var metaDict = DocMetadataSach();
                    ApDungMetadataChoSach(sachTonTai, metaDict);

                    var uploadedCover = await LuuFileAnhBiaAsync(anhBiaFile, sachTonTai.MaSach);
                    if (!string.IsNullOrWhiteSpace(uploadedCover))
                    {
                        CapNhatMetadataChoSach(sachTonTai.MaSach, sachTonTai.MaTienTo ?? cleanPrefix, uploadedCover);
                    }

                    TempData["Success"] = $"Sách '{sachTonTai.TenSach}' đã có trong thư viện (Mã #{sachTonTai.MaDinhDanh}). Đã tự động cộng dồn thêm {soLuongConThem} cuốn (Hiện còn: {sachTonTai.SoLuongCon} cuốn).";
                    return RedirectToAction(nameof(Index));
                }

                var anBanCu = await _context.Sachs
                    .FirstOrDefaultAsync(s =>
                        s.TenSach.ToLower() == tenChuan &&
                        s.TacGia.ToLower() == tacGiaChuan &&
                        s.NamXuatBan != sach.NamXuatBan);

                _context.Sachs.Add(sach);
                await _context.SaveChangesAsync();

                var coverPath = await LuuFileAnhBiaAsync(anhBiaFile, sach.MaSach) ?? DefaultCoverPath;
                CapNhatMetadataChoSach(sach.MaSach, cleanPrefix, coverPath);

                if (anBanCu != null)
                {
                    TempData["Success"] = $"Đã thêm ấn bản mới (Năm XB: {sach.NamXuatBan}) của sách '{sach.TenSach}' với mã mới #{cleanPrefix}{sach.MaSach:D5} (Tách biệt với ấn bản năm {anBanCu.NamXuatBan} đang có trong thư viện).";
                }
                else
                {
                    TempData["Success"] = $"Thêm sách mới thành công với mã #{cleanPrefix}{sach.MaSach:D5}.";
                }
                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachTheLoaiAsync();
            var nextId = await LaySoThuTuTiepTheoAsync();
            ViewBag.TenTheLoaiHienTai = tenTheLoai?.Trim();
            ViewBag.MaTienToHienTai = cleanPrefix;
            ViewBag.MaHauToTiepTheo = nextId.ToString("D5");

            return View(sach);
        }

        [YeuCauAdmin]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }

            var metaDict = DocMetadataSach();
            ApDungMetadataChoSach(sach, metaDict);

            await NapDanhSachTheLoaiAsync();
            ViewBag.TenTheLoaiHienTai = sach.TheLoai?.TenTheLoai ?? string.Empty;
            ViewBag.MaTienToHienTai = sach.MaTienTo;
            ViewBag.MaHauToHienTai = sach.MaHauTo;

            return View(sach);
        }

        [YeuCauAdmin]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaSach,TenSach,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach,
            string? tenTheLoai,
            string? maTienTo,
            IFormFile? anhBiaFile)
        {
            if (id == 0 && sach.MaSach > 0)
            {
                id = sach.MaSach;
            }

            if (id != sach.MaSach)
            {
                return NotFound();
            }

            var sachTrongDb = await _context.Sachs
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sachTrongDb == null)
            {
                return NotFound();
            }

            sach.SoLuong = Math.Max(sachTrongDb.SoLuong, sach.SoLuongCon);

            await GanTheLoaiAsync(sach, tenTheLoai);
            await KiemTraDuLieuSachAsync(sach);

            XoaRaoCanModelState();

            var cleanPrefix = ChuanHoaTienTo(maTienTo);
            var metaDict = DocMetadataSach();
            var existingCover = metaDict.TryGetValue(id, out var existingMeta) && !string.IsNullOrWhiteSpace(existingMeta.HinhAnh)
                ? existingMeta.HinhAnh
                : DefaultCoverPath;

            if (ModelState.IsValid)
            {
                sachTrongDb.TenSach = sach.TenSach;
                sachTrongDb.TacGia = sach.TacGia;
                sachTrongDb.NhaXuatBan = sach.NhaXuatBan;
                sachTrongDb.NamXuatBan = sach.NamXuatBan;
                sachTrongDb.SoLuong = sach.SoLuong;
                sachTrongDb.SoLuongCon = sach.SoLuongCon;
                sachTrongDb.GiaSach = sach.GiaSach;
                sachTrongDb.MoTa = sach.MoTa;
                sachTrongDb.TrangThai = sach.TrangThai;
                sachTrongDb.MaTheLoai = sach.MaTheLoai;

                await _context.SaveChangesAsync();

                var newCover = await LuuFileAnhBiaAsync(anhBiaFile, sach.MaSach);
                var finalCover = !string.IsNullOrWhiteSpace(newCover) ? newCover : existingCover;
                CapNhatMetadataChoSach(sach.MaSach, cleanPrefix, finalCover);

                TempData["Success"] = $"Cập nhật sách #{cleanPrefix}{sach.MaSach:D5} thành công.";
                return RedirectToAction(nameof(Index));
            }

            sach.MaTienTo = cleanPrefix;
            sach.HinhAnh = existingCover;

            await NapDanhSachTheLoaiAsync();
            ViewBag.TenTheLoaiHienTai = tenTheLoai?.Trim();
            ViewBag.MaTienToHienTai = cleanPrefix;
            ViewBag.MaHauToHienTai = sach.MaSach.ToString("D5");

            return View(sach);
        }

        [YeuCauAdmin]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sach = await _context.Sachs
                .Include(s => s.TheLoai)
                .FirstOrDefaultAsync(s => s.MaSach == id);

            if (sach == null)
            {
                return NotFound();
            }

            var metaDict = DocMetadataSach();
            ApDungMetadataChoSach(sach, metaDict);

            return View(sach);
        }

        [YeuCauAdmin]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Sachs.FindAsync(id);

            if (sach != null)
            {
                _context.Sachs.Remove(sach);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Xóa sách thành công.";
            return RedirectToAction(nameof(Index));
        }

        private void XoaRaoCanModelState()
        {
            ModelState.Remove(nameof(Sach.TheLoai));
            ModelState.Remove(nameof(Sach.MaTheLoai));
            ModelState.Remove(nameof(Sach.SoLuong));
            ModelState.Remove(nameof(Sach.MaTienTo));
            ModelState.Remove(nameof(Sach.MaHauTo));
            ModelState.Remove(nameof(Sach.MaDinhDanh));
            ModelState.Remove(nameof(Sach.HinhAnh));
        }

        private async Task<int> LaySoThuTuTiepTheoAsync()
        {
            var danhSach = await _context.Sachs.Where(s => s.MaSach < 1000).ToListAsync();
            if (danhSach.Count == 0) return 1;
            return danhSach.Max(s => s.MaSach) + 1;
        }

        private string SinhTienToNgauNhien()
        {
            string chuCai = "ABCDEFGHJKLMNPQRSTUVWXYZ";
            string chuSo = "23456789";
            Random rd = new Random();
            char c1 = chuCai[rd.Next(chuCai.Length)];
            char d1 = chuSo[rd.Next(chuSo.Length)];
            char c2 = chuCai[rd.Next(chuCai.Length)];
            char c3 = chuCai[rd.Next(chuCai.Length)];
            char d2 = chuSo[rd.Next(chuSo.Length)];
            return $"{c1}{d1}{c2}{c3}{d2}";
        }

        private string ChuanHoaTienTo(string? prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix)) return "T9LS5";
            string kq = prefix.Trim().ToUpper();
            if (kq.Length > 5) return kq.Substring(0, 5);
            if (kq.Length < 5) return kq.PadRight(5, '0');
            return kq;
        }

        private string SinhTienToMacDinhTheoId(int maSach)
        {
            string[] mauTienTo = new[]
            {
                "T9LS5", "C8IT2", "K4QT9", "N7NN3", "V5KH1",
                "B6CN4", "D3AT8", "M2AI6", "H9HD5", "P7MK2"
            };
            if (maSach == 21) return "T9LS5";
            return mauTienTo[Math.Abs(maSach) % mauTienTo.Length];
        }

        private string LayDuongDanMetaJson()
        {
            var folder = Path.Combine(_env.WebRootPath, "images", "books");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            return Path.Combine(folder, "book_meta.json");
        }

        private Dictionary<int, BookExtraMeta> DocMetadataSach()
        {
            try
            {
                var file = LayDuongDanMetaJson();
                if (!System.IO.File.Exists(file))
                {
                    return new Dictionary<int, BookExtraMeta>();
                }
                var json = System.IO.File.ReadAllText(file);
                return JsonSerializer.Deserialize<Dictionary<int, BookExtraMeta>>(json)
                       ?? new Dictionary<int, BookExtraMeta>();
            }
            catch
            {
                return new Dictionary<int, BookExtraMeta>();
            }
        }

        private void CapNhatMetadataChoSach(int maSach, string maTienTo, string hinhAnh)
        {
            var dict = DocMetadataSach();
            dict[maSach] = new BookExtraMeta
            {
                MaTienTo = ChuanHoaTienTo(maTienTo),
                HinhAnh = string.IsNullOrWhiteSpace(hinhAnh) ? DefaultCoverPath : hinhAnh
            };
            var file = LayDuongDanMetaJson();
            var json = JsonSerializer.Serialize(dict, new JsonSerializerOptions { WriteIndented = true });
            System.IO.File.WriteAllText(file, json);
        }

        private void ApDungMetadataChoSach(Sach sach, Dictionary<int, BookExtraMeta> metaDict)
        {
            if (metaDict.TryGetValue(sach.MaSach, out var meta))
            {
                sach.MaTienTo = string.IsNullOrWhiteSpace(meta.MaTienTo)
                    ? SinhTienToMacDinhTheoId(sach.MaSach)
                    : meta.MaTienTo;
                sach.HinhAnh = string.IsNullOrWhiteSpace(meta.HinhAnh)
                    ? DefaultCoverPath
                    : meta.HinhAnh;
            }
            else
            {
                sach.MaTienTo = SinhTienToMacDinhTheoId(sach.MaSach);
                sach.HinhAnh = DefaultCoverPath;
            }
        }

        private async Task<string?> LuuFileAnhBiaAsync(IFormFile? file, int maSach)
        {
            if (file == null || file.Length == 0) return null;

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".gif", ".svg" };
            if (!allowed.Contains(ext))
            {
                ext = ".png";
            }

            var folder = Path.Combine(_env.WebRootPath, "images", "books");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            var fileName = $"cover_{maSach}_{DateTime.Now:yyyyMMddHHmmss}{ext}";
            var fullPath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/images/books/{fileName}";
        }

        private async Task GanTheLoaiAsync(Sach sach, string? tenTheLoai)
        {
            if (string.IsNullOrWhiteSpace(tenTheLoai))
            {
                ModelState.AddModelError(
                    "TenTheLoai",
                    "Vui lòng nhập hoặc chọn thể loại.");
                return;
            }

            var tenChuanHoa = tenTheLoai.Trim();

            var theLoai = await _context.TheLoais
                .FirstOrDefaultAsync(t =>
                    t.TenTheLoai.ToLower() == tenChuanHoa.ToLower());

            if (theLoai == null)
            {
                theLoai = new TheLoai
                {
                    TenTheLoai = tenChuanHoa,
                    TrangThai = true
                };

                _context.TheLoais.Add(theLoai);
                await _context.SaveChangesAsync();
            }

            sach.MaTheLoai = theLoai.MaTheLoai;
        }

        private async Task KiemTraDuLieuSachAsync(Sach sach)
        {
            if (sach.SoLuong < 0)
            {
                ModelState.AddModelError(
                    nameof(Sach.SoLuong),
                    "Số lượng tổng không được âm.");
            }

            if (sach.SoLuongCon < 0 || sach.SoLuongCon > sach.SoLuong)
            {
                ModelState.AddModelError(
                    nameof(Sach.SoLuongCon),
                    "Số lượng còn phải từ 0 đến số lượng tổng.");
            }

            if (sach.GiaSach <= 0)
            {
                ModelState.AddModelError(
                    nameof(Sach.GiaSach),
                    "Giá sách phải lớn hơn 0.");
            }

            if (sach.NamXuatBan > DateTime.Now.Year || sach.NamXuatBan < 1000)
            {
                ModelState.AddModelError(
                    nameof(Sach.NamXuatBan),
                    "Năm xuất bản không hợp lệ.");
            }

            await Task.CompletedTask;
        }

        private async Task NapDanhSachTheLoaiAsync()
        {
            ViewBag.DanhSachTheLoai = await _context.TheLoais
                .Select(t => t.TenTheLoai)
                .Distinct()
                .OrderBy(t => t)
                .ToListAsync();
        }

        private bool SachExists(int id)
        {
            return _context.Sachs.Any(s => s.MaSach == id);
        }
    }
}
