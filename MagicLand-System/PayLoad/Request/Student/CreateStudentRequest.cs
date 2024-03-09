﻿using System.ComponentModel.DataAnnotations;

namespace MagicLand_System.PayLoad.Request.Student
{
    public class CreateStudentRequest
    {
        [Required(ErrorMessage = "Tên Bé Không Được Để Trống")]
        [MaxLength(50, ErrorMessage = "Tên Bé Không Nên Vượt Quá 50 Ký Tự")]
        [MinLength(5, ErrorMessage = "Tên Bé Nên Có Ít Nhất 5 Ký Tự")]
        public required string FullName { get; set; }
        public required DateTime DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? AvatarImage { get; set; }
        [EmailAddress(ErrorMessage = "Định Dạng Email Không Hợp Lệ")]
        public string? Email { get; set; }
    }
}
