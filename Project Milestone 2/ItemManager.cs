using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.Sql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;

namespace Project_Milestone_2
{
    public enum ItemDetail
    {
        Category,
        ItemName,
        Price,
        Quantity
    }
    internal class ItemManager
    {
        readonly SqlConnection sqlConnection;

        public ItemManager(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        // This method simply adds a new item to the sql database.
        public bool AddItem(String name, int cat, int quantity, double price)
        {
            bool success = false;
            string cmdString = "INSERT INTO Items (ItemName, Price, CategoryID, Quantity) VALUES (@name, @price, @cat, @quant)";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            sqlCommand.Parameters.AddWithValue("@name", name);
            sqlCommand.Parameters.AddWithValue("@price", price);
            sqlCommand.Parameters.AddWithValue("@cat", cat);
            sqlCommand.Parameters.AddWithValue("@quant", quantity);       
            int rows = sqlCommand.ExecuteNonQuery();
            if (rows > 0)
            {
                success = true;
            }      
            return success;
        }

        // This method changes the an items information.
        public bool UpdateItemInfo(string update)
        {
            //id#ItemName#Price#Category#Quantity
            // 0 = id
            // 1 = ItemName
            // 2 = Price
            // 3 = Category
            // 4 = Quantity
            var splitUpdate = update.Split('#');

            string id = splitUpdate[0];
            string itemName = splitUpdate[1];
            string price = splitUpdate[2];
            string quantity = splitUpdate[3];
            string category = splitUpdate[4];

            bool success = false;
            string cmdString = $"UPDATE Items SET ItemName = '{itemName}', Price = {price}, CategoryID = {category}, Quantity = {quantity} WHERE ItemID = @id";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            sqlCommand.Parameters.AddWithValue("@id", id);
            int rows = sqlCommand.ExecuteNonQuery();
            if (rows > 0)
            {
                success = true;
            }
            return success;
        }

        // Removes the record of an item from the DB
        public bool RemoveItem(String id)
        {
            bool success = false;
            string cmdString = "DELETE FROM Items WHERE ItemID = @id";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            sqlCommand.Parameters.AddWithValue("@id", id);

            int rows = sqlCommand.ExecuteNonQuery();
            if (rows > 0)
            {
                success = true;
            }

            return success;
        }

        //Show all
        public DataTable ShowAllItems()
        {
            string cmdString = "SELECT I.ItemID, I.ItemName, I.Price, I.Quantity, C.Category FROM Items AS I INNER JOIN Category AS C ON I.CategoryID = C.CategoryID";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);

            return ds.Tables[0];
        }

        public DataTable FilterItems(List<string> filters)
        {
            List<string> fields = new List<string>();
            List<string> signs = new List<string>();
            List<string> values = new List<string>();

            foreach (string filter in filters)
            {
                // 0 = field
                // 1 = sign
                // 2 = value
                var splitFilters = filter.Split('#');
                fields.Add(splitFilters[0]);
                if (splitFilters[1].Equals("="))
                    signs.Add("LIKE");
                else if (splitFilters[1].Equals("!="))
                    signs.Add("NOT LIKE");
                else
                    signs.Add(splitFilters[1]);

                if (splitFilters[0].Equals("Price") || splitFilters[0].Equals("Category"))
                    values.Add(splitFilters[2]);
                else if ((splitFilters[1].Equals("LIKE") || splitFilters[1].Equals("NOT LIKE")) && !splitFilters[1].Equals("="))
                    values.Add($"'%{splitFilters[2]}%'");
                else
                    values.Add($"'{splitFilters[2]}'");
            }
            string cmdString = $"SELECT I.ItemID, I.ItemName, I.Price, I.Quantity, C.Category FROM Items AS I INNER JOIN Category AS C ON I.CategoryID = C.CategoryID WHERE I.{fields[0]} {signs[0]} {values[0]}";
            if (fields.Count > 1)
            {
                for (int i = 1; i < signs.Count; i++)
                {
                    cmdString += $" AND I.{fields[i]} {signs[i]} {values[i]}";
                }
            }
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);

            return ds.Tables[0];
        }

        internal DataTable FillNames()
        {
            string cmdString = "SELECT * FROM Items";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);
            return ds.Tables[0];
        }

        public DataTable FillCategories()
        {
            string cmdString = "SELECT * FROM Category";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);
            return ds.Tables[0];
        }
    }
}
