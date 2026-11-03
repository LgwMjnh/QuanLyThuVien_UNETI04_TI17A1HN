
using System.Collections.Generic;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class TraCuuDocGiaViewModel
    {
        public int? MaDocGia { get; set; }

        public bool DaTimKiem { get; set; }

        public string? ThongBao { get; set; }

        public DocGia? DocGia { get; set; }

        public List<PhieuMuon> LichSu { get; set; } = new();

        // Thống kê phiếu
        public int TongSoPhieu { get; set; }
        public int SoLanMuon { get; set; }
        public int SoPhieuChoXacNhan { get; set; }
        public int SoPhieuDangMuon { get; set; }
        public int SoPhieuQuaHan { get; set; }
        public int SoPhieuDaTra { get; set; }
        public int SoPhieuTuChoi { get; set; }
        public int SoPhieuDaHuy { get; set; }

        // Thống kê số lượng sách
        public int TongSoSachDaMuon { get; set; }
        public int SoSachDaTra { get; set; }
        public int SoSachDangMuon { get; set; }
        public int SoSachQuaHan { get; set; }
    }
}
