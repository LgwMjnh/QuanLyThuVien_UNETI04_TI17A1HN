using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    [Table("ChiTietPhieuMuons")]
    public class ChiTietPhieuMuon
    {
        [Key]
        [Column("MaPhieuMuon", Order = 0)]
        public int MaPhieuMuon { get; set; }

        [Key]
        [Column("MaSach", Order = 1)]
        public int MaSach { get; set; }

        [Required]
        [Column("SoLuongMuon")]
        [Range(1, 1000)]
        public int SoLuong { get; set; } = 1;

        public DateTime? NgayTra { get; set; }

        [StringLength(50)]
        public string? TinhTrangTra { get; set; }

        public PhieuMuon? PhieuMuon { get; set; }

        public Sach? Sach { get; set; }
    }
}