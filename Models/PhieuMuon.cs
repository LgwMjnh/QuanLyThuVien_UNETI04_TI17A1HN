
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class PhieuMuon
    {
        [Key]
        public int MaPhieuMuon { get; set; }

        [Required]
        public int MaDocGia { get; set; }

        [Required]
        public DateTime NgayMuon { get; set; } = DateTime.Today;

        [Required]
        public DateTime HanTra { get; set; } = DateTime.Today.AddDays(14);

        public DateTime? NgayTra { get; set; }

        [StringLength(30)]
        public string TrangThai { get; set; } = "Đang mượn";

        public DocGia? DocGia { get; set; }

        public ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons
        {
            get;
            set;
        } = new List<ChiTietPhieuMuon>();
    }
}
