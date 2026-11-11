using System.ComponentModel.DataAnnotations;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.ViewModels
{
    public class TaiKhoanTaoViewModel
    {
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

        public bool TaoAdmin { get; set; }

        public string? MaBaoMat { get; set; }
    }

    public class TaiKhoanSuaViewModel
    {
        public int MaTaiKhoan { get; set; }

        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(50, MinimumLength = 3,
            ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
        public string TenDangNhap { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100,
            ErrorMessage = "Họ tên không được vượt quá 100 ký tự.")]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(20,
            ErrorMessage = "Mật khẩu không được vượt quá 20 ký tự.")]
        public string? MatKhauMoi { get; set; }
        public TrangThaiTaiKhoan TrangThai { get; set; }
                = TrangThaiTaiKhoan.BinhThuong;
    }

    public class TaiKhoanDanhSachViewModel
    {
        public List<TaiKhoan> TaiKhoans { get; set; } = new();

        public int TrangHienTai { get; set; }

        public int TongSoTrang { get; set; }

        public int SoTaiKhoanMoiTrang { get; set; }

        public int TongSoTaiKhoan { get; set; }

        public string? TuKhoa { get; set; }

        public int? TrangThai { get; set; }
    }
}