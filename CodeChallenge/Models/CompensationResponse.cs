using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Models
{
    public class CompensationResponse
    {
        public Employee Employee { get; set; }
        public int Salary { get; set; }
        public String EffectiveDate { get; set; }
    }
}
