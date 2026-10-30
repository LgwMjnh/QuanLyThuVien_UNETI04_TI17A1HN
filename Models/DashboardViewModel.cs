
using System.Collections.Generic;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class DashboardViewModel
    {
        // Thống kê tổng quan của Module 3
        public int TongSoSach { get; set; }

        public int TongSoDocGia { get; set; }

        public int SoSachDangMuon { get; set; }

        public int SoSachDaTra { get; set; }

        public int SoSachQuaHan { get; set; }

        public int TongSoPhieuMuon { get; set; }

        // Thống kê trạng thái phiếu mượn của Module 4
        public int TongPhieuMuon { get; set; }

        public int DangMuon { get; set; }

        public int DaTra { get; set; }

        public int QuaHan { get; set; }

        // Danh sách phiếu mượn quá hạn
        public List<PhieuMuon> PhieuMuonQuaHan { get; set; }
            = new List<PhieuMuon>();
    }
}
