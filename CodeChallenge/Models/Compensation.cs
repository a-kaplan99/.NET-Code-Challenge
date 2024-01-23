using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CodeChallenge.Models
{
    public class Compensation
    {
        public Employee Employee { get; set; }
        public int Salary { get; set; }
        public DateOnly EffectiveDate { get; set; }
    }
}
