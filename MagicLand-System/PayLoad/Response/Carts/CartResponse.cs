﻿namespace MagicLand_System.PayLoad.Response.Carts
{
    public class CartResponse
    {
        public Guid CartId { get; set; }
        public List<CartItemResponse> Items { get; set; } = new List<CartItemResponse>();
    }
}
