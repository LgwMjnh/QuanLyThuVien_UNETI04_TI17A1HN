
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using QuanLyThuVien_UNETI04_TI17A1HN.Data;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Controllers
{
    public class MuonTraController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MuonTraController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // DANH SÁCH VÀ QUẢN LÝ PHIẾU MƯỢN
        // ==========================================

        public async Task<IActionResult> Index(
            string? trangThai,
            string? tuKhoa,
            DateTime? ngayTu,
            DateTime? ngayDen,
            int page = 1)
        {
            var query = _context.PhieuMuons
                .Include(p => p.DocGia)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                .AsQueryable();

            switch (trangThai)
            {
                case "ChoXacNhan":
                    query = query.Where(p =>
                        p.TrangThai == "Chờ xác nhận");
                    break;

                case "DangMuon":
                    query = query.Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.NgayTra == null &&
                        p.HanTra >= DateTime.Today);
                    break;

                case "QuaHan":
                    query = query.Where(p =>
                        p.TrangThai == "Đang mượn" &&
                        p.NgayTra == null &&
                        p.HanTra < DateTime.Today);
                    break;

                case "DaTra":
                    query = query.Where(p =>
                        p.TrangThai == "Đã trả" ||
                        p.NgayTra != null);
                    break;

                case "TuChoi":
                    query = query.Where(p =>
                        p.TrangThai == "Từ chối");
                    break;
            }

            // Tìm kiếm theo tên độc giả
            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                string tuKhoaTimKiem = tuKhoa.Trim();

                query = query.Where(p =>
                    p.DocGia != null &&
                    p.DocGia.HoTen.Contains(tuKhoaTimKiem));
            }

            // Lọc từ ngày
            if (ngayTu.HasValue)
            {
                query = query.Where(p =>
                    p.NgayMuon >= ngayTu.Value.Date);
            }

            // Lọc đến ngày
            if (ngayDen.HasValue)
            {
                var den = ngayDen.Value.Date.AddDays(1);

                query = query.Where(p =>
                    p.NgayMuon < den);
            }

            // Sắp xếp theo mã phiếu tăng dần
            query = query.OrderBy(p => p.MaPhieuMuon);

            const int soLuongTrenTrang = 10;

            int tongSoPhieu = await query.CountAsync();

            int tongSoTrang = (int)Math.Ceiling(
                tongSoPhieu / (double)soLuongTrenTrang);

            page = Math.Max(1, page);

            if (tongSoTrang > 0 && page > tongSoTrang)
            {
                page = tongSoTrang;
            }

            var danhSachPhieu = await query
                .Skip((page - 1) * soLuongTrenTrang)
                .Take(soLuongTrenTrang)
                .ToListAsync();

            var homNay = DateTime.Today;

            // Thống kê tổng số phiếu
            ViewBag.TongPhieu =
                await _context.PhieuMuons.CountAsync();

            ViewBag.ChoXacNhan =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Chờ xác nhận");

            ViewBag.DangMuon =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.NgayTra == null &&
                    p.HanTra >= homNay);

            ViewBag.QuaHan =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.NgayTra == null &&
                    p.HanTra < homNay);

            ViewBag.DaTra =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Đã trả" ||
                    p.NgayTra != null);

            ViewBag.TuChoi =
                await _context.PhieuMuons.CountAsync(p =>
                    p.TrangThai == "Từ chối");

            // Giữ lại thông tin lọc và phân trang
            ViewBag.TrangThaiLoc = trangThai;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.NgayTu = ngayTu;
            ViewBag.NgayDen = ngayDen;
            ViewBag.TrangHienTai = page;
            ViewBag.TongSoTrang = tongSoTrang;

            return View(danhSachPhieu);
        }

        // ==========================================
        // TRA CỨU ĐỘC GIẢ THEO MÃ ĐỘC GIẢ
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> TraCuuDocGia(int? maDocGia)
        {
            var model = new TraCuuDocGiaViewModel
            {
                MaDocGia = maDocGia
            };

            // Chưa nhập mã độc giả
            if (!maDocGia.HasValue)
            {
                return View(model);
            }

            model.DaTimKiem = true;

            // Kiểm tra mã độc giả
            if (maDocGia.Value <= 0)
            {
                model.ThongBao =
                    "Mã độc giả phải là số nguyên lớn hơn 0.";

                return View(model);
            }

            // Tìm thông tin độc giả
            var docGia = await _context.DocGias
                .AsNoTracking()
                .FirstOrDefaultAsync(d =>
                    d.MaDocGia == maDocGia.Value);

            if (docGia == null)
            {
                model.ThongBao =
                    $"Không tìm thấy độc giả có mã {maDocGia.Value}.";

                return View(model);
            }

            model.DocGia = docGia;

            // Lấy toàn bộ lịch sử phiếu mượn của độc giả
            var lichSu = await _context.PhieuMuons
                .AsNoTracking()
                .Where(p => p.MaDocGia == maDocGia.Value)
                .Include(p => p.ChiTietPhieuMuons)
                    .ThenInclude(ct => ct.Sach)
                .OrderByDescending(p => p.NgayMuon)
                .ThenByDescending(p => p.MaPhieuMuon)
                .ToListAsync();

            model.LichSu = lichSu;

            var homNay = DateTime.Today;

            // Phiếu chờ xác nhận
            var choXacNhan = lichSu
                .Where(p => p.TrangThai == "Chờ xác nhận")
                .ToList();

            // Phiếu đang mượn và chưa quá hạn
            var dangMuon = lichSu
                .Where(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.NgayTra == null &&
                    p.HanTra.Date >= homNay)
                .ToList();

            // Phiếu quá hạn
            var quaHan = lichSu
                .Where(p =>
                    p.TrangThai == "Đang mượn" &&
                    p.NgayTra == null &&
                    p.HanTra.Date < homNay)
                .ToList();

            // Phiếu đã trả
            var daTra = lichSu
                .Where(p =>
                    p.TrangThai == "Đã trả" ||
                    p.NgayTra != null)
                .ToList();

            // Phiếu bị từ chối
            var tuChoi = lichSu
                .Where(p => p.TrangThai == "Từ chối")
                .ToList();

            // Phiếu đã hủy cũ (nếu có)
            var daHuy = lichSu
                .Where(p => p.TrangThai == "Đã hủy")
                .ToList();

            // Những phiếu đã được xác nhận cho mượn
            // Bao gồm phiếu đang mượn, quá hạn và đã trả
            var phieuDaMuon = lichSu
                .Where(p =>
                    p.TrangThai == "Đang mượn" ||
                    p.TrangThai == "Đã trả" ||
                    p.NgayTra != null)
                .ToList();

            // Thống kê số lượng phiếu
            model.TongSoPhieu = lichSu.Count;

            model.SoLanMuon = phieuDaMuon.Count;

            model.SoPhieuChoXacNhan = choXacNhan.Count;

            model.SoPhieuDangMuon = dangMuon.Count;

            model.SoPhieuQuaHan = quaHan.Count;

            model.SoPhieuDaTra = daTra.Count;

            model.SoPhieuTuChoi = tuChoi.Count;

            model.SoPhieuDaHuy = daHuy.Count;

            // Tổng số sách đã được xác nhận cho mượn
            model.TongSoSachDaMuon = phieuDaMuon
                .SelectMany(p => p.ChiTietPhieuMuons)
                .Sum(ct => ct.SoLuongMuon);

            // Tổng số sách thuộc các phiếu đã trả
            model.SoSachDaTra = daTra
                .SelectMany(p => p.ChiTietPhieuMuons)
                .Sum(ct => ct.SoLuongMuon);

            // Số sách thuộc các phiếu đang mượn
            model.SoSachDangMuon = dangMuon
                .SelectMany(p => p.ChiTietPhieuMuons)
                .Sum(ct => ct.SoLuongMuon);

            // Số sách thuộc các phiếu quá hạn
            model.SoSachQuaHan = quaHan
                .SelectMany(p => p.ChiTietPhieuMuons)
                .Sum(ct => ct.SoLuongMuon);

            return View(model);
        }

        // ==========================================
        // NẠP DỮ LIỆU CHO FORM LẬP PHIẾU
        // ==========================================

        private async Task<List<Sach>> NapDuLieuForm()
        {
            var docGias = await _context.DocGias
                .OrderBy(d => d.HoTen)
                .ToListAsync();

            var theLoais = await _context.TheLoais
                .Where(t => t.TrangThai == true)
                .OrderBy(t => t.TenTheLoai)
                .ToListAsync();

            var sach = await _context.Sachs
                .Where(s =>
                    s.SoLuongCon > 0 &&
                    s.TrangThai == "Đang phục vụ")
                .OrderBy(s => s.TenSach)
                .ToListAsync();

            ViewBag.DocGias = new SelectList(
                docGias,
                "MaDocGia",
                "HoTen"
            );

            ViewBag.TheLoais = theLoais;

            return sach;
        }

        // ==========================================
        // HIỂN THỊ FORM LẬP PHIẾU
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var sach = await NapDuLieuForm();

            ViewBag.NgayMuon = DateTime.Today;
            ViewBag.HanTra = DateTime.Today.AddDays(14);

            return View(sach);
        }

        // ==========================================
        // TRẢ LẠI FORM KHI CÓ LỖI
        // ==========================================

        private async Task<IActionResult> TraVeFormLoi(
            string thongBao,
            DateTime ngayMuon,
            DateTime hanTra,
            int maDocGia = 0,
            int maTheLoai = 0,
            int maSach = 0,
            int soLuong = 1)
        {
            TempData["Loi"] = thongBao;

            var sach = await NapDuLieuForm();

            ViewBag.NgayMuon = ngayMuon;
            ViewBag.HanTra = hanTra;

            ViewBag.MaDocGia = maDocGia;
            ViewBag.MaTheLoai = maTheLoai;
            ViewBag.MaSach = maSach;
            ViewBag.SoLuong = soLuong;

            return View("Create", sach);
        }

        // ==========================================
        // LẬP PHIẾU MƯỢN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int MaDocGia,
            int MaTheLoai,
            int MaSach,
            int SoLuong,
            DateTime NgayMuon,
            DateTime HanTra)
        {
            if (NgayMuon == DateTime.MinValue ||
                HanTra == DateTime.MinValue)
            {
                return await TraVeFormLoi(
                    "Vui lòng chọn đầy đủ ngày mượn và ngày trả dự kiến.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            NgayMuon = NgayMuon.Date;
            HanTra = HanTra.Date;

            if (HanTra < NgayMuon)
            {
                return await TraVeFormLoi(
                    "Ngày trả dự kiến không được trước ngày mượn.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            var docGia = await _context.DocGias
                .FirstOrDefaultAsync(d =>
                    d.MaDocGia == MaDocGia);

            if (docGia == null)
            {
                return await TraVeFormLoi(
                    "Vui lòng chọn độc giả hợp lệ.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            var theLoai = await _context.TheLoais
                .FirstOrDefaultAsync(t =>
                    t.MaTheLoai == MaTheLoai &&
                    t.TrangThai == true);

            if (theLoai == null)
            {
                return await TraVeFormLoi(
                    "Vui lòng chọn thể loại sách hợp lệ.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            var sach = await _context.Sachs
                .FirstOrDefaultAsync(s =>
                    s.MaSach == MaSach &&
                    s.MaTheLoai == MaTheLoai);

            if (sach == null)
            {
                return await TraVeFormLoi(
                    "Sách không thuộc thể loại đã chọn hoặc không tồn tại.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            if (SoLuong <= 0)
            {
                return await TraVeFormLoi(
                    "Số lượng mượn phải lớn hơn 0.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            if (sach.TrangThai != "Đang phục vụ")
            {
                return await TraVeFormLoi(
                    "Sách hiện không được phục vụ cho mượn.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            if (sach.SoLuongCon <= 0 ||
                SoLuong > sach.SoLuongCon)
            {
                return await TraVeFormLoi(
                    $"Sách hiện chỉ còn {sach.SoLuongCon} cuốn.",
                    NgayMuon,
                    HanTra,
                    MaDocGia,
                    MaTheLoai,
                    MaSach,
                    SoLuong
                );
            }

            var phieuMuon = new PhieuMuon
            {
                MaDocGia = MaDocGia,
                NgayMuon = NgayMuon,
                HanTra = HanTra,
                TrangThai = "Chờ xác nhận"
            };

            _context.PhieuMuons.Add(phieuMuon);

            await _context.SaveChangesAsync();

            var chiTiet = new ChiTietPhieuMuon
            {
                MaPhieuMuon = phieuMuon.MaPhieuMuon,
                MaSach = MaSach,
                SoLuongMuon = SoLuong
            };

            _context.ChiTietPhieuMuons.Add(chiTiet);

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                $"Đã tạo phiếu mượn số {phieuMuon.MaPhieuMuon} thành công. Phiếu đang chờ xác nhận.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // XÁC NHẬN PHIẾU MƯỢN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhan(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var phieuMuon = await _context.PhieuMuons
                    .Include(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(p =>
                        p.MaPhieuMuon == id);

                if (phieuMuon == null)
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                    return RedirectToAction(nameof(Index));
                }

                if (phieuMuon.TrangThai != "Chờ xác nhận")
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] =
                        "Chỉ xác nhận được phiếu đang chờ xác nhận.";
                    return RedirectToAction(nameof(Index));
                }

                if (!phieuMuon.ChiTietPhieuMuons.Any())
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] = "Phiếu chưa có chi tiết sách.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    if (ct.Sach == null ||
                        ct.SoLuongMuon <= 0 ||
                        ct.Sach.TrangThai != "Đang phục vụ" ||
                        ct.Sach.SoLuongCon < ct.SoLuongMuon)
                    {
                        await transaction.RollbackAsync();
                        TempData["Loi"] =
                            "Sách không còn đủ số lượng hoặc không còn phục vụ.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    ct.Sach!.SoLuongCon -= ct.SoLuongMuon;
                }

                phieuMuon.TrangThai = "Đang mượn";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    $"Đã xác nhận phiếu mượn số {id}.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["Loi"] = "Có lỗi khi xác nhận phiếu mượn.";
                return RedirectToAction(nameof(Index));
            }
        }

        // ==========================================
        // TỪ CHỐI PHIẾU MƯỢN
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(int id)
        {
            var phieuMuon = await _context.PhieuMuons
                .FirstOrDefaultAsync(p =>
                    p.MaPhieuMuon == id);

            if (phieuMuon == null)
            {
                TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                return RedirectToAction(nameof(Index));
            }

            if (phieuMuon.TrangThai != "Chờ xác nhận")
            {
                TempData["Loi"] =
                    "Chỉ được từ chối phiếu đang chờ xác nhận.";
                return RedirectToAction(nameof(Index));
            }

            phieuMuon.TrangThai = "Từ chối";

            await _context.SaveChangesAsync();

            TempData["ThongBao"] =
                $"Đã từ chối phiếu mượn số {id}.";

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // TRẢ SÁCH
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Tra(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var phieuMuon = await _context.PhieuMuons
                    .Include(p => p.ChiTietPhieuMuons)
                        .ThenInclude(ct => ct.Sach)
                    .FirstOrDefaultAsync(p =>
                        p.MaPhieuMuon == id);

                if (phieuMuon == null)
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                    return RedirectToAction(nameof(Index));
                }

                if (phieuMuon.TrangThai != "Đang mượn")
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] =
                        "Phiếu này không ở trạng thái có thể trả sách.";
                    return RedirectToAction(nameof(Index));
                }

                if (!phieuMuon.ChiTietPhieuMuons.Any())
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] =
                        "Phiếu mượn chưa có chi tiết sách.";
                    return RedirectToAction(nameof(Index));
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    if (ct.Sach == null ||
                        ct.SoLuongMuon <= 0 ||
                        ct.Sach.SoLuongCon + ct.SoLuongMuon > ct.Sach.SoLuong)
                    {
                        await transaction.RollbackAsync();
                        TempData["Loi"] =
                            "Thông tin số lượng sách không hợp lệ.";
                        return RedirectToAction(nameof(Index));
                    }
                }

                foreach (var ct in phieuMuon.ChiTietPhieuMuons)
                {
                    ct.Sach!.SoLuongCon += ct.SoLuongMuon;
                    ct.NgayTra = DateTime.Today;
                    ct.TinhTrangTra = "Đã nhận trả";
                }

                phieuMuon.NgayTra = DateTime.Today;
                phieuMuon.TrangThai = "Đã trả";

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    $"Đã nhận trả sách cho phiếu số {id}.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Có lỗi khi trả sách. Vui lòng thử lại.";

                return RedirectToAction(nameof(Index));
            }
        }

        // ==========================================
        // XÓA PHIẾU MƯỢN
        // Chỉ xóa phiếu chưa mượn hoặc đã kết thúc.
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                var phieuMuon = await _context.PhieuMuons
                    .Include(p => p.ChiTietPhieuMuons)
                    .FirstOrDefaultAsync(p =>
                        p.MaPhieuMuon == id);

                if (phieuMuon == null)
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] = "Không tìm thấy phiếu mượn.";
                    return RedirectToAction(nameof(Index));
                }

                bool duocPhepXoa =
                    phieuMuon.TrangThai == "Chờ xác nhận" ||
                    phieuMuon.TrangThai == "Từ chối" ||
                    phieuMuon.TrangThai == "Đã hủy" ||
                    phieuMuon.TrangThai == "Đã trả";

                if (!duocPhepXoa)
                {
                    await transaction.RollbackAsync();
                    TempData["Loi"] =
                        "Không thể xóa phiếu đang mượn hoặc quá hạn. Vui lòng xử lý trả sách trước.";

                    return RedirectToAction(nameof(Index));
                }

                if (phieuMuon.ChiTietPhieuMuons.Any())
                {
                    _context.ChiTietPhieuMuons.RemoveRange(
                        phieuMuon.ChiTietPhieuMuons);
                }

                _context.PhieuMuons.Remove(phieuMuon);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["ThongBao"] =
                    $"Đã xóa phiếu mượn số {id} thành công.";

                return RedirectToAction(nameof(Index));
            }
            catch
            {
                await transaction.RollbackAsync();

                TempData["Loi"] =
                    "Không thể xóa phiếu mượn. Vui lòng kiểm tra lại dữ liệu liên quan.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}
