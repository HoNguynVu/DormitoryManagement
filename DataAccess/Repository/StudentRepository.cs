using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;
using DataAccess.Interfaces;
using DataAccess.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Repository
{
    public class StudentRepository : IStudentRepository
    {
        private readonly DormitoryDbContext _context;
        public StudentRepository(DormitoryDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Student>> GetAllStudentsAsync()
        {
            return await _context.Students.ToListAsync();
        }
        public async Task<Student?> GetStudentByIdAsync(string studentId)
        {
            return await _context.Students.FindAsync(studentId);
        }
        public async Task<Student?> GetStudentByEmailAsync(string Email)
        {
            return await _context.Students.FirstOrDefaultAsync(s => s.Email == Email);
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
