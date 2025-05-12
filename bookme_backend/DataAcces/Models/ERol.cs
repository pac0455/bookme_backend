using System.Reflection;
using Google.Apis.Util;

namespace bookme_backend.DataAcces.Models
{
    public class ERol
    {
        private string Value {  get; set; }
        public ERol(string value)
        {       
            Value = value;
        }
       
        public static ERol Cliente { get { return new ERol("CLIENTE"); } }
        public static ERol Negocio { get { return new ERol("NEGOCIO"); } }
        public static ERol Admin { get { return new ERol("ADMIN"); } }
    }
}
//RESOURCE: https://stackoverflow.com/questions/630803/associating-enums-with-strings-in-c-sharp
