using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace webapi_80.src.Tenant.Model
{
    public class Tenant
    {
        [Key]
        public Guid Id { get; set; }
        public string? CompanyName { get; set; }
        [Required]
        public string Subdomain { get; set; }
        
        public DateTime DateCreated { get; set; }
        
        public string? UpdatedBy { get; set; }
        
        public DateTime LastUpdate { get; set; }
        
        public bool isSchemaCreated { get; set; }
        
        public DateTime LastMigration { get; set; }
        
        public string? AdminId { get; set; }
        
        public string? LogoUrl { get; set; }
        
        public string? Status { get; set; }
    }
}
