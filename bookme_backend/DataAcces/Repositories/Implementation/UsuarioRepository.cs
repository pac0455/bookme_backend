using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace bookme_backend.DataAcces.Repositories.Implementation
{
    public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
    {
        

        public UsuarioRepository(BookmeContext context) : base(context)
        {
           
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Usuario> GetByFirebaseUidAsync(string uid)
        {
            return await _dbSet.FirstAsync(u => u.FirebaseUid == uid);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
