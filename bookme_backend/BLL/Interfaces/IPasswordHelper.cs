using bookme_backend.DataAcces.Models;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.General;

namespace bookme_backend.BLL.Interfaces
{
    public interface IPasswordHelper
    {
        string HashPassword(Usuario user, string password);
        bool VerifyPassword(Usuario user, string hashedPassword, string password);
    }

}
