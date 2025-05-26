using Microsoft.AspNetCore.Mvc;
using project.Models;
using System.Collections.Generic;

namespace project.Controllers
{
    
    public class SubjectController : Controller
    {
        SubjectModel subjectModel = new SubjectModel();

        public IActionResult Subject()
        {
            List<Subject> subjects = subjectModel.GetAllSubjects();
            return View(subjects);
        }

        public IActionResult SubjectCreate()
        {
            return View();
        }

        [HttpPost]
        public IActionResult SubjectCreate(Subject subject)
        {
            if (ModelState.IsValid)
            {
                subjectModel.InsertSubject(subject);
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        public IActionResult SubjectEdit(int id)
        {
            Subject subject = subjectModel.GetSubjectById(id);
            return View(subject);
        }

        [HttpPost]
        public IActionResult SubjectEdit(Subject subject)
        {
            if (ModelState.IsValid)
            {
                subjectModel.UpdateSubject(subject);
                return RedirectToAction("Index");
            }
            return View(subject);
        }

        public IActionResult SubjectDelete(int id)
        {
            Subject subject = subjectModel.GetSubjectById(id);
            return View(subject);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            subjectModel.DeleteSubject(id);
            return RedirectToAction("Index");
        }
    }
}
