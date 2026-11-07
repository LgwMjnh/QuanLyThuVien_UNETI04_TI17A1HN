using System.Collections.Generic;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class ThongKeViewModel
    {
        // Các chỉ số tổng quan của Module 3
        public int TongLuotMuon { get; set; }

        public int TongSachDangMuon { get; set; }

        public int TongSachDaTra { get; set; }

        public int TongSachQuaHan { get; set; }

        public string SachMuonNhieuNhat { get; set; } = string.Empty;

        public int SoLuotMuonNhieuNhat { get; set; }

        // Các chỉ số tổng quan của Module 4
        public int TongPhieuMuon { get; set; }

        public int DangMuon { get; set; }

        public int DaTra { get; set; }

        public int QuaHan { get; set; }

        public int TongDocGia { get; set; }

        public int TongSach { get; set; }

        // Báo cáo 1: Số đầu sách theo thể loại
        public List<SachTheoTheLoaiThongKe> SachTheoTheLoai { get; set; }
            = new();

        // Báo cáo 2: Số lượng của từng đầu sách
        public List<SoLuongTungSachThongKe> SoLuongTungSach { get; set; }
            = new();

        // Báo cáo 3: Lượt mượn theo sách
        public List<LuotMuonTheoSachThongKe> LuotMuonTheoSach { get; set; }
            = new();

        // Báo cáo 4: Phiếu theo trạng thái
        public List<PhieuTheoTrangThaiThongKe> PhieuTheoTrangThai { get; set; }
            = new();

        // Báo cáo 5: Top sách được mượn nhiều nhất
        public List<LuotMuonTheoSachThongKe> TopSachMuonNhieuNhat { get; set; }
            = new();

        // Báo cáo 6: Lượt mượn theo độc giả
        public List<LuotMuonTheoDocGiaThongKe> LuotMuonTheoDocGia { get; set; }
            = new();

        // Báo cáo 7: Phiếu mượn theo tháng
        public List<PhieuMuonTheoThangThongKe> PhieuMuonTheoThang { get; set; }
            = new();
    }

    // Thống kê số đầu sách theo thể loại
    public class SachTheoTheLoaiThongKe
    {
        public string TenTheLoai { get; set; } = string.Empty;

        public int SoDauSach { get; set; }
    }

    // Thống kê số lượng từng đầu sách
    public class SoLuongTungSachThongKe
    {
        public string TenSach { get; set; } = string.Empty;

        public string TenTheLoai { get; set; } = string.Empty;

        public int SoLuong { get; set; }

        public int SoLuongCon { get; set; }
    }

    // Thống kê lượt mượn theo sách
    public class LuotMuonTheoSachThongKe
    {
        public string TenSach { get; set; } = string.Empty;

        public int SoLuotMuon { get; set; }

        public int TongSoLuong { get; set; }
    }

    // Thống kê phiếu theo trạng thái
    public class PhieuTheoTrangThaiThongKe
    {
        public string TrangThai { get; set; } = string.Empty;

        public int SoPhieu { get; set; }
    }

    // Thống kê lượt mượn theo độc giả
    public class LuotMuonTheoDocGiaThongKe
    {
        public string HoTen { get; set; } = string.Empty;

        public int SoLuotMuon { get; set; }
    }

    // Thống kê phiếu mượn theo tháng
    public class PhieuMuonTheoThangThongKe
    {
        public int Thang { get; set; }

        public int Nam { get; set; }

        public int SoPhieu { get; set; }
    }
}