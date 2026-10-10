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

    // GET: SACHS
    public async Task<IActionResult> Index()
    {
        var sachs = _context.Set<Sach>().Include(s => s.TheLoai);
        return View(await sachs.ToListAsync());
    }

    // GET: SACHS/Details/5
    public async Task<IActionResult> Details(int? masach)
    {
        if (masach == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>()
            .Include(s => s.TheLoai)
            .FirstOrDefaultAsync(m => m.MaSach == masach);

        if (sach == null)
        {
            return NotFound();
        }

        return View(sach);
    }

    // GET: SACHS/Create
    public IActionResult Create()
    {
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai");
        return View();
    }

    // POST: SACHS/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaSach,TenSach,MaTheLoai,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach)
    {
        if (ModelState.IsValid)
        {
            _context.Set<Sach>().Add(sach);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
        return View(sach);
    }

    // GET: SACHS/Edit/5
    public async Task<IActionResult> Edit(int? masach)
    {
        if (masach == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>().FindAsync(masach);
        if (sach == null)
        {
            return NotFound();
        }
        ViewData["MaTheLoai"] = new SelectList(_context.TheLoais, "MaTheLoai", "TenTheLoai", sach.MaTheLoai);
        return View(sach);
    }

    // POST: SACHS/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int masach, [Bind("MaSach,TenSach,MaTheLoai,TacGia,NhaXuatBan,NamXuatBan,SoLuong,SoLuongCon,GiaSach,MoTa,TrangThai")] Sach sach)
    {
        if (masach != sach.MaSach)
        {
            return NotFound();
        }

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

    // GET: SACHS/Delete/5
    public async Task<IActionResult> Delete(int? masach)
    {
        if (masach == null)
        {
            return NotFound();
        }

        var sach = await _context.Set<Sach>()
            .Include(s => s.TheLoai)
            .FirstOrDefaultAsync(m => m.MaSach == masach);

        if (sach == null)
        {
            return NotFound();
        }

        return View(sach);
    }

    // POST: SACHS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int masach)
    {
        var sach = await _context.Set<Sach>().FindAsync(masach);
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
