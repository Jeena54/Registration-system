using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Lab6.Models.DataAccess;
using Lab6.Models;
using Lab8.Models.ViewModel;

namespace Lab6.Controllers
{
    public class EmployeesController : Controller
    {
        private readonly StudentRecordContext _context;

        public EmployeesController(StudentRecordContext context)
        {
            _context = context;
        }

        public IActionResult Search()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Search(SearchViewModel nameSearchViewModel)
        {
            if (ModelState.IsValid)
            {
                string searchString = nameSearchViewModel.NameSearchString;
                List<Employee> searchResults = _context.Employees.Include(a => a.Roles).Where(s =>
                s.Name.Contains(searchString) || s.UserName.Contains(searchString)).ToList();

                List<Role> r = _context.Roles.ToList();
                List<EmployeeRoleSelections> listEmp = new List<EmployeeRoleSelections>();
                if (searchResults.Count == 0)
                {
                    ModelState.AddModelError("NameSearchString", "No employee name or network ID contains the specified characters");
                }
                else
                {
                    foreach(var sResults in searchResults)
                    {
                        ViewData["searchString"] = searchString;
                        EmployeeRoleSelections searchEmployee = new EmployeeRoleSelections(sResults, r);
                        listEmp.Add(searchEmployee);
                    }                  
                    return View("Index", listEmp);
                }
            }
            return View(nameSearchViewModel);
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {
            //return View(await _context.Employees.Include(a => a.Roles).ToListAsync());
            var employee = await _context.Employees.Include(a => a.Roles).ToListAsync();
            List<Role> r = _context.Roles.ToList();
            List<EmployeeRoleSelections> listEmp = new List<EmployeeRoleSelections>();
            foreach (var emp in employee)
            {
                EmployeeRoleSelections AllEmployee = new EmployeeRoleSelections(emp, r);
                listEmp.Add(AllEmployee);
            }


            return View(listEmp);

        }



        // GET: Employees/Create
        public IActionResult Create()
        {
            Employee e = new Employee();
            List<Role> r = _context.Roles.ToList();
            EmployeeRoleSelections newEmployee = new EmployeeRoleSelections(e, r);
            return View(newEmployee);
        }

        // POST: Employees/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeRoleSelections employeeRoleSelections)
        {
            if (!employeeRoleSelections.roleSelections.Any(m => m.Selected))
            {
                ModelState.AddModelError("roleSelections", "You must select at leat one role!");
            }
            if (_context.Employees.Any(e => e.UserName == employeeRoleSelections.employee.UserName && e.Id != employeeRoleSelections.employee.Id))
            {
                ModelState.AddModelError("employee.UserName", "This user name has been used by another employee!");
            }

            if (ModelState.IsValid)
            {
                foreach (RoleSelection roleSelection in employeeRoleSelections.roleSelections)
                {
                    if (roleSelection.Selected)
                    {
                        if (roleSelection.Selected)
                        {
                            Role role = _context.Roles.SingleOrDefault(r => r.Id == roleSelection.role.Id);
                            employeeRoleSelections.employee.Roles.Add(role);
                        }
                    }
                }
                _context.Add(employeeRoleSelections.employee);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(employeeRoleSelections);
        }

        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            Employee employee = await _context.Employees.Include(e => e.Roles).SingleOrDefaultAsync(e => e.Id == id);
            List<Role> r = _context.Roles.ToList();
            EmployeeRoleSelections newEmployee = new EmployeeRoleSelections(employee, r);

            return View(newEmployee);
        }

        // POST: Employees/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeRoleSelections employeeRoleSelections)
        {

            if (!employeeRoleSelections.roleSelections.Any(m => m.Selected))
            {
                ModelState.AddModelError("roleSelections", "You must select at leat one role!");
            }
            if (_context.Employees.Any(e => e.UserName == employeeRoleSelections.employee.UserName && e.Id != employeeRoleSelections.employee.Id))
            {
                ModelState.AddModelError("employee.UserName", "This user name has been used by another employee!");
            }
          

            if (ModelState.IsValid)
            {
                Employee employee = await _context.Employees.Include(e => e.Roles).SingleOrDefaultAsync(e => e.Id == employeeRoleSelections.employee.Id);
                employee.Name = employeeRoleSelections.employee.Name;
                _context.Update(employee);

                employee.UserName = employeeRoleSelections.employee.UserName;
                _context.Update(employee);
                employee.Password = employeeRoleSelections.employee.Password;
                _context.Update(employee);
                employee.Roles.Clear();
                foreach (RoleSelection roleSelection in employeeRoleSelections.roleSelections)
                {
                    if (roleSelection.Selected)
                    {
                        Role role = _context.Roles.SingleOrDefault(r => r.Id == roleSelection.role.Id);
                        employee.Roles.Add(role);
                    }
                }
                _context.Update(employee);
                _context.SaveChangesAsync();
               

                return RedirectToAction(nameof(Index));
            }
            
            return View(employeeRoleSelections);
        }


        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }
}
