﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class FlashCard
    {
        public Guid Id { get; set; }
        public double Score { get; set; }
        public List<SideFlashCard> SideFlashCards { get; set; } = new List<SideFlashCard>();
        public Guid QuestionId { get; set; }
        public Question Question { get; set; }
    }
}