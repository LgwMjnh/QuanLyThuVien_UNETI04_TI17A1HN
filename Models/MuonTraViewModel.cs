
using System;
using System.Collections.Generic;
using QuanLyThuVien_UNETI04_TI17A1HN.Models;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class MuonTraViewModel
    {
        public List<PhieuMuon> DanhSachPhieuMuon { get; set; }
            = new List<PhieuMuon>();

        public int TongPhieu { get; set; }

        public int DangMuon { get; set; }

        public int QuaHan { get; set; }

        public int DaTra { get; set; }

        public string? TrangThaiLoc { get; set; }

        public string? TuKhoa { get; set; }

        public DateTime? NgayTu { get; set; }

        public DateTime? NgayDen { get; set; }

        public int TrangHienTai { get; set; } = 1;

        public int TongSoTrang { get; set; }
    }
}
