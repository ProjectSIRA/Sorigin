using Microsoft.EntityFrameworkCore;
using NodaTime;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Sorigin.Models
{
    [Index(nameof(Value))]
    public class RefreshToken
    {
        [Key]
        public string Value { get; set; } = null!;

        [ForeignKey(nameof(User))]
        public Guid UserID { get; set; }

        public Instant Created { get; set; }
        public Instant Expiration { get; set; }
    }
}