using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class CarSale
    {
        public Guid Id { get; set; }

        public string Title { get; set; }

        public int Capacity { get; set; }

        public int SoldCount { get; set; }
    }
}