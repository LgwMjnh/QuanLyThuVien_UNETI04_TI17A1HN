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

    public IActionResult Create()
    {
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaSach,TenSach,MaTheLoai,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach)
    {
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

        if (ModelState.IsValid)
        {
            _context.Set<Sach>().Add(sach);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
        return View(sach);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>().FindAsync(id);
        if (sach == null)
        {
            return NotFound();
        }
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
        return View(sach);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("MaSach,TenSach,MaTheLoai,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach)
    {
        if (id != sach.MaSach)
        {
            return NotFound();
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
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
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
