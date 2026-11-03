using System;
using System.Collections.Generic;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class SachFilterViewModel
    {
        public List<Sach> DanhSachSach { get; set; } = new();

        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 6;
        public int TotalItems { get; set; } = 0;
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / (PageSize > 0 ? PageSize : 1));
        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public string? Search { get; set; }
        public string? TheLoai { get; set; }
        public string? NhaXuatBan { get; set; }
        public string? TrangThai { get; set; }
        public int? NamTu { get; set; }
        public int? NamDen { get; set; }
        public string SortOrder { get; set; } = "name_asc";

        public List<string> DanhSachTheLoai { get; set; } = new();
        public List<string> DanhSachNhaXuatBan { get; set; } = new();
    }
}
