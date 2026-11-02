
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class PhieuMuon
    {
        [Key]
        [Display(Name = "Mã phiếu mượn")]
        public int MaPhieuMuon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn độc giả")]
        [Display(Name = "Mã độc giả")]
        public int MaDocGia { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày mượn")]
        public DateTime NgayMuon { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Hạn trả không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả dự kiến")]
        public DateTime HanTra { get; set; } = DateTime.Today.AddDays(14);

        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả thực tế")]
        public DateTime? NgayTra { get; set; }

        [Display(Name = "Trạng thái")]
        [StringLength(30)]
        public string TrangThai { get; set; } = "Chờ xác nhận";

        [ForeignKey(nameof(MaDocGia))]
        [ValidateNever]
        public virtual DocGia? DocGia { get; set; }

        [ValidateNever]
        public virtual ICollection<ChiTietPhieuMuon> ChiTietPhieuMuons { get; set; }
            = new List<ChiTietPhieuMuon>();
    }
}
