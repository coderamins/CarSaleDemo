using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public class OrderRequest
    {
        public Guid UserId { get; set; }
        public Guid SaleId { get; set; }
    }
}