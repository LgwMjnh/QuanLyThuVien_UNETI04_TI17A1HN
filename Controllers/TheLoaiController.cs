
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class TheLoaiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TheLoaiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TheLoai
        public async Task<IActionResult> Index()
        {
            return View(await _context.TheLoais.ToListAsync());
        }

        // GET: TheLoai/Details/5
        public async Task<IActionResult> Details(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theloai = await _context.TheLoais
                .FirstOrDefaultAsync(m => m.MaTheLoai == matheloai);

            if (theloai == null)
            {
                return NotFound();
            }

            return View(theloai);
        }

        // GET: TheLoai/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TheLoai/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("TenTheLoai,MoTa,TrangThai")] TheLoai theloai)
        {
            theloai.TenTheLoai = theloai.TenTheLoai?.Trim() ?? string.Empty;

            bool trungTen = await _context.TheLoais
                .AnyAsync(t => t.TenTheLoai.ToLower() == theloai.TenTheLoai.ToLower());

            if (trungTen)
            {
                ModelState.AddModelError(
                    nameof(theloai.TenTheLoai),
                    "Tên thể loại này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(theloai);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(theloai);
        }

        // GET: TheLoai/Edit/5
        public async Task<IActionResult> Edit(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theloai = await _context.TheLoais.FindAsync(matheloai);

            if (theloai == null)
            {
                return NotFound();
            }

            return View(theloai);
        }

        // POST: TheLoai/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int? matheloai,
            [Bind("MaTheLoai,TenTheLoai,MoTa,TrangThai")] TheLoai theloai)
        {
            if (matheloai != theloai.MaTheLoai)
            {
                return NotFound();
            }

            theloai.TenTheLoai = theloai.TenTheLoai?.Trim() ?? string.Empty;

            bool trungTen = await _context.TheLoais.AnyAsync(t =>
                t.TenTheLoai.ToLower() == theloai.TenTheLoai.ToLower()
                && t.MaTheLoai != theloai.MaTheLoai);

            if (trungTen)
            {
                ModelState.AddModelError(
                    nameof(theloai.TenTheLoai),
                    "Tên thể loại này đã tồn tại.");
            }

            if (ModelState.IsValid)
            {
                var theLoaiCu = await _context.TheLoais.FindAsync(matheloai);

                if (theLoaiCu == null)
                {
                    return NotFound();
                }

                theLoaiCu.TenTheLoai = theloai.TenTheLoai;
                theLoaiCu.MoTa = theloai.MoTa;
                theLoaiCu.TrangThai = theloai.TrangThai;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(theloai);
        }

        // GET: TheLoai/Delete/5
        public async Task<IActionResult> Delete(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theloai = await _context.TheLoais
                .FirstOrDefaultAsync(m => m.MaTheLoai == matheloai);

            if (theloai == null)
            {
                return NotFound();
            }

            return View(theloai);
        }

        // POST: TheLoai/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? matheloai)
        {
            if (matheloai == null)
            {
                return NotFound();
            }

            var theloai = await _context.TheLoais.FindAsync(matheloai);

            if (theloai == null)
            {
                return NotFound();
            }

            bool coSach = await _context.Sachs
                .AnyAsync(s => s.MaTheLoai == matheloai);

            if (coSach)
            {
                TempData["Loi"] =
                    "Không thể xóa thể loại đang có sách. Vui lòng chuyển sách sang thể loại khác trước.";

                return RedirectToAction(nameof(Index));
            }

            _context.TheLoais.Remove(theloai);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TheLoaiExists(int matheloai)
        {
            return _context.TheLoais.Any(e => e.MaTheLoai == matheloai);
        }
    }
}
