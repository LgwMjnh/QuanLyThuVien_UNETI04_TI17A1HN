using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;

public class SachController : Controller
{
    private readonly ApplicationDbContext _context;

    public SachController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var sachs = _context.Set<Sach>().Include(s => s.TheLoai);
        return View(await sachs.ToListAsync());
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>()
            .Include(s => s.TheLoai)
            .FirstOrDefaultAsync(m => m.MaSach == id);

        if (sach == null)
        {
            return NotFound();
        }

        return View(sach);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.DanhSachTheLoai = await _context.TheLoais
            .Select(t => t.TenTheLoai)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaSach,TenSach,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach, string? tenTheLoai)
    {
        if (string.IsNullOrWhiteSpace(tenTheLoai))
        {
            ModelState.AddModelError("TenTheLoai", "Vui lòng nhập hoặc chọn thể loại.");
        }
        else
        {
            var cleanTenTheLoai = tenTheLoai.Trim();
            var theLoai = await _context.TheLoais
                .FirstOrDefaultAsync(t => t.TenTheLoai.ToLower() == cleanTenTheLoai.ToLower());
            if (theLoai == null)
            {
                theLoai = new TheLoai
                {
                    TenTheLoai = cleanTenTheLoai,
                    TrangThai = true
                };
                _context.TheLoais.Add(theLoai);
                await _context.SaveChangesAsync();
            }
            sach.MaTheLoai = theLoai.MaTheLoai;
        }

        if (sach.SoLuongCon > sach.SoLuong)
        {
            ModelState.AddModelError("SoLuongCon", "Số lượng còn không được vượt quá số lượng tổng.");
        }
        if (sach.GiaSach <= 0)
        {
            ModelState.AddModelError("GiaSach", "Giá sách phải lớn hơn 0.");
        }
        if (sach.NamXuatBan > DateTime.Now.Year)
        {
            ModelState.AddModelError("NamXuatBan", "Năm xuất bản không được lớn hơn năm hiện tại.");
        }
        if (sach.NamXuatBan < 1000)
        {
            ModelState.AddModelError("NamXuatBan", "Năm xuất bản không hợp lệ.");
        }

        ModelState.Remove("TheLoai");
        ModelState.Remove("MaTheLoai");

        if (ModelState.IsValid)
        {
            _context.Set<Sach>().Add(sach);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DanhSachTheLoai = await _context.TheLoais
            .Select(t => t.TenTheLoai)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
        ViewBag.TenTheLoaiHienTai = tenTheLoai?.Trim();
        return View(sach);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>()
            .Include(s => s.TheLoai)
            .FirstOrDefaultAsync(s => s.MaSach == id);
        if (sach == null)
        {
            return NotFound();
        }

        ViewBag.DanhSachTheLoai = await _context.TheLoais
            .Select(t => t.TenTheLoai)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
        ViewBag.TenTheLoaiHienTai = sach.TheLoai?.TenTheLoai ?? string.Empty;
        return View(sach);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaSach,TenSach,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach, string? tenTheLoai)
    {
        if (id != sach.MaSach)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(tenTheLoai))
        {
            ModelState.AddModelError("TenTheLoai", "Vui lòng nhập hoặc chọn thể loại.");
        }
        else
        {
            var cleanTenTheLoai = tenTheLoai.Trim();
            var theLoai = await _context.TheLoais
                .FirstOrDefaultAsync(t => t.TenTheLoai.ToLower() == cleanTenTheLoai.ToLower());
            if (theLoai == null)
            {
                theLoai = new TheLoai
                {
                    TenTheLoai = cleanTenTheLoai,
                    TrangThai = true
                };
                _context.TheLoais.Add(theLoai);
                await _context.SaveChangesAsync();
            }
            sach.MaTheLoai = theLoai.MaTheLoai;
        }

        if (sach.SoLuongCon > sach.SoLuong)
        {
            ModelState.AddModelError("SoLuongCon", "Số lượng còn không được vượt quá số lượng tổng.");
        }
        if (sach.GiaSach <= 0)
        {
            ModelState.AddModelError("GiaSach", "Giá sách phải lớn hơn 0.");
        }
        if (sach.NamXuatBan > DateTime.Now.Year)
        {
            ModelState.AddModelError("NamXuatBan", "Năm xuất bản không được lớn hơn năm hiện tại.");
        }
        if (sach.NamXuatBan < 1000)
        {
            ModelState.AddModelError("NamXuatBan", "Năm xuất bản không hợp lệ.");
        }

        ModelState.Remove("TheLoai");
        ModelState.Remove("MaTheLoai");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Set<Sach>().Update(sach);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SachExists(sach.MaSach))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        ViewBag.DanhSachTheLoai = await _context.TheLoais
            .Select(t => t.TenTheLoai)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
        ViewBag.TenTheLoaiHienTai = tenTheLoai?.Trim();
        return View(sach);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>()
            .Include(s => s.TheLoai)
            .FirstOrDefaultAsync(m => m.MaSach == id);

        if (sach == null)
        {
            return NotFound();
        }

        return View(sach);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var sach = await _context.Set<Sach>().FindAsync(id);
        if (sach != null)
        {
            _context.Set<Sach>().Remove(sach);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private bool SachExists(int id)
    {
        return _context.Set<Sach>().Any(e => e.MaSach == id);
    }
}
