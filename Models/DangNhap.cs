
using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class DangNhap
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; } = string.Empty;
    }
}
