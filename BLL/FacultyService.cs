using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class FacultyService
    {
        private readonly StudentDBContext _context; // Đảm bảo bạn đã khởi tạo DbContext

        public FacultyService()
        {
            _context = new StudentDBContext(); // Khởi tạo DbContext ở đây
        }

        // Phương thức GetById để lấy khoa theo ID
        public Faculty GetById(int facultyId)
        {
            return _context.Faculties.FirstOrDefault(f => f.FacultyID == facultyId);
        }
        public List<Faculty> GetAll()
        {
            StudentDBContext context = new StudentDBContext();
            return context.Faculties.ToList();
        }
    }
}
