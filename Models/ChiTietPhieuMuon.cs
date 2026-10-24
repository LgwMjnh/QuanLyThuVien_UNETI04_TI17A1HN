
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    [Table("ChiTietPhieuMuons")]
    [PrimaryKey(nameof(MaPhieuMuon), nameof(MaSach))]
    public class ChiTietPhieuMuon
    {
        [Display(Name = "Mã phiếu mượn")]
        public int MaPhieuMuon { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn sách")]
        [Display(Name = "Mã sách")]
        public int MaSach { get; set; }

        [Required(ErrorMessage = "Số lượng mượn không được để trống")]
        [Range(1, 1000, ErrorMessage = "Số lượng mượn phải từ 1 đến 1000")]
        [Column("SoLuongMuon")]
        [Display(Name = "Số lượng mượn")]
        public int SoLuongMuon { get; set; } = 1;

        [DataType(DataType.Date)]
        [Display(Name = "Ngày trả")]
        public DateTime? NgayTra { get; set; }

        [StringLength(50)]
        [Display(Name = "Tình trạng trả")]
        public string? TinhTrangTra { get; set; }

        [ForeignKey(nameof(MaPhieuMuon))]
        [ValidateNever]
        public virtual PhieuMuon? PhieuMuon { get; set; }

        [ForeignKey(nameof(MaSach))]
        [ValidateNever]
        public virtual Sach? Sach { get; set; }
    }
}
