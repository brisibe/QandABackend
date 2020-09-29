using System;

namespace QandAbackend.Data
{
    public class AnswerGetResponse
    {
        public string AnswerId { get; set; }
        public string Content { get; set; }

        public string UserName { get; set; }
        public DateTime Created { get; set; }
    }
}