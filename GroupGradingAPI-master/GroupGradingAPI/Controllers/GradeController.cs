﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GroupGradingAPI.Data;
using GroupGradingAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace GroupGradingAPI.Controllers
{
    [Authorize(Roles = "Teacher")]
    [Route("api/[controller]")]
    [ApiController]
    public class GradeController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly GradingContext _context;

        public GradeController(GradingContext context, UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        //CREATE VALUES
        [EnableCors("AllAccessCors")]
        [HttpPost("create")]
        public ActionResult<string> createEvaluation([FromBody] Grade model)
        {
            try
            {
                Grade newGrade = new Grade();
                newGrade.GradeId = model.GradeId;
                newGrade.Percentage = model.Percentage;
                newGrade.StudentId = model.StudentId;

                _context.Grades.Add(newGrade);
                _context.SaveChanges();
                return JsonConvert.SerializeObject("Created New Grade");
            }
            catch (Exception e)
            {

            }
            return JsonConvert.SerializeObject("Error");
        }

        // DELETE VALUES
        [EnableCors("AllAccessCors")]
        [HttpPost("delete/{gradeId}")]
        public ActionResult<string> deleteEvaluation(string gradeId)
        {
            try
            {
                var grade = _context.Grades.Where(c => c.GradeId == gradeId).FirstOrDefault();

                _context.Grades.Remove(grade);
                _context.SaveChanges();
                return JsonConvert.SerializeObject("Deleted ");
            }
            catch (Exception e)
            {

            }
            return JsonConvert.SerializeObject("Error");
        }

        [EnableCors("AllAccessCors")]
        // GET VALUE BY ID
        [HttpGet("{gradeId}")]
        public ActionResult<string> getEvaluations(string gradeId)
        {
            try
            {
                var grade = _context.Grades.Where(c => c.GradeId == gradeId).FirstOrDefault();
                return JsonConvert.SerializeObject(grade);
            }
            catch (Exception e)
            {

            }
            return JsonConvert.SerializeObject("Error");
        }


        [EnableCors("AllAccessCors")]
        //EDIT VALUES
        [HttpPut("{id}")]
        public ActionResult<string> seStudentData([FromBody] Grade model, [FromRoute] string id)
        {
            try
            {
                var grade = _context.Grades
                    .Where(c => c.GradeId == id).FirstOrDefault();

                grade.GradeId = model.GradeId;
                grade.Percentage = model.Percentage;
                grade.StudentId = model.StudentId;

                _context.Grades.Update(grade);
                _context.SaveChanges();
                return JsonConvert.SerializeObject("Success");
            }
            catch (Exception e)
            {

            }
            return JsonConvert.SerializeObject("Error");
        }

        [EnableCors("AllAccessCors")]
        //GET ALL
        [HttpGet]
        public ActionResult<string> getGradeData()
        {
            try
            {
                try
                {
                    var evaluations = _context.Grades.ToList();
                    return JsonConvert.SerializeObject(evaluations);
                }
                catch (Exception e)
                {

                }
                return JsonConvert.SerializeObject("Error");
            }
            catch (Exception e)
            {

            }
            return JsonConvert.SerializeObject("Error");
        }
    }
}