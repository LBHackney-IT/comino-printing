using System;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace UseCases
{
    public class EndIdParameter
    {
        public string endId { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmssfff");
    }
}