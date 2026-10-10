using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class PhieuMuon
    {
        [Key]
        public int MaPhieuMuon { get; set; }

        public int MaDocGia { get; set; }

        public DateTime NgayMuon { get; set; }

        public DateTime HanTra { get; set; }

        public string TrangThai { get; set; } = "Chờ xác nhận";

        [ForeignKey(nameof(MaDocGia))]
        public virtual DocGia? DocGia { get; set; }

        public virtual ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }
            = new List<ChiTietPhieuMuon>();
    }
}