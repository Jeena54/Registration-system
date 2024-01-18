using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab6.Models.DataAccess;
using Lab8.Models.ViewModel;

namespace Lab6.Controllers
{
    public class AcademicRecordsController : Controller
    {
        private readonly StudentRecordContext _context;

        public AcademicRecordsController(StudentRecordContext context)
        {
            _context = context;
        }



        // GET: AcademicRecords
        public async Task<IActionResult> Index(string sort)
        {

            var studentRecords = _context.AcademicRecords.Include(a => a.CourseCodeNavigation).Include(a => a.Student);

            if (sort != null)
            {
                HttpContext.Session.SetString("sort", sort);
            }
            else if (HttpContext.Session.GetString("sort") != null)
            {
                sort = HttpContext.Session.GetString("sort");
            }

            if (sort == "course")
            {
                IOrderedQueryable<AcademicRecord> list;
                list = _context.AcademicRecords.Include(a => a.CourseCodeNavigation).Include(a => a.Student).OrderBy(a => a.CourseCodeNavigation.Title);
                return View(await list.ToListAsync());
            }
            else if (sort == "student")
            {
                IOrderedQueryable<AcademicRecord> list;
                list = _context.AcademicRecords.Include(a => a.CourseCodeNavigation).Include(a => a.Student).OrderBy(a => a.StudentId);
                return View(await list.ToListAsync());
            }

            return View(await studentRecords.ToListAsync());

        }


        public async Task<IActionResult> EditAll()
        {
            var studentRecordContext = _context.AcademicRecords.Include(a => a.CourseCodeNavigation).Include(a => a.Student);
            return View(await studentRecordContext.ToListAsync());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAll(List<AcademicRecord> myrecords)
        //public async Task<IActionResult> EditAll(AcademicRecord[] myrecords)
        {
            if (ModelState.IsValid)
            {
                foreach (var re in myrecords)
                {
                    _context.Update(re);
                    await _context.SaveChangesAsync();
                }
                //return RedirectToAction(nameof(Index));
                return Redirect("/AcademicRecords");
            }
            else
            {
                var studentRecordContext = _context.AcademicRecords.Include(a => a.CourseCodeNavigation).Include(a => a.Student);


                foreach (var re in myrecords)
                {
                    _context.Update(re);

                }
                return View(await studentRecordContext.ToListAsync());
            }
        }

        private bool AcademicRecordExists(object studentId)
        {
            throw new NotImplementedException();
        }



        // GET: AcademicRecords/Create
        public IActionResult Create()
        {
            ViewData["CourseCode"] = new SelectList(_context.Courses, "Code", "Code");
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id");
            return View();
        }

        // POST: AcademicRecords/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseCode,StudentId,Grade")] AcademicRecord academicRecord)
        {
            if (ModelState.IsValid)
            {
                _context.Add(academicRecord);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "Code", "Code", academicRecord.CourseCode);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", academicRecord.StudentId);
            return View(academicRecord);
        }

        // GET: AcademicRecords/Edit/5
        public async Task<IActionResult> Edit(string studentId, string courseCode)
        {
            if (studentId == null || courseCode == null)
            {
                return NotFound();
            }

            var academicRecord = await _context.AcademicRecords.Include(a => a.CourseCodeNavigation).Include(a => a.Student).FirstOrDefaultAsync(m => m.StudentId == studentId && m.CourseCode == courseCode);

            if (academicRecord == null)
            {
                return NotFound();
            }

            //ViewData["CourseCode"] = new SelectList(_context.Courses, "Code", "Code", academicRecord.CourseCode);
            //ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", academicRecord.StudentId);
            return View(academicRecord);
        }

        // POST: AcademicRecords/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AcademicRecord academicRecord)
        {
            AcademicRecord updatedGrade = _context.AcademicRecords.FirstOrDefault(c => c.StudentId == academicRecord.StudentId && c.CourseCode == academicRecord.CourseCode);

            updatedGrade.Grade = academicRecord.Grade;

            _context.Update(updatedGrade);

            if (ModelState.IsValid)
            {
                try
                {
                    //_context.Update(academicRecord);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AcademicRecordExists(academicRecord.StudentId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CourseCode"] = new SelectList(_context.Courses, "Code", "Code", academicRecord.CourseCode);
            ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id", academicRecord.StudentId);
            return View(academicRecord);
        }



        private bool AcademicRecordExists(string id)
        {
            return _context.AcademicRecords.Any(e => e.StudentId == id);
        }
    }
}
