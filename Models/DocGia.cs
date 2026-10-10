
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class DocGia
    {
        [Key]
        public int MaDocGia { get; set; }

        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime? NgaySinh { get; set; }

        public string? GioiTinh { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        public string? DiaChi { get; set; }

        public DateTime NgayDangKy { get; set; }

        public string TrangThai { get; set; } = string.Empty;

        [ForeignKey(nameof(MaTaiKhoan))]
        public virtual TaiKhoan? TaiKhoan { get; set; }
    }
}
