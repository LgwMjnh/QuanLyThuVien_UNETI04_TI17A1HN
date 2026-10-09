
using System.Collections.Generic;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class DashboardViewModel
    {
        public int TongPhieuMuon { get; set; }
        public int DangMuon { get; set; }
        public int DaTra { get; set; }
        public int QuaHan { get; set; }

        public List<PhieuMuon> PhieuMuonQuaHan { get; set; }
            = new List<PhieuMuon>();
    }
}
