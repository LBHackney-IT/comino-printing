using System.Collections.Generic;

namespace Usecases.Domain
{
    public class Response
    {
        public bool Success { get; set; }

        public List<string> Errors { get; set; }
    }
}