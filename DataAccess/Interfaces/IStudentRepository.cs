using BusinessObject.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetStudentByIdAsync(string studentId);
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        Task<Student?> GetStudentByEmailAsync(string Email);
        void AddStudent(Student student);
        void UpdateStudent(Student student);
        void DeleteStudent(Student student);
    }
}
