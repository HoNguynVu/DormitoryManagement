using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;

namespace DataAccess.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly DormitoryDbContext _context;
        public StudentRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public void AddStudent(Student student)
        {
            _context.Students.Add(student);
        }
        public void UpdateStudent(Student student)
        {
            _context.Students.Update(student);
        }
        public void DeleteStudent(Student student)
        {
            _context.Students.Remove(student);
        }
    }
}
