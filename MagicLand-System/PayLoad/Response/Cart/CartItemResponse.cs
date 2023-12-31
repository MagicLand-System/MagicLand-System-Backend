﻿using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Student;

namespace MagicLand_System.PayLoad.Response.Cart
{
    public class CartItemResponse
    {
        public Guid Id { get; set; }

        public required List<StudentResponse> Students { get; set; } = new List<StudentResponse>();
        public required ClassResponseV1 Class { get; set; }
    }
}
