using BLL;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace _3_layers
{
    public partial class frmQuanLySinhVien : Form
    {
        private readonly StudentService studentService = new StudentService();
        private readonly FacultyService facultyService = new FacultyService();
        private List<Student> students;

        public frmQuanLySinhVien()
        {
            InitializeComponent();
            ResetInputFields();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                picAvatar.Image = null; // No default image
                setGridViewStyle(dgvStudent);

                // Fill Faculty Combobox and Bind Grid with Student Data
                var faculties = facultyService.GetAll();
                var students = studentService.GetAll();
                FillFacultyCombobox(faculties);
                BindGrid(students);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void FillFacultyCombobox(List<Faculty> faculties)
        {
            faculties.Insert(0, new Faculty()); // Blank option
            cmbFaculty.DataSource = faculties;
            cmbFaculty.DisplayMember = "FacultyName";
            cmbFaculty.ValueMember = "FacultyID";
        }

        private void BindGrid(List<Student> students)
        {
            dgvStudent.Rows.Clear();
            foreach (var student in students)
            {
                int index = dgvStudent.Rows.Add();
                dgvStudent.Rows[index].Cells[0].Value = student.StudentID;
                dgvStudent.Rows[index].Cells[1].Value = student.StudentName;
                dgvStudent.Rows[index].Cells[2].Value = student.Faculty?.FacultyName ?? "N/A";
                dgvStudent.Rows[index].Cells[3].Value = student.AverageScore?.ToString("F2") ?? "N/A";  
                dgvStudent.Rows[index].Cells[4].Value = student.Major?.MajorName ?? "N/A";
            }
        }


        private void ShowAvatar(string imageName)
        {
            picAvatar.Image = null; // Clear the avatar first
            if (!string.IsNullOrEmpty(imageName))
            {
                string imagePath = Path.Combine(Application.StartupPath, "Images", imageName);
                if (File.Exists(imagePath))
                {
                    picAvatar.Image = Image.FromFile(imagePath);
                    picAvatar.Refresh();
                }
            }
        }

        public void setGridViewStyle(DataGridView dgview)
        {
            dgview.BorderStyle = BorderStyle.None;
            dgview.DefaultCellStyle.SelectionBackColor = Color.DarkTurquoise;
            dgview.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgview.BackgroundColor = Color.White;
            dgview.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private void chkUnregisterMajor_CheckedChanged(object sender, EventArgs e)
        {
            List<Student> students = chkUnregisterMajor.Checked
                ? studentService.GetAllHasNoMajor()
                : studentService.GetAll();

            BindGrid(students);
        }

        private void btnAddAndUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate form inputs
                if (!ValidateInputFields()) return;

                // Validate and parse the average score
                if (!decimal.TryParse(txtAverageScore.Text.Trim(), out decimal averageScore))
                {
                    ShowErrorMessage("Điểm trung bình không hợp lệ!", "Lỗi");
                    return;
                }

                // Ensure an image is selected
                string selectedImagePath = picAvatar.Tag?.ToString();
                if (string.IsNullOrEmpty(selectedImagePath))
                {
                    ShowErrorMessage("Vui lòng chọn ảnh!", "Thông báo");
                    return;
                }

                // Save the selected image and get the image file name
                string imageName = SaveImage(selectedImagePath);

                // Create a new Student object from form data
                Student student = new Student
                {
                    StudentID = txtStudentID.Text.Trim(),
                    StudentName = txtFullName.Text.Trim(),
                    FacultyID = (int)cmbFaculty.SelectedValue,
                    AverageScore = averageScore,
                    Avatar = imageName
                };

                // Check if the faculty exists
                if (!FacultyExists(student.FacultyID))
                {
                    ShowErrorMessage("Khoa không tồn tại!", "Lỗi");
                    return;
                }

                // Check if the student already exists
                var existingStudent = studentService.GetById(student.StudentID);
                if (existingStudent != null)
                {
                    // Update the existing student
                    UpdateStudent(existingStudent, student);
                    ShowInfoMessage("Cập nhật sinh viên thành công!", "Thông báo");
                }
                else
                {
                    // Add a new student
                    studentService.Add(student);
                    ShowInfoMessage("Thêm sinh viên mới thành công!", "Thông báo");
                }

                // Refresh the grid to display updated data
                BindGrid(studentService.GetAll());

                // Reset input fields for the next entry
                ResetInputFields();
            }
            catch (DbUpdateException dbEx)
            {
                ShowErrorMessage($"Lỗi cập nhật: {dbEx.InnerException?.Message ?? "Không có thông tin chi tiết"}", "Lỗi cập nhật");
            }
            catch (IOException ioEx)
            {
                ShowErrorMessage($"Lỗi file: {ioEx.Message}", "Lỗi file");
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"Có lỗi xảy ra: {ex.Message}", "Lỗi");
            }
        }


        private string SaveImage(string imagePath)
        {
            string imageDirectory = Path.Combine(Application.StartupPath, "Images");
            EnsureDirectoryExists(imageDirectory);

            string imageName = Path.GetFileName(imagePath);
            string destinationPath = Path.Combine(imageDirectory, imageName);
            File.Copy(imagePath, destinationPath, true);

            return imageName;
        }

        private bool ValidateInputFields()
        {
            if (string.IsNullOrWhiteSpace(txtStudentID.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                cmbFaculty.SelectedIndex <= 0 ||
                string.IsNullOrWhiteSpace(txtAverageScore.Text))
            {
                ShowErrorMessage("Vui lòng nhập đầy đủ thông tin!", "Thông báo");
                return false;
            }
            return true;
        }

        private void EnsureDirectoryExists(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        private bool FacultyExists(int facultyId)
        {
            return facultyService.GetById(facultyId) != null;
        }

        private void UpdateStudent(Student existingStudent, Student updatedStudent)
        {
            existingStudent.StudentName = updatedStudent.StudentName;
            existingStudent.FacultyID = updatedStudent.FacultyID;
            existingStudent.AverageScore = updatedStudent.AverageScore;
            existingStudent.Avatar = updatedStudent.Avatar;

            studentService.Update(existingStudent);
        }

        private void ResetInputFields()
        {
            txtStudentID.Clear();
            txtFullName.Clear();
            cmbFaculty.SelectedIndex = -1;
            txtAverageScore.Clear();
            picAvatar.Image = null;
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtStudentID.Text))
                {
                    MessageBox.Show("Vui lòng chọn sinh viên cần xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string studentId = txtStudentID.Text.Trim();
                var student = studentService.GetById(studentId);

                if (student == null)
                {
                    MessageBox.Show("Sinh viên không tồn tại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = MessageBox.Show("Bạn có chắc chắn muốn xóa sinh viên này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    studentService.Delete(student);
                    MessageBox.Show("Xóa sinh viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    BindGrid(studentService.GetAll());
                    ResetInputFields();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow selectedRow = dgvStudent.Rows[e.RowIndex];
                if (selectedRow.Cells["colMSSV"].Value != null)
                {
                    string studentID = selectedRow.Cells["colMSSV"].Value.ToString();
                    Student student = studentService.GetById(studentID);
                    if (student != null)
                    {
                        txtStudentID.Text = student.StudentID;
                        txtFullName.Text = student.StudentName;
                        txtAverageScore.Text = student.AverageScore?.ToString("F2") ?? string.Empty;
                        cmbFaculty.SelectedValue = student.FacultyID;
                        ShowAvatar(student.Avatar);
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sinh viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnAvatar_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image files (*.jpg, *.jpeg, *.png, *.bmp, *.gif) | *.jpg; *.jpeg; *.png; *.bmp; *.gif";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedImagePath = openFileDialog.FileName;
                    picAvatar.Tag = selectedImagePath; // Correctly set the Tag to the selected image path

                    // Release old image if it exists
                    if (picAvatar.Image != null)
                    {
                        picAvatar.Image.Dispose();
                        picAvatar.Image = null;
                    }

                    try
                    {
                        // Display the selected image
                        using (Image img = Image.FromFile(selectedImagePath))
                        {
                            picAvatar.Image = new Bitmap(img);
                        }

                        picAvatar.Refresh(); // Refresh to ensure image display
                    }
                    catch (IOException ioEx)
                    {
                        MessageBox.Show($"Lỗi truy cập file: {ioEx.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Đã xảy ra lỗi: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void ShowErrorMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowInfoMessage(string message, string caption)
        {
            MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDangKyChuyenNganh_Click(object sender, EventArgs e)
        {
            frmRegister frmRegister = new frmRegister(this);
            frmRegister.ShowDialog();
        }
    }
}
  
