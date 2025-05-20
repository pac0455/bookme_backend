using System.Linq.Expressions;
using bookme_backend.DataAcces.Models;
using bookme_backend.DataAcces.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using Org.BouncyCastle.Asn1;

namespace bookme_backend.DataAcces.Repositories.Implementation
{
    public class HorarioRepository: Repository<Horarios>, IHorarioRepository
    {
        public HorarioRepository(BookmeContext context) : base(context)
        {

        }

        public async Task<(bool, string msg = "")> AddAllAsync(List<Horarios> horarios)
        {
            try
            {
                await _dbSet.AddRangeAsync(horarios);
                return true;
            }
            catch (Exception ex) {
                msg = ex.Message;
                return false;
            }
            
        }


    }
}
