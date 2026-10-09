
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;

public class TaiKhoanController : Controller
{
    private readonly ApplicationDbContext _context;

    public TaiKhoanController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: TAIKHOANS
    public async Task<IActionResult> Index()
    {
        return View(await _context.TaiKhoans.ToListAsync());
    }

    // GET: TAIKHOANS/Details/5
    public async Task<IActionResult> Details(int? mataikhoan)
    {
        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(m => m.MaTaiKhoan == mataikhoan);
        if (taikhoan == null)
        {
            return NotFound();
        }

        return View(taikhoan);
    }

    // GET: TAIKHOANS/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TAIKHOANS/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,VaiTro,TrangThai")] TaiKhoan taikhoan)
    {
        if (ModelState.IsValid)
        {
            _context.Add(taikhoan);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        return View(taikhoan);
    }

    // GET: TAIKHOANS/Edit/5
    public async Task<IActionResult> Edit(int? mataikhoan)
    {
        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans.FindAsync(mataikhoan);
        if (taikhoan == null)
        {
            return NotFound();
        }
        return View(taikhoan);
    }

    // POST: TAIKHOANS/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int? mataikhoan, [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,VaiTro,TrangThai")] TaiKhoan taikhoan)
    {
        if (mataikhoan != taikhoan.MaTaiKhoan)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(taikhoan);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TaiKhoanExists(taikhoan.MaTaiKhoan))
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
        return View(taikhoan);
    }

    // GET: TAIKHOANS/Delete/5
    public async Task<IActionResult> Delete(int? mataikhoan)
    {
        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(m => m.MaTaiKhoan == mataikhoan);
        if (taikhoan == null)
        {
            return NotFound();
        }

        return View(taikhoan);
    }

    // POST: TAIKHOANS/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? mataikhoan)
    {
        var taikhoan = await _context.TaiKhoans.FindAsync(mataikhoan);
        if (taikhoan != null)
        {
            _context.TaiKhoans.Remove(taikhoan);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool TaiKhoanExists(int? mataikhoan)
    {
        return _context.TaiKhoans.Any(e => e.MaTaiKhoan == mataikhoan);
    }
}
