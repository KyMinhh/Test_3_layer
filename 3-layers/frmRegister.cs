using BLL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3_layers
{
    public partial class frmRegister : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private readonly MajorService majorService = new MajorService();
        private frmQuanLySinhVien _parents = new frmQuanLySinhVien();
        public frmRegister(frmQuanLySinhVien parents)
        {
            InitializeComponent();
            _parents = parents;
        }

        private void frmRegister_Load(object sender, EventArgs e)
        {
            try
            {
                var listFacultys = facultyService.GetAll();
                FillFalcultyCombobox(listFacultys);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFalcultyCombobox(List<Faculty> listFacultys)
        {
            this.cmbFaculty.DataSource = listFacultys;
            this.cmbFaculty.DisplayMember = "FacultyName";
            this.cmbFaculty.ValueMember = "FacultyID";
        }

        private void FillMajorCombobox(List<Major> listMajors)
        {
            this.cmbChuyenNganh.DataSource = listMajors;
            this.cmbChuyenNganh.DisplayMember = "MajorName";
            this.cmbChuyenNganh.ValueMember = "MajorID";
        }

        private void cmbFaculty_SelectedIndexChanged(object sender, EventArgs e)
        {
            Faculty selectedFaculty = cmbFaculty.SelectedItem as Faculty;
            if (selectedFaculty != null)
            {
                // Kiểm tra nếu khoa là "Quản trị kinh doanh"
                if (selectedFaculty.FacultyName == "Quản trị kinh doanh")
                {
                    // Xóa dữ liệu trong combobox Major
                    cmbChuyenNganh.DataSource = null;
                    cmbChuyenNganh.Items.Clear(); // Xóa các item đang có trong cmbMajor
                }
                else
                {
                    // Nếu không phải khoa Quản trị kinh doanh, load lại các Major
                    var listMajor = majorService.GetAllByFaculty(selectedFaculty.FacultyID);
                    FillMajorCombobox(listMajor);
                }

                // Load lại danh sách sinh viên chưa đăng ký chuyên ngành
                var listStudent = studentService.GetAllHasNoMajor(selectedFaculty.FacultyID);
                BindGrid(listStudent);
            }
        }


        private void BindGrid(List<Student> listStudent)
        {
            dgvStudent.Rows.Clear();
            foreach (var item in listStudent)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[1].Value = item.StudentID;
                dgvStudent.Rows[index].Cells[2].Value = item.StudentName;
                if (item.Faculty != null)
                    dgvStudent.Rows[index].Cells[3].Value =
                    item.Faculty.FacultyName;
                dgvStudent.Rows[index].Cells[4].Value = item.AverageScore +
                "";
                if (item.MajorID != null)
                    dgvStudent.Rows[index].Cells[5].Value = item.Major.MajorName +
                    "";
                //ShowAvatar(item.Avatar);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                // Check if a major is selected
                if (cmbChuyenNganh.SelectedIndex <= 0)
                {
                    MessageBox.Show("Vui lòng chọn chuyên ngành!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Get selected MajorID from the combo box
                int selectedMajorId = (int)cmbChuyenNganh.SelectedValue;

                // List to hold the students to be updated
                List<Student> studentsToUpdate = new List<Student>();

                // Loop through DataGridView rows to find selected students
                foreach (DataGridViewRow row in dgvStudent.Rows)
                {
                    // Check if the checkbox in the first column is checked
                    if (Convert.ToBoolean(row.Cells[0].Value)) // Assuming the first column is for checkboxes
                    {
                        // Get the StudentID from the second column
                        string studentID = row.Cells[1].Value?.ToString();

                        // Retrieve the student object
                        var student = studentService.GetById(studentID);
                        if (student != null)
                        {
                            // Update the student's MajorID
                            student.MajorID = selectedMajorId;
                            studentsToUpdate.Add(student); // Add to the list for batch update
                        }
                    }
                }

                // Check if any students are selected
                if (studentsToUpdate.Count == 0)
                {
                    MessageBox.Show("Vui lòng chọn ít nhất một sinh viên!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Register students for the selected major
                foreach (var student in studentsToUpdate)
                {
                    studentService.Update(student); // Update the student's information in the database
                }

                MessageBox.Show("Đăng ký chuyên ngành cho sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Refresh the DataGridView to reflect updated data
                var listStudent = studentService.GetAllHasNoMajor((int)cmbFaculty.SelectedValue);
                BindGrid(listStudent);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
