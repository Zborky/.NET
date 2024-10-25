using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EshopCrud.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? Description { get; set; }

        [Required]
        public int Quantity { get; set; }

        public string? ImagePath { get; set; }

        // Navigačná vlastnosť pre vzťah s objednávkami
         // Navigačná vlastnosť pre vzťah s objednávkami
        //public required ICollection<OrderItem> OrderItems { get; set; }
    }
}

