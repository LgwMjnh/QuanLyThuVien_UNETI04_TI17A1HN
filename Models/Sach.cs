using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    [Table("Sach")]
    public class Sach
    {
        [Key]
        public int MaSach { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [StringLength(200, ErrorMessage = "Tên sách không được vượt quá 200 ký tự")]
        [Display(Name = "Tên sách")]
        public string TenSach { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn thể loại")]
        [Display(Name = "Thể loại")]
        public int MaTheLoai { get; set; }

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [StringLength(150, ErrorMessage = "Tên tác giả không được vượt quá 150 ký tự")]
        [Display(Name = "Tác giả")]
        public string TacGia { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nhà xuất bản không được để trống")]
        [StringLength(200, ErrorMessage = "Nhà xuất bản không được vượt quá 200 ký tự")]
        [Display(Name = "Nhà xuất bản")]
        public string NhaXuatBan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Năm xuất bản không được để trống")]
        [Range(1000, 2100, ErrorMessage = "Năm xuất bản không hợp lệ")]
        [Display(Name = "Năm xuất bản")]
        public int NamXuatBan { get; set; }

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "Số lượng còn không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng còn phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Số lượng còn")]
        public int SoLuongCon { get; set; }

        [Required(ErrorMessage = "Giá sách không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá sách phải lớn hơn hoặc bằng 0")]
        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Giá sách")]
        public decimal GiaSach { get; set; }

        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [StringLength(50)]
        [Display(Name = "Trạng thái")]
        public string TrangThai { get; set; } = "Đang phục vụ";


        // Navigation property
        [ForeignKey("MaTheLoai")]
        public virtual TheLoai? TheLoai { get; set; }
    }
}
