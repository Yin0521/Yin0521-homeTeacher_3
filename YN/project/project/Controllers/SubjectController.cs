using Microsoft.AspNetCore.Mvc;
using project.Models;
using System.Collections.Generic;

namespace project.Controllers
{
    public class SubjectController : Controller
    {
        private readonly SubjectModel _subjectModel;

        // 用依賴注入方式取得 SubjectModel
        public SubjectController(SubjectModel subjectModel)
        {
            _subjectModel = subjectModel;
        }

        // 科目列表
        public IActionResult Subject()
        {
            List<Subject> subjects = _subjectModel.GetAllSubjects();
            return View(subjects);
        }

        // 新增科目頁面
        public IActionResult SubjectCreate()
        {
            return View();
        }

        // 新增科目（POST）
        [HttpPost]
        public IActionResult SubjectCreate(Subject subject)
        {
            if (ModelState.IsValid)
            {
                _subjectModel.InsertSubject(subject);
                return RedirectToAction("Subject");
            }
            return View(subject);
        }

        // 編輯科目頁面
        public IActionResult SubjectEdit(int id)
        {
            Subject subject = _subjectModel.GetSubjectById(id);
            return View(subject);
        }

        // 編輯科目（POST）
        [HttpPost]
        public IActionResult SubjectEdit(Subject subject)
        {
            if (ModelState.IsValid)
            {
                _subjectModel.UpdateSubject(subject);
                return RedirectToAction("Subject");
            }
            return View(subject);
        }

        // 刪除確認頁
        public IActionResult SubjectDelete(int id)
        {
            Subject subject = _subjectModel.GetSubjectById(id);
            return View(subject);
        }

        // 刪除執行（POST）
        [HttpPost, ActionName("SubjectDelete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _subjectModel.DeleteSubject(id);
            return RedirectToAction("Subject");
        }
    }
}
