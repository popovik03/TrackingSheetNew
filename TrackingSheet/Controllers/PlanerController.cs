using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrackingSheet.Models.RO_Planer;
using TrackingSheet.Models;
using TrackingSheet.Data;

namespace TrackingSheet.Controllers
{
    public class PlanerController : Controller
    {
        private readonly MVCDbContext _context;

        public PlanerController(MVCDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            // Получаем данные из базы данных
            var employeePlans = _context.EmployeePlaner2024.ToList();
            var employees = _context.ROemployees.ToList();

            // Создаем ViewModel для отображения
            var employeePlanViewModels = employees.Select(employee => new EmployeePlanViewModel
            {
                EmployeeId = employee.Id,
                Name = employee.Name,
                Stol = employee.Stol,
                MonthlyPlans = GetMonthlyPlans(employee.Id, employeePlans)
            }).ToList();

            // Получаем заголовки столбцов и данные для таблицы
            var headerColumns = GetHeaderColumns();
            var tableData = GetTableData(employeePlanViewModels);

            // Передаем данные в представление
            ViewBag.HeaderColumns = JsonConvert.SerializeObject(headerColumns);
            ViewBag.TableData = JsonConvert.SerializeObject(tableData);

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveChanges([FromBody] List<List<string>> updatedData)
        {
            try
            {
                var employees = _context.ROemployees.ToList();
                var employeePlans = _context.EmployeePlaner2024.ToList();

                foreach (var row in updatedData)
                {
                    if (row.Count < 2) continue; // Проверка на корректность строки данных

                    var employeeName = row[0];
                    var stol = int.Parse(row[1]);

                    var employee = employees.FirstOrDefault(e => e.Name == employeeName);
                    if (employee == null) continue;

                    var monthNames = new[] { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
                    var dayIndex = 2;

                    var employeePlan = employeePlans.FirstOrDefault(ep => ep.EmployeeId == employee.Id && ep.Year == DateTime.Now.Year);
                    if (employeePlan == null)
                    {
                        employeePlan = new EmployeePlaner2024
                        {
                            EmployeeId = employee.Id,
                            Year = DateTime.Now.Year
                        };
                        _context.EmployeePlaner2024.Add(employeePlan);
                    }

                    foreach (var month in monthNames)
                    {
                        int daysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, Array.IndexOf(monthNames, month) + 1);
                        var days = new List<string>();
                        for (int day = 0; day < daysInMonth; day++)
                        {
                            if (dayIndex >= row.Count) break;
                            days.Add(row[dayIndex++]);
                        }

                        switch (month)
                        {
                            case "January": employeePlan.January = string.Join(",", days); break;
                            case "February": employeePlan.February = string.Join(",", days); break;
                            case "March": employeePlan.March = string.Join(",", days); break;
                            case "April": employeePlan.April = string.Join(",", days); break;
                            case "May": employeePlan.May = string.Join(",", days); break;
                            case "June": employeePlan.June = string.Join(",", days); break;
                            case "July": employeePlan.July = string.Join(",", days); break;
                            case "August": employeePlan.August = string.Join(",", days); break;
                            case "September": employeePlan.September = string.Join(",", days); break;
                            case "October": employeePlan.October = string.Join(",", days); break;
                            case "November": employeePlan.November = string.Join(",", days); break;
                            case "December": employeePlan.December = string.Join(",", days); break;
                        }
                    }
                }

                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                // Логирование ошибки для отладки
                System.Diagnostics.Debug.WriteLine(ex.Message);
                return Json(new { success = false, message = ex.Message });
            }
        }


        private Dictionary<string, List<string>> GetMonthlyPlans(int employeeId, List<EmployeePlaner2024> employeePlans)
        {
            var monthlyPlans = new Dictionary<string, List<string>>();

            var plans = employeePlans.FirstOrDefault(ep => ep.EmployeeId == employeeId);
            if (plans != null)
            {
                monthlyPlans["Январь"] = plans.January.Split(',').ToList();
                monthlyPlans["Февраль"] = plans.February.Split(',').ToList();
                monthlyPlans["Март"] = plans.March.Split(',').ToList();
                monthlyPlans["Апрель"] = plans.April.Split(',').ToList();
                monthlyPlans["Май"] = plans.May.Split(',').ToList();
                monthlyPlans["Июнь"] = plans.June.Split(',').ToList();
                monthlyPlans["Июль"] = plans.July.Split(',').ToList();
                monthlyPlans["Август"] = plans.August.Split(',').ToList();
                monthlyPlans["Сентябрь"] = plans.September.Split(',').ToList();
                monthlyPlans["Октябрь"] = plans.October.Split(',').ToList();
                monthlyPlans["Ноябрь"] = plans.November.Split(',').ToList();
                monthlyPlans["Декабрь"] = plans.December.Split(',').ToList();
            }

            return monthlyPlans;
        }

        private List<string> GetHeaderColumns()
        {
            var months = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            var daysInMonth = new Dictionary<string, int>
            {
                { "Январь", 31 },
                { "Февраль", 29 },
                { "Март", 31 },
                { "Апрель", 30 },
                { "Май", 31 },
                { "Июнь", 30 },
                { "Июль", 31 },
                { "Август", 31 },
                { "Сентябрь", 30 },
                { "Октябрь", 31 },
                { "Ноябрь", 30 },
                { "Декабрь", 31 }
            };

            var headerColumns = new List<string> { "Сотрудник", "Стол" };

            foreach (var month in months)
            {
                headerColumns.Add(month); // Название месяца
                for (int day = 1; day <= daysInMonth[month]; day++)
                {
                    headerColumns.Add(day.ToString()); // Дни месяца
                }
            }

            return headerColumns;
        }

        private List<List<string>> GetTableData(List<EmployeePlanViewModel> employeePlanViewModels)
        {
            var months = new[] { "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь", "Декабрь" };
            var daysInMonth = new Dictionary<string, int>
            {
                { "Январь", 31 },
                { "Февраль", 29 },
                { "Март", 31 },
                { "Апрель", 30 },
                { "Май", 31 },
                { "Июнь", 30 },
                { "Июль", 31 },
                { "Август", 31 },
                { "Сентябрь", 30 },
                { "Октябрь", 31 },
                { "Ноябрь", 30 },
                { "Декабрь", 31 }
            };

            var tableData = new List<List<string>>();

            foreach (var employee in employeePlanViewModels)
            {
                var row = new List<string> { employee.Name, employee.Stol.ToString() };

                foreach (var month in months)
                {
                    row.Add(month); // Название месяца
                    if (employee.MonthlyPlans.ContainsKey(month))
                    {
                        row.AddRange(employee.MonthlyPlans[month]);
                    }
                    else
                    {
                        var days = new string[daysInMonth[month]];
                        row.AddRange(days);
                    }
                }

                tableData.Add(row);
            }

            return tableData;
        }
    }
}
