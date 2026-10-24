using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class SachController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SachController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Sach
        public async Task<IActionResult> Index()
        {
            var sachs = _context.Sachs
                .Include(s => s.TheLoai);

            return View(await sachs.ToListAsync());
        }

        // GET: Sach/Details/5
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

            return View(sach);
        }

        // GET: Sach/Create
        public async Task<IActionResult> Create()
        {
            await NapDanhSachTheLoaiAsync();
            return View();
        }

        // POST: Sach/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("MaSach,TenSach,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach,
            string? tenTheLoai)
        {
            await GanTheLoaiAsync(sach, tenTheLoai);
            await KiemTraDuLieuSachAsync(sach);

            ModelState.Remove(nameof(Sach.TheLoai));
            ModelState.Remove(nameof(Sach.MaTheLoai));

            if (ModelState.IsValid)
            {
                _context.Sachs.Add(sach);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Thêm sách thành công.";
                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachTheLoaiAsync();
            ViewBag.TenTheLoaiHienTai = tenTheLoai?.Trim();

            return View(sach);
        }

        // GET: Sach/Edit/5
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

            await NapDanhSachTheLoaiAsync();
            ViewBag.TenTheLoaiHienTai = sach.TheLoai?.TenTheLoai ?? string.Empty;

            return View(sach);
        }

        // POST: Sach/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("MaSach,TenSach,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach,
            string? tenTheLoai)
        {
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

            await GanTheLoaiAsync(sach, tenTheLoai);
            await KiemTraDuLieuSachAsync(sach);

            ModelState.Remove(nameof(Sach.TheLoai));
            ModelState.Remove(nameof(Sach.MaTheLoai));

            if (ModelState.IsValid)
            {
                // Cập nhật các trường được phép, không ghi đè dữ liệu
                // không có trong biểu mẫu.
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

                TempData["Success"] = "Cập nhật sách thành công.";
                return RedirectToAction(nameof(Index));
            }

            await NapDanhSachTheLoaiAsync();
            ViewBag.TenTheLoaiHienTai = tenTheLoai?.Trim();

            return View(sach);
        }

        // GET: Sach/Delete/5
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

            return View(sach);
        }

        // POST: Sach/Delete/5
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