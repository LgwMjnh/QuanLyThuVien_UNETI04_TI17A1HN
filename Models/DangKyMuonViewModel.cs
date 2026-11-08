using System.ComponentModel.DataAnnotations;

namespace QuanLyThuVien_UNETI04_TI17A1HN.Models
{
    public class DangKyMuonViewModel
    {
        [Display(Name = "Sách cần mượn")]
        public int? MaSach { get; set; }

        [Range(1, 100, ErrorMessage = "Số lượng mượn phải từ 1 đến 100")]
        [Display(Name = "Số lượng mượn")]
        public int SoLuongMuon { get; set; } = 1;

        [Required(ErrorMessage = "Hạn trả không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn trả dự kiến")]
        public DateTime HanTra { get; set; } = DateTime.Today.AddDays(14);
    }
}
