using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessObject.Entities;

namespace DataAccess.Interfaces
{
    public interface IRegistrationFormRepository
    {
        Task<IEnumerable<RegistrationForm>> GetAllForms();
        Task<RegistrationForm?> GetByIdAsync(string formId);
        void Add(RegistrationForm registrationForm);
        void Update(RegistrationForm registrationForm);
        void Delete(RegistrationForm registrationForm);
    }
}
