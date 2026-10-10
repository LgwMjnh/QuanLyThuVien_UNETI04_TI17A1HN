
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    [PrimaryKey(nameof(MaPhieuMuon), nameof(MaSach))]
    public class ChiTietPhieuMuon
    {
        public int MaPhieuMuon { get; set; }

        public int MaSach { get; set; }

        public int SoLuongMuon { get; set; }

        public DateTime? NgayTra { get; set; }

        public string? TinhTrangTra { get; set; }

        public virtual PhieuMuon? PhieuMuon { get; set; }

        public virtual Sach? Sach { get; set; }
    }
}
