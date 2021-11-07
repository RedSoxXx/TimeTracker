using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeTracker.Data.Extensions;

namespace TimeTracker.Data.Entities.Base
{
    public class BaseEntity : IEntity
    {
        [Key]
        public long Id { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; }

        public bool IsNew()
        {
            return EntityExtensions.IsNew(this);
        }
    }
}
