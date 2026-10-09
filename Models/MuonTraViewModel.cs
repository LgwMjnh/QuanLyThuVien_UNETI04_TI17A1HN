namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class MuonTraViewModel
    {
        public int MaPhieuMuon { get; set; }

        public string TenDocGia { get; set; }

        public string TenSach { get; set; }

        public DateTime NgayMuon { get; set; }

        public DateTime? HanTra { get; set; }

        public DateTime? NgayTra { get; set; }

        public string TrangThai { get; set; }

        public int SoNgayQuaHan { get; set; }
    }
}