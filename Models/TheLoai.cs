using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class TheLoai
    {
        [Key]
        public int MaTheLoai { get; set; }

        public string TenTheLoai { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; }
    }
}