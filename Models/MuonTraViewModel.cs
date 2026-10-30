
using System;
using System.Collections.Generic;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class MuonTraViewModel
    {
        // Thông tin của một phiếu mượn
        public int MaPhieuMuon { get; set; }

        public string TenDocGia { get; set; } = string.Empty;

        public string TenSach { get; set; } = string.Empty;

        public DateTime NgayMuon { get; set; }

        public DateTime? HanTra { get; set; }

        public DateTime? NgayTra { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public int SoNgayQuaHan { get; set; }

        // Danh sách phiếu mượn cho Module 4
        public List<PhieuMuon> DanhSachPhieuMuon { get; set; }
            = new List<PhieuMuon>();

        // Thống kê
        public int TongPhieu { get; set; }

        public int DangMuon { get; set; }

        public int QuaHan { get; set; }

        public int DaTra { get; set; }

        // Bộ lọc
        public string? TrangThaiLoc { get; set; }

        public string? TuKhoa { get; set; }

        public DateTime? NgayTu { get; set; }

        public DateTime? NgayDen { get; set; }

        // Phân trang
        public int TrangHienTai { get; set; } = 1;

        public int TongSoTrang { get; set; }
    }
}
