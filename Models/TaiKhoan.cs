
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }


        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3,
          ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string TenDangNhap { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(20, MinimumLength = 6,
            ErrorMessage = "Mật khẩu phải từ 6 đến 20 ký tự.")]
        public string MatKhau { get; set; } = string.Empty;


        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100,
            ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string HoTen { get; set; } = string.Empty;


        [Required(ErrorMessage = "Vai trò không được để trống.")]
        [StringLength(30,
            ErrorMessage = "Vai trò không được vượt quá 30 ký tự.")]
        public string VaiTro { get; set; } = string.Empty;


        public bool TrangThai { get; set; } = true;


        public virtual DocGia? DocGia { get; set; }
    }
}
