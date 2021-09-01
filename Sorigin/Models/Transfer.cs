using Microsoft.EntityFrameworkCore;
using System;

namespace Sorigin.Models
{
    [Index(nameof(ID))]
    [Index(nameof(TransferID))]
    public class Transfer
    {
        public Guid ID { get; set; }
        public Guid TransferID { get; set; }
    }
}