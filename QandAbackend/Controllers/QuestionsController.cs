using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using QandAbackend.Data;
using QandAbackend.Hubs;
using QandAbackend.Models;

namespace QandAbackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuestionsController : ControllerBase
    {
        private readonly IDataRepository _dataRepository;
        private readonly IHubContext<QuestionsHub> _questionHubContext;
        private readonly ILogger _logger;
        public QuestionsController(IDataRepository dataRepository, IHubContext<QuestionsHub> questionHubContext, ILogger<QuestionsController> logger)
        {
            _dataRepository = dataRepository;
            _questionHubContext = questionHubContext;
            _logger = logger;
        }

        public IEnumerable<QuestionGetManyResponse> GetQuestions(string search)
        {
            if (string.IsNullOrEmpty(search)) {
                return _dataRepository.GetQuestions();
            }
            else
            {
                _logger.LogInformation("hello from the logger");
                return _dataRepository.GetQuestionsBySearch(search);
            }

        }

        [HttpGet("unanswered")]
        public IEnumerable<QuestionGetManyResponse> GetUnansweredQuestions()
        {
            return _dataRepository.GetUnansweredQuestions();
        }

        [HttpGet("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> GetQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if (question == null)
            {
                return NotFound();
            }
            else
            {
                return Ok(question);
            }
        }

        [HttpPost]
        public ActionResult<QuestionGetSingleResponse> PostQuestion(QuestionPostRequest questionPostRequest)
        {
            var SavedQuestion = _dataRepository.PostQuestion(new QuestionPostFullRequest
            {
                Title = questionPostRequest.Title,
                Content = questionPostRequest.Content,
                UserId = "1",
                Username = "bob.test@test.com",
                Created = DateTime.UtcNow
            });

            return CreatedAtAction(nameof(GetQuestion), new { questionId = SavedQuestion.QuestionId }, SavedQuestion);
        }

        [HttpPut("{questionId}")]
        public ActionResult<QuestionGetSingleResponse> PutQuestion(int questionId, QuestionPutRequest questionPutRequest)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if(question == null)
            {
                return NotFound();
            }
            //using ternary expressions to update the request model with dataa from the existing question if it hasn't been supplied in the request
            questionPutRequest.Title = string.IsNullOrEmpty(questionPutRequest.Title) ? question.Title : questionPutRequest.Title;
            questionPutRequest.Content = string.IsNullOrEmpty(questionPutRequest.Content) ? question.Content : questionPutRequest.Content;

            var savedQuestion = _dataRepository.PutQuestion(questionId, questionPutRequest);
            return savedQuestion;
        }

        [HttpDelete("{questionId}")]
        public ActionResult DeleteQuestion(int questionId)
        {
            var question = _dataRepository.GetQuestion(questionId);
            if(question == null)
            {
                return NotFound();
            }

            _dataRepository.DeleteQuestion(questionId);
            return NoContent();
        }

        [HttpPost("answers")]
        public ActionResult<AnswerGetResponse> PostAnswer (AnswerPostRequest answerPostRequest) 
        {
            
            var questionExists = _dataRepository.QuestionExists(answerPostRequest.QuestionId.Value);
            if (!questionExists)
            {
                return NotFound();
            }
            var savedAnswer = _dataRepository.PostAnswer(new AnswerPostFullRequest
            {

               QuestionId = answerPostRequest.QuestionId.Value,
               Content = answerPostRequest.Content,
               UserId = "1",
               UserName = "bob.test@test.com",
               Created = DateTime.UtcNow
            }) ;

            ////this pushes questions with the new answer to clients that have subcribed to the question.
            //_questionHubContext.Clients.Group(
            //    $"Question-{answerPostRequest.QuestionId.Value}"
            //    ).SendAsync("RecieveQuestion", _dataRepository.GetQuestion(answerPostRequest.QuestionId.Value));
            ////------------------------------------------------------------------------------------------------

            return savedAnswer;
        }


        

    }
}
