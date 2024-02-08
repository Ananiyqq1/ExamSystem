﻿//using ExamSystem.Models.ViewModels;
using Azure.Core;
using ExamSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
//using System.Web.Helpers;
//using ExamSystem.Models;
//using ExamSystem.Models.ViewModels;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;

namespace ExamSystem.Controllers
{
    public class ExaminerController : Controller
    {
        private readonly ExamContext examContext;
        public ExaminerController(ExamContext exc)
        {
            examContext = exc;
        }


        public IActionResult Index()
        {
            ViewBag.Subjects = examContext.Subjects.ToList();
            List<Exam> examList = examContext.Exams.ToList();
            return View("Exam",examList);
        }

        [HttpPost]
        public async Task<IActionResult> SaveExam()
        {
            string requestBody;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            JObject json = JObject.Parse(requestBody);

            Subject sbj = examContext.Subjects.Where(s => s.SubjectName == (string)json["Subject"]).First();
            Exam ex = new Exam()
            {
                ExamName = (string)json["Exam"],
                ExamDifficulty = "Proffessional",
                DateCreated = DateTime.Now,
                PassingMark = (int)json["Pass"],
                TimeAllocated = (string)json["Time"],
                QuestionWeight = (int)json["Value"],
                Subject = sbj,
            };

          await examContext.Exams.AddAsync(ex);
          

            JArray questionAnswerArray = (JArray)json["questions"];

            foreach (JObject questionAnswerObj in questionAnswerArray)
            {
                string question = (string)questionAnswerObj["quest"];
                Question qst = new Question() { 
                Query=question,
                Exam=ex
                };
                await examContext.Questions.AddAsync(qst);
              
                JArray answerArray = (JArray)questionAnswerObj["answer"];

                foreach (JArray answer in answerArray)
                {
                    bool answerValue = (bool)answer[0];
                    string answerText = (string)answer[1];

                    Answer asr = new Answer() { 
                    AnswerText=answerText,
                    isCorrect=answerValue,
                    Question=qst
                    };
                  await examContext.Answers.AddAsync(asr);
                
                    // Your code...
                }

                // Your code...
            }
            try {
                await examContext.SaveChangesAsync();
            }
            catch (Exception esx) {
                Console.WriteLine(esx.Message);
            }



            return Ok(); // Return an appropriate response
           




            //// return Ok();
        }
        public async Task<IActionResult>  SaveEdit() {
            string requestBody;
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                requestBody = await reader.ReadToEndAsync();
            }

            JObject json = JObject.Parse(requestBody);

            Subject sbj = examContext.Subjects.Where(s => s.SubjectName == (string)json["Subject"]).First();
            Exam ex = examContext.Exams.Where(e => e.ExamId == (Guid)json["Id"]).First();
            ex.Subject = sbj;
            ex.ExamName = (string)json["Exam"];
            // ex.ExamDifficulty = "Proffessional",
            ex.DateCreated = DateTime.Now;
            ex.PassingMark = (int)json["Pass"];
            ex.TimeAllocated = (string)json["Time"];
            ex.QuestionWeight = (int)json["Value"];
            await examContext.SaveChangesAsync();

            JArray questionAnswerArray = (JArray)json["questions"];
            foreach (JObject questionAnswerObj in questionAnswerArray)
            {
                string question = (string)questionAnswerObj["quest"];
                Question qst = examContext.Questions.Where(q => q.QuesionId == (Guid)questionAnswerObj["QId"]).First();
                qst.Query = question;
                await examContext.SaveChangesAsync();
                JArray answerArray = (JArray)questionAnswerObj["answer"];

                foreach (JArray answer in answerArray)
                {
                    bool answerValue = (bool)answer[1];
                    string answerText = (string)answer[2];

                    Answer asr = examContext.Answers.Where(e => e.AnswerId == (Guid)answer[0]).First();
                    asr.AnswerText = answerText;
                    asr.isCorrect=answerValue;                  

                    await examContext.SaveChangesAsync();

                    // Your code...
                }

                // Your code...
            }




            return Ok();
        }





        public IActionResult EditExam(Guid id)
        {
            //    try
            ////    {
            ////    {
            Exam ex = examContext.Exams.Include(e => e.Subject).Include(e => e.Questions).ThenInclude(q => q.Answers).Where(e => e.ExamId == id).First();
            ////        //var jsobj = new {Exam=ex.ExamName,Time=ex.TimeAllocated,Value=ex.QuestionWeight,
            ////        //Subjec=ex.Subject.SubjectName,
            ////        //    };
            ////        //jsobj.questions = []
            ////        return Json(ex);
            ////        return Json(ex);

            var options = new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve,
                MaxDepth = 64 // Set the maximum depth as needed
            };

            string json = JsonSerializer.Serialize(ex, options);
            return Content(json, "application/json");


            //    }
            //    catch (Exception ex) {
            //        Console.WriteLine(ex.Message);
            //        return Ok();
            //    }




        }
    }
}
