using System;
using System.ComponentModel.DataAnnotations;

namespace shift_pick_management.Models
{
    public class ExcelRegisterModel
    {
        [Display(Name = "FileName")]
        public string FileName { get; set; } = string.Empty;

        [Display(Name = "UploadedAt")]
        public DateTime? UploadedAt { get; set; }

        public string UserName { get; set; } = string.Empty;

        public bool Registered { get; set; }

        public DateTime? RegisteredAt { get; set; }

        public string Message { get; set; } = string.Empty;
    }
}
