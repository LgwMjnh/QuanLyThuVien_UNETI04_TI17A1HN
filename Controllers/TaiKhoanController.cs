
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

public class TaiKhoanController : Controller
{
    private readonly ApplicationDbContext _context;

    public TaiKhoanController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: TaiKhoans
    public async Task<IActionResult> Index()
    {
        return View(await _context.TaiKhoans.ToListAsync());
    }

    // GET: TaiKhoans/Details/5
    public async Task<IActionResult> Details(int? mataikhoan)
    {
        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == mataikhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        return View(taikhoan);
    }

    // GET: TaiKhoans/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: TaiKhoans/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,VaiTro")] TaiKhoan taikhoan)
    {
        // Kiểm tra tên đăng nhập đã tồn tại hay chưa.
        bool tenDangNhapDaTonTai = await _context.TaiKhoans
            .AnyAsync(tk => tk.TenDangNhap == taikhoan.TenDangNhap);

        if (tenDangNhapDaTonTai)
        {
            ModelState.AddModelError(
                nameof(taikhoan.TenDangNhap),
                "Tên đăng nhập này đã tồn tại.");
        }

        // Tài khoản mới mặc định được kích hoạt.
        taikhoan.TrangThai = true;

        if (ModelState.IsValid)
        {
            _context.TaiKhoans.Add(taikhoan);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        return View(taikhoan);
    }

    // GET: TaiKhoans/Edit/5
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

    // POST: TaiKhoans/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int? mataikhoan,
        [Bind("MaTaiKhoan,TenDangNhap,MatKhau,HoTen,VaiTro,TrangThai")] TaiKhoan taikhoan)
    {
        if (mataikhoan == null || mataikhoan != taikhoan.MaTaiKhoan)
        {
            return NotFound();
        }

        // Không cho phép trùng tên đăng nhập với tài khoản khác.
        bool tenDangNhapDaTonTai = await _context.TaiKhoans
            .AnyAsync(tk =>
                tk.TenDangNhap == taikhoan.TenDangNhap &&
                tk.MaTaiKhoan != taikhoan.MaTaiKhoan);

        if (tenDangNhapDaTonTai)
        {
            ModelState.AddModelError(
                nameof(taikhoan.TenDangNhap),
                "Tên đăng nhập này đã tồn tại.");
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

                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        return View(taikhoan);
    }

    // GET: TaiKhoans/Delete/5
    public async Task<IActionResult> Delete(int? mataikhoan)
    {
        if (mataikhoan == null)
        {
            return NotFound();
        }

        var taikhoan = await _context.TaiKhoans
            .FirstOrDefaultAsync(tk => tk.MaTaiKhoan == mataikhoan);

        if (taikhoan == null)
        {
            return NotFound();
        }

        return View(taikhoan);
    }

    // POST: TaiKhoans/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? mataikhoan)
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

        _context.TaiKhoans.Remove(taikhoan);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private bool TaiKhoanExists(int mataikhoan)
    {
        return _context.TaiKhoans.Any(tk => tk.MaTaiKhoan == mataikhoan);
    }
}
