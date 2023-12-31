﻿using MagicLand_System.PayLoad.Response.Class;
using MagicLand_System.PayLoad.Response.Student;

namespace MagicLand_System.PayLoad.Response.Cart
{
    public class CartResponse
    {
        public Guid Id { get; set; }

        public List<CartItemResponse> CartItems { get; set; } = new List<CartItemResponse>();
    }
}
