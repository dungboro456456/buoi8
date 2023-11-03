using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using System.Xml.Linq;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace QuanLiSinhViennn
{
    public partial class Form1 : Form
    {
        private string MyConnection = "SERVER=localhost;" +
            "DATABASE=qlisinhvien;" +
            "UID=sa;" +
            "PASSWORD=Dungboro1;";

        private int index;


        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;

        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void btThem_Click(object sender, EventArgs e)
        {
            try
            {
                string mssv = textboxMasv.Text;
                string name = textBoxName.Text;
                string tuoiText = textBoxTuoi.Text;
                bool gioiTinh = radiobtnam.Checked;
                if (string.IsNullOrWhiteSpace(mssv) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(tuoiText))
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                    return;
                }

                if (IsMssvExists(mssv))
                {
                    MessageBox.Show("Mã số sinh viên đã tồn tại. Vui lòng chọn một mã số khác.");
                    return;
                }
                if (!radiobtnam.Checked && !radiobtnu.Checked)
                {
                    MessageBox.Show("Bạn chưa chọn giới tính.");
                    return; // Thoát khỏi phương thức hoặc xử lý thêm lỗi khác ở đây (tuỳ vào yêu cầu).
                }



                byte[] hinhanhBytes = GetImageDataFromPictureBox();

                using (SqlConnection connection = new SqlConnection(MyConnection))
                {
                    connection.Open();

                    string query = "INSERT INTO sinhvien (mssv, name, ngaysinh, gioitinh, hinhanh) VALUES (@mssv, @name, @ngaysinh, @gioitinh, @hinhanh)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@ngaysinh", Convert.ToDateTime(tuoiText));
                        command.Parameters.AddWithValue("@gioitinh", gioiTinh);
                        command.Parameters.AddWithValue("@mssv", mssv);
                        command.Parameters.AddWithValue("@hinhanh", hinhanhBytes);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Thêm sinh viên " + mssv + " thành công.");
                        }
                        else
                        {
                            MessageBox.Show("Thêm sinh viên thất bại.");
                        }
                    }
                }
                lammoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }


        private bool IsMssvExists(string mssv)
        {
            using (SqlConnection connection = new SqlConnection(MyConnection))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM sinhvien WHERE mssv = @mssv";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@mssv", mssv);

                    int count = (int)command.ExecuteScalar();

                    return count > 0;
                }
            }
        }

        private byte[] GetImageDataFromPictureBox()
        {
            if (pictureBox1.Image == null)
            {
                return null;
            }

            using (MemoryStream ms = new MemoryStream())
            {
                pictureBox1.Image.Save(ms, ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }
        private void pictureBox1_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog opnfd = new OpenFileDialog();
                opnfd.Filter = "Image Files (*.jpg;*.jpeg;*.gif;*.png)|*.jpg;*.jpeg;*.gif;*.png";

                if (opnfd.ShowDialog() == DialogResult.OK)
                {
                    // Kiểm tra kích thước tối đa cho hình ảnh (ví dụ: giới hạn kích thước 300x300)
                    int maxWidth = 500;
                    int maxHeight = 500;

                    Image selectedImage = Image.FromFile(opnfd.FileName);

                    // Kiểm tra kích thước của hình ảnh
                    if (selectedImage.Width <= maxWidth && selectedImage.Height <= maxHeight)
                    {
                        pictureBox1.Image = new Bitmap(selectedImage);
                    }
                    else
                    {
                        MessageBox.Show("Hình ảnh vượt quá kích thước tối đa (" + maxWidth + "x" + maxHeight + "). Vui lòng chọn hình ảnh khác.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "Có vẻ như thiếu hình rồi");
            }
        }

        private void btLamMoi_Click(object sender, EventArgs e)
        {

            lammoi();
            textboxMasv.Text = String.Empty;
            textboxMasv.Enabled = true;
            textBoxName.Text = String.Empty;
            textBoxTuoi.Text = String.Empty;
            tbTimkiem.Text = String.Empty;
            pictureBox1.Image = null;
        }

        private void btThoat_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn thoát khỏi ứng dụng?", "Xác nhận thoát", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                // Người dùng đã xác nhận thoát, đóng ứng dụng
                Close();
            }
            else
            {
                // Người dùng đã hủy việc thoát, không làm gì cả
            }
        }


        private void btXoa_Click(object sender, EventArgs e)
        {
            try
            {
                if (dataGridView1.Rows.Count > 0 && index >= 0)
                {
                    // Kiểm tra nếu danh sách không rỗng và index hợp lệ thì thực hiện xóa
                    DataGridViewRow row = dataGridView1.Rows[index];
                    string mssvToDelete = row.Cells[0].Value.ToString().Trim();
                    using (SqlConnection connection = new SqlConnection(MyConnection))
                    {
                        connection.Open();

                        string query = "DELETE FROM sinhvien WHERE mssv = @mssv";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@mssv", mssvToDelete);
                            int result = command.ExecuteNonQuery();
                            if (result > 0)
                            {
                                MessageBox.Show("Xóa thông tin sinh viên " + mssvToDelete + " thành công.");
                                lammoi();
                                if (dataGridView1.Rows.Count == 0)
                                {
                                    int? index = null;
                                }

                                // Không cần tăng index vì dòng đã bị xóa
                            }
                            else
                            {
                                MessageBox.Show("Xóa thông tin sinh viên " + mssvToDelete + " thất bại.");
                                lammoi();
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Không có dữ liệu hoặc bạn chưa chọn một sinh viên để xóa.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }






        private void button1_TimKiem_Click(object sender, EventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(MyConnection))
            {
                connection.Open();

                string query = "select *  from sinhvien  WHERE mssv like @input or ngaysinh like @input or gioitinh like @input or name like @input";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@input", "%" + tbTimkiem.Text + "%");
                    SqlDataReader reader = command.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);
                        dataGridView1.DataSource = dataTable;
                    }
                    else
                    {
                        MessageBox.Show("Không tìm thấy sinh viên tương ứng với thông tin " + tbTimkiem.Text);
                    }
                }
            }
        }
        private bool IsMssvExistsForUpdate(string mssv)
        {
            using (SqlConnection connection = new SqlConnection(MyConnection))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM sinhvien WHERE mssv = @mssv";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@mssv", mssv);

                    int count = (int)command.ExecuteScalar();

                    return count > 1; // Kiểm tra nếu có nhiều hơn 1 bản ghi (đã tồn tại trước đó) có cùng Mssv.
                }
            }
        }


        private void btSua_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textboxMasv.Text) || string.IsNullOrWhiteSpace(textBoxName.Text) || string.IsNullOrWhiteSpace(textBoxTuoi.Text) || pictureBox1.Image == null)
                {
                    MessageBox.Show("Vui lòng điền đầy đủ thông tin.");
                    return;
                }
                if (index < 0)
                {
                    MessageBox.Show("Vui lòng chọn một sinh viên để sửa.");
                    return;
                }
                if (!radiobtnam.Checked && !radiobtnu.Checked)
                {
                    MessageBox.Show("Bạn chưa chọn giới tính.");
                    return; // Thoát khỏi phương thức hoặc xử lý thêm lỗi khác ở đây (tuỳ vào yêu cầu).
                }

                string mssv = textboxMasv.Text;
                string name = textBoxName.Text;
                string tuoiText = textBoxTuoi.Text;
                bool gioiTinh = radiobtnam.Checked;

                // Kiểm tra xem Mssv đã thay đổi so với giá trị ban đầu
                string mssv2 = dataGridView1.Rows[index].Cells[0].Value.ToString().Trim();
                if (!mssv.Equals(mssv2))
                {
                    MessageBox.Show("Không được thay đổi MSSV");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(MyConnection))
                {
                    connection.Open();

                    string query = "UPDATE sinhvien SET mssv = @mssv, name = @name, ngaysinh = @ngaysinh, gioitinh = @gioitinh, hinhanh = @hinhanh WHERE mssv = @mssv2";

                    byte[] hinhanhBytes = GetImageDataFromPictureBox();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", name);
                        command.Parameters.AddWithValue("@ngaysinh", Convert.ToDateTime(tuoiText));
                        command.Parameters.AddWithValue("@gioitinh", gioiTinh);
                        command.Parameters.AddWithValue("@mssv", mssv);
                        command.Parameters.AddWithValue("@hinhanh", hinhanhBytes);
                        command.Parameters.AddWithValue("@mssv2", mssv2); // MSSV cũ

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            MessageBox.Show("Sửa thông tin sinh viên " + mssv + " thành công.");
                        }
                        else
                        {
                            MessageBox.Show(result.ToString());
                        }
                    }
                }
                lammoi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
        public void indamdong()
        {
            DataGridViewRow selectedRow = dataGridView1.Rows[index];
            selectedRow.Selected = true;
            selectedRow.DefaultCellStyle.SelectionBackColor = Color.LightBlue; // Màu nền khi chọn
            selectedRow.DefaultCellStyle.SelectionForeColor = Color.Black; // Màu chữ khi chọn
        }

        public void lammoi()
        {
            using (SqlConnection connection = new SqlConnection(MyConnection))
            {
                try
                {
                    connection.Open();

                    string query = "SELECT Mssv, Name, NgaySinh, GioiTinh, HinhAnh FROM SinhVien";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt;

                    foreach (DataRow drow in dt.Rows)
                    {
                        byte[] imageBytes = (byte[])drow["hinhanh"];
                        Image image = Image.FromStream(new MemoryStream(imageBytes));
                    }
                    dataGridView1.ClearSelection();
                    if (index == dataGridView1.Rows.Count - 1)
                    {
                        // Nếu index là chỉ mục cuối cùng, hãy cập nhật index thủ công.
                        // Điều này đảm bảo rằng index sẽ không vượt quá giới hạn của danh sách.
                        index = dataGridView1.Rows.Count - 2; // Hoặc cách khác bạn muốn.
                        indamdong();
                    }
                    else
                    {
                        indamdong();
                    }

                    connection.Close();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);

                }
            }
        }
        public void clickchuot(DataGridViewCellEventArgs e)
        {
            try
            {
                index = e.RowIndex;
                DataGridViewRow row = dataGridView1.Rows[index];
                textboxMasv.Enabled = false;
                textboxMasv.Text = row.Cells[0].Value.ToString().Trim();
                textBoxName.Text = row.Cells[1].Value.ToString().Trim();
                textBoxTuoi.Text = row.Cells[2].Value.ToString().Trim();
                bool checkgioitinh = (bool)(row.Cells[3].Value);
                if (checkgioitinh != null && checkgioitinh)
                {
                    radiobtnam.Checked = true;
                }
                else { radiobtnu.Checked = true; }

                byte[] imgData = (byte[])dataGridView1.CurrentRow.Cells[4].Value;
                MemoryStream ms = new MemoryStream(imgData);
                pictureBox1.Image = Image.FromStream(ms);


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\ndưới đó chưa có dữ liệu đâu");
            }
        }


        public void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            index = e.RowIndex;
            if (index >= 0)
            {
                // Bạn muốn thay đổi màu của hàng, hãy đặt màu cho tất cả ô trong hàng
                indamdong();
            }
            clickchuot(e);
        }

        private void btexportpdf_Click(object sender, EventArgs e)
        {

        }

        private void btexportexcel_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();

                // Thiết lập đường dẫn mặc định
                saveFileDialog.InitialDirectory = "G:\\";

                // Thiết lập các thuộc tính của SaveFileDialog
                saveFileDialog.Filter = "Excel Files|*.xlsx";
                saveFileDialog.Title = "Chọn vị trí và tên tệp Excel";
                saveFileDialog.FileName = "SinhVien.xlsx"; // Tên mặc định

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;

                    // Tạo một tệp Excel mới
                    XSSFWorkbook workbook = new XSSFWorkbook();
                    ISheet sheet = workbook.CreateSheet("Sheet1");

                    // Bắt đầu ghi dữ liệu vào sheet
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        IRow row = sheet.CreateRow(i);
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            ICell cell = row.CreateCell(j);

                            object cellValue = dataGridView1.Rows[i].Cells[j].Value;
                            if (cellValue != null)
                            {
                                if (cellValue is byte[] imageData) // Kiểm tra xem dữ liệu có phải là hình ảnh không
                                {
                                    // Ghi hình ảnh vào tệp Excel
                                    var pictureIdx = workbook.AddPicture(imageData, PictureType.JPEG);
                                    var drawing = sheet.CreateDrawingPatriarch();
                                    var anchor = new XSSFClientAnchor(0, 0, 0, 0, j, i, j + 1, i + 1);
                                    var picture = drawing.CreatePicture(anchor, pictureIdx);
                                }
                                else
                                {
                                    cell.SetCellValue(cellValue.ToString());
                                }
                            }
                            else
                            {
                                cell.SetCellValue(string.Empty);
                            }
                        }
                    }

                    // Điều chỉnh kích thước cột cho phù hợp với nội dung
                    for (int columnIndex = 0; columnIndex < dataGridView1.Columns.Count; columnIndex++)
                    {
                        sheet.AutoSizeColumn(columnIndex);
                    }

                    // Lưu tệp Excel tại vị trí đã chọn
                    using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                    {
                        workbook.Write(fileStream);
                    }

                    // Đóng workbook sau khi hoàn thành
                    workbook.Close();

                    // Thông báo thành công
                    MessageBox.Show("Xuất dữ liệu thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                // Thông báo lỗi nếu có lỗi xảy ra
                MessageBox.Show("Có lỗi xảy ra: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }






    }
}
