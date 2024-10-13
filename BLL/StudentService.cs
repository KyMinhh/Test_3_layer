using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class StudentService
    {
        private readonly StudentDBContext _context;
        public List<Student> GetAll()
        {
            StudentDBContext context = new StudentDBContext();
            return context.Students.ToList();
        }

        public StudentService()
        {
            // Khởi tạo DbContext, thay thế "DbContext" bằng lớp context thực tế của bạn.
            _context = new StudentDBContext(); // Chắc chắn rằng bạn khởi tạo đúng tên DbContext của dự án
        }

        // Phương thức lấy sinh viên theo ID
        public Student GetById(string studentId)
        {
            return _context.Students.FirstOrDefault(s => s.StudentID == studentId);
        }

        // Phương thức thêm mới sinh viên
        public void Add(Student student)
        {
            _context.Students.Add(student);
            _context.SaveChanges();
        }

        // Phương thức cập nhật sinh viên
        public void Update(Student student)
        {
            var existingStudent = _context.Students.Find(student.StudentID);
            if (existingStudent != null)
            {
                _context.Entry(existingStudent).CurrentValues.SetValues(student);
                _context.SaveChanges();
            }
        }
        public void Delete(Student student)
        {
            // Kiểm tra nếu student không null
            if (student != null)
            {
                _context.Students.Remove(student); // Xóa sinh viên khỏi DbSet
                _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu
            }
        }

        public List<Student> GetAllHasNoMajor(int? facultyID = null)
        {
            if (facultyID.HasValue)
            {
                // Nếu facultyID có giá trị, lọc sinh viên theo khoa
                return _context.Students.Where(p => p.MajorID == null && p.FacultyID == facultyID.Value).ToList();
            }
            else
            {
                // Nếu không có facultyID, lấy tất cả sinh viên không có chuyên ngành
                return _context.Students.Where(p => p.MajorID == null).ToList();
            }
        }


    }
}
