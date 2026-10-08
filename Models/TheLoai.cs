using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class TheLoai
    {
        [Key]
        public int MaTheLoai { get; set; }

        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thể loại không được vượt quá 100 ký tự")]
        [Display(Name = "Tên thể loại")]
        public string TenTheLoai { get; set; } = string.Empty;

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Hoạt động";

        // Quan hệ 1 - N với Sach
        public virtual ICollection<Sach> Saches { get; set; }
            = new List<Sach>();
    }
}
