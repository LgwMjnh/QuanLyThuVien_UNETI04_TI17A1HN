using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        public string TenDangNhap { get; set; }

        public string MatKhau { get; set; }

        public string HoTen { get; set; }

        public string VaiTro { get; set; }

        public bool TrangThai { get; set; }
    }
}