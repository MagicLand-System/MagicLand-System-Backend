﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicLand_System.Domain.Models
{
    public class Address
    {
        public Guid Id { get; set; } 
        public string Street { get; set; }
        public string City { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }  
        public ICollection<Class> Classes { get; set; } = new List<Class>();    
    }
}