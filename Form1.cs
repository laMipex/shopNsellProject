﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using iTextSharp.text;
using iTextSharp.text.pdf;
using IronBarCode;


namespace BuyNSell
{
    public partial class Form1 : Form
    {
        string connectionString = "Data Source=DESKTOP-2AAQ8OB;Initial Catalog=shopNsell;Integrated Security=True";
        private DatabaseHelper dbHelper = new DatabaseHelper();
        private string imageUrl;
        bool login = false;

        public Form1()
        {
            InitializeComponent();
            txtBoxPassword.PasswordChar = '*';
            txtBoxPasswordReg.PasswordChar = '*';
            LoadItems();
            LoadItemTypes();

            dataGridView1.CellFormatting += DataGridView1_CellFormatting;

            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dataGridView1.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            dataGridView1.ColumnHeadersDefaultCellStyle.Font = new System.Drawing.Font("Lovelo Black", 10);
            dataGridView1.EnableHeadersVisualStyles = false;

            dataGridView1.DefaultCellStyle.BackColor = Color.FromArgb(30, 30, 30);
            dataGridView1.DefaultCellStyle.ForeColor = Color.White;
            dataGridView1.DefaultCellStyle.Font = new System.Drawing.Font("Lovelo Black", 15);

        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void btnTabRegister_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 2;
        }

        private void btnTabHome_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 0;
            btnTabHome.BackColor = Color.FromArgb(114, 47, 255);
            btnTabLogin.BackColor = Color.FromArgb(124, 77, 255);
            btnTabCart.BackColor = Color.FromArgb(124, 77, 255);
            btnTabAdd.BackColor = Color.FromArgb(124, 77, 255);
        }

        private void btnTabLogin_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 1;
            btnTabHome.BackColor = Color.FromArgb(124, 77, 255);
            btnTabLogin.BackColor = Color.FromArgb(114, 47, 255);
            btnTabCart.BackColor = Color.FromArgb(124, 77, 255);
            btnTabAdd.BackColor = Color.FromArgb(124, 77, 255);
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string username = txtBoxUsernameReg.Text;
            string email = txtBoxEmailReg.Text;
            string password = txtBoxPasswordReg.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please Fill in All The Fields!");
                return;
            }

            string hashedPassword = HashPassword(password);
            RegisterUser(username, email, hashedPassword);
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void RegisterUser(string username, string email, string password)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Users (Username, Email, Password) VALUES (@Username, @Email, @Password)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Registration Successful!");
                        tabControl1.SelectedIndex = 1;
                        btnTabHome.BackColor = Color.FromArgb(114, 47, 255);
                        btnTabLogin.BackColor = Color.FromArgb(124, 77, 255);
                        btnTabCart.BackColor = Color.FromArgb(124, 77, 255);
                        btnTabAdd.BackColor = Color.FromArgb(124, 77, 255);
                    }
                    else
                    {
                        MessageBox.Show("Registration Failed. Please Try Again.");
                    }
                }
            }
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtBoxUsername.Text;
            string password = txtBoxPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Username and Password are Required!");
                return;
            }

            string hashedPassword = HashPassword(password);
            if (AuthenticateUser(username, hashedPassword))
            {
                MessageBox.Show("Login Successful!");
                tabControl1.SelectedIndex = 0;

                btnTabAdd.Visible = true;
                btnTabCart.Visible = true;
                btnTabLogout.Visible = true;
                btnTabLogin.Visible = false;

                btnTabHome.BackColor = Color.FromArgb(114, 47, 255);
                btnTabLogin.BackColor = Color.FromArgb(124, 77, 255);
                btnTabCart.BackColor = Color.FromArgb(124, 77, 255);
                btnTabAdd.BackColor = Color.FromArgb(124, 77, 255);

                lbWelcome.Text = "Welcome " + username + "!";
                lbWelcome.Visible = true;

                login = true;
            }
            else
            {
                MessageBox.Show("Invalid Username or Password.");
            }
        }

        private bool AuthenticateUser(string username, string hashedPassword)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Password FROM Users WHERE Username = @Username";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Username", username);
                    connection.Open();
                    var result = command.ExecuteScalar();

                    if (result != null)
                    {
                        string storedPasswordHash = result.ToString();
                        return storedPasswordHash == hashedPassword;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        private void btnTabAdd_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 3;
            btnTabHome.BackColor = Color.FromArgb(124, 77, 255);
            btnTabLogin.BackColor = Color.FromArgb(124, 77, 255);
            btnTabCart.BackColor = Color.FromArgb(124, 77, 255);
            btnTabAdd.BackColor = Color.FromArgb(114, 47, 255);
        }

        private void btnTabCart_Click(object sender, EventArgs e)
        {
            tabControl1.SelectedIndex = 4;
            btnTabHome.BackColor = Color.FromArgb(124, 77, 255);
            btnTabLogin.BackColor = Color.FromArgb(124, 77, 255);
            btnTabCart.BackColor = Color.FromArgb(114, 47, 255);
            btnTabAdd.BackColor = Color.FromArgb(124, 77, 255);
        }

        public DataTable GetItems()
        {
            DataTable dataTable = new DataTable();
            string query = "SELECT item_Image_Data AS Image, item_Name AS Product, item_Price AS Price, item_Condition AS Condition, item_Description AS Description FROM item";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
            }
            return dataTable;
        }

        private void LoadItems()
        {
            try
            {
                DataTable items = GetItems();

                if (items.Rows.Count > 0)
                {
                    dataGridView1.DataSource = items;

                    if (dataGridView1.Columns.Contains("item_Image_Data"))
                    {
                        dataGridView1.Columns["item_Image_Data"].Visible = false;
                    }
                    if (dataGridView1.Columns.Contains("item_Image"))
                    {
                        dataGridView1.Columns["item_Image"].Visible = false;
                    }

                    if (dataGridView1.Columns.Contains("Image"))
                    {
                        dataGridView1.Columns["Image"].DisplayIndex = 0;
                        dataGridView1.Columns["Image"].HeaderText = "Product Image";
                        dataGridView1.Columns["Image"].Width = 150;
                    }

                    if (dataGridView1.Columns.Contains("Product"))
                    {
                        dataGridView1.Columns["Product"].Width = 200;
                    }
                    if (dataGridView1.Columns.Contains("Price"))
                    {
                        dataGridView1.Columns["Price"].Width = 100;
                    }
                    if (dataGridView1.Columns.Contains("Condition"))
                    {
                        dataGridView1.Columns["Condition"].Width = 100;
                    }
                    if (dataGridView1.Columns.Contains("Description"))
                    {
                        dataGridView1.Columns["Description"].Width = 300;
                    }

                    dataGridView1.RowTemplate.Height = 100;
                    dataGridView1.SelectionChanged += DataGridView1_SelectionChanged;
                }
                else
                {
                    MessageBox.Show("No Items Found in The Database.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Loading Items: " + ex.Message);
            }
        }

        private void DataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataRowView row = (DataRowView)dataGridView1.SelectedRows[0].DataBoundItem;
                byte[] imageData = row["Image"] as byte[];
                if (imageData != null)
                {
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        pictureBox1.Image = System.Drawing.Image.FromStream(ms);
                    }
                }
                else
                {
                    pictureBox1.Image = null;
                }
            }
        }

        private void DataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns["Image"].Index && e.Value != null)
            {
                byte[] imageBytes = e.Value as byte[];
                if (imageBytes != null)
                {
                    using (MemoryStream ms = new MemoryStream(imageBytes))
                    {
                        System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                        e.Value = new Bitmap(img, new Size(100, 100));
                    }
                }
            }
        }


        private void btnTabLogout_Click(object sender, EventArgs e)
        {
            MessageBox.Show("You Are Logged Out!");

            tabControl1.SelectedIndex = 0;
            btnTabLogout.Visible = false;
            btnTabAdd.Visible = false;
            btnTabCart.Visible = false;
            btnTabLogin.Visible = true;

            txtBoxUsername.Text = "";
            txtBoxPassword.Text = "";
            lbWelcome.Visible = false;

            labelCartEmpty.Visible = true;
            btnReceipt.Visible = false;
        }

        private void btnUpload_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    imageUrl = ofd.FileName;
                }
            }
        }

        private byte[] ImageToByteArray(string imagePath)
        {
            using (System.Drawing.Image image = System.Drawing.Image.FromFile(imagePath))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    image.Save(ms, image.RawFormat);
                    return ms.ToArray();
                }
            }
        }

        private void LoadItemTypes()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT item_typeID, item_Type FROM itemType";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        comBoxProdType.DataSource = dataTable;
                        comBoxProdType.DisplayMember = "item_Type";
                        comBoxProdType.ValueMember = "item_typeID";
                    }
                }
            }
        }

        private void btnAddProduct_Click(object sender, EventArgs e)
        {
            string productName = txtBoxProdTitle.Text;
            string productDescription = txtBoxProdDesc.Text;
            string productCondition = comBoxProdCond.SelectedItem?.ToString();
            decimal productPrice;
            int itemTypeID;

            if (string.IsNullOrEmpty(productName) || string.IsNullOrEmpty(productDescription) ||
                string.IsNullOrEmpty(productCondition) || !decimal.TryParse(txtBoxProdPrice.Text, out productPrice) ||
                string.IsNullOrEmpty(imageUrl) || comBoxProdType.SelectedIndex == -1)
            {
                MessageBox.Show("Please Fill in All the Fields and Upload an Image.");
                return;
            }

            if (!int.TryParse(comBoxProdType.SelectedValue.ToString(), out itemTypeID))
            {
                MessageBox.Show("Invalid Ttem Type Selected.");
                return;
            }

            byte[] productImage = ImageToByteArray(imageUrl);
            AddProductToDatabase(productName, productDescription, productCondition, productPrice, productImage, itemTypeID);
            LoadItems();
        }


        private void AddProductToDatabase(string name, string description, string condition, decimal price, byte[] image, int itemTypeID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO item (item_Name, item_Description, item_Condition, item_Price, item_Image_Data, item_typeID) " +
                               "VALUES (@Name, @Description, @Condition, @Price, @Image, @ItemTypeID)";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", name);
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Condition", condition);
                    command.Parameters.AddWithValue("@Price", price);
                    command.Parameters.AddWithValue("@Image", image);
                    command.Parameters.AddWithValue("@ItemTypeID", itemTypeID);

                    connection.Open();
                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Product Added Successfully!");
                        txtBoxProdTitle.Text = "";
                        txtBoxProdDesc.Text = "";
                        comBoxProdCond.SelectedIndex = -1;
                        txtBoxProdPrice.Text = "";
                        comBoxProdType.SelectedIndex = -1;
                        imageUrl = null;
                    }
                    else
                    {
                        MessageBox.Show("Failed to Add Product!");
                    }
                }
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comBoxProdType.SelectedIndex = -1;
        }

        private void GeneratePdfWithQrCode(string filePath, DataGridViewRow selectedRow)
        {
            
            string productName = selectedRow.Cells["Product"].Value.ToString();
            string productDescription = selectedRow.Cells["Description"].Value.ToString();
            string productCondition = selectedRow.Cells["Condition"].Value.ToString();
            string productPrice = selectedRow.Cells["Price"].Value.ToString();
            byte[] imageData = selectedRow.Cells["Image"].Value as byte[];

            string qrText = $"Product: {productName}\nDescription: {productDescription}\nCondition: {productCondition}\nPrice: {productPrice}";
            var qrCode = QRCodeWriter.CreateQrCode(qrText, 200);
            var qrCodeImage = qrCode.ToBitmap();

            using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var document = new Document(PageSize.A5, 50, 50, 25, 25))
            using (var writer = PdfWriter.GetInstance(document, fs))
            {
                document.Open();

                if (imageData != null)
                {
                    iTextSharp.text.Image itemImage = iTextSharp.text.Image.GetInstance(imageData);
                    itemImage.ScaleToFit(150f, 150f);
                    itemImage.Alignment = Element.ALIGN_LEFT;
                    document.Add(itemImage);
                }

                document.Add(new Paragraph($"Product: {productName}", FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16)));
                document.Add(new Paragraph($"Description: {productDescription}", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                document.Add(new Paragraph($"Condition: {productCondition}", FontFactory.GetFont(FontFactory.HELVETICA, 12)));
                document.Add(new Paragraph($"Price: {productPrice}", FontFactory.GetFont(FontFactory.HELVETICA, 12)));

                using (var qrCodeStream = new MemoryStream())
                {
                    qrCodeImage.Save(qrCodeStream, System.Drawing.Imaging.ImageFormat.Png);
                    var qrCodeBytes = qrCodeStream.ToArray();
                    iTextSharp.text.Image qrCodePdfImage = iTextSharp.text.Image.GetInstance(qrCodeBytes);
                    qrCodePdfImage.ScaleToFit(150f, 150f);
                    qrCodePdfImage.Alignment = Element.ALIGN_LEFT;
                    document.Add(qrCodePdfImage);
                }

                document.Close();
            }

            if (login == false)
            {
                MessageBox.Show("Please Login First!");
            }
            else
            {
                MessageBox.Show("Please Procide To Your Cart!");
            }
        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
            {
                DataGridViewRow selectedRow = dataGridView1.Rows[e.RowIndex];
                string filePath = Path.Combine(Application.StartupPath, "Receipt.pdf");
                GeneratePdfWithQrCode(filePath, selectedRow);
                labelCartEmpty.Visible = false;
                btnReceipt.Visible = true;
            }
        }

        private void btnReceipt_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("Receipt.pdf");
        }
    }
}