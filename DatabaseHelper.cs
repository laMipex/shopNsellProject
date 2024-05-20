using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;

namespace BuyNSell
{
    public class DatabaseHelper
    {
        private string connectionString = "Data Source=DESKTOP-2AAQ8OB;Initial Catalog=shopNsell;Integrated Security=True";

        public DataTable GetItems()
        {
            DataTable dataTable = new DataTable();
            string query = "SELECT item_Image_Data, item_Name AS Product, item_Price AS Price, item_Condition AS Condition, item_Description AS Description FROM item";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dataTable);
                }
            }

            dataTable.Columns.Add("Image", typeof(Image));

            foreach (DataRow row in dataTable.Rows)
            {
                if (row["item_Image_Data"] != DBNull.Value)
                {
                    byte[] imageData = (byte[])row["item_Image_Data"];
                    using (MemoryStream ms = new MemoryStream(imageData))
                    {
                        row["Image"] = Image.FromStream(ms);
                    }
                }
            }

            return dataTable;
        }
    }
}
