namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class MuonTraViewModel
    {
        public int MaPhieuMuon { get; set; }

        public string TenDocGia { get; set; } = string.Empty;

        public string TenSach { get; set; } = string.Empty;

        public DateTime NgayMuon { get; set; }

        public DateTime? HanTra { get; set; }

        public DateTime? NgayTra { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        public int SoNgayQuaHan { get; set; }
    }
}