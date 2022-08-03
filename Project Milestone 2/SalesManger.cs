using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using System.Data;

namespace Project_Milestone_2
{
    internal class SalesManger
    {
        readonly SqlConnection sqlConnection;

        public SalesManger(SqlConnection sqlConnection)
        {
            this.sqlConnection = sqlConnection;
        }

        public bool AddBlankSale(DateTime time)
        {
            bool success = false;         
            string cmdString = $"INSERT INTO Sales (TotalPrice, TimePlaced) VALUES (0, '{time}')";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            try
            {
                int itemRows = sqlCommand.ExecuteNonQuery();      

                if (itemRows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;

        }

        public bool AddSaleDetail(string saleID, string itemID, int quantity)
        {
            bool success = false;

            string cmdString = $"SELECT * FROM SaleItems WHERE SaleID LIKE '{saleID}' AND ItemID LIKE '{itemID}'";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            try
            {
                sqlCommand.ExecuteNonQuery();

                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        return false;
                    }
                }
            }
            catch
            {

            }

            cmdString = $"SELECT Price FROM Items WHERE ItemID = {itemID}";
            try
            {
                double price = 0;
                using (SqlDataReader dataReader = sqlCommand.ExecuteReader())
                {
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            price = (double)dataReader["Price"];
                        }
                    }
                }
                cmdString = $"INSERT INTO SaleItems (SaleID, ItemID, Price, Quantity) VALUES ({saleID}, {itemID}, {price}, {quantity})";
                sqlCommand.CommandText = cmdString;
                int rows = sqlCommand.ExecuteNonQuery();
                if (rows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }

        public bool RemoveSaleDetail(string saleID, string itemID, int quantity, string price)
        {
            bool success = false;
            string cmdString = $"DELETE FROM SaleItems WHERE SaleID = {saleID} AND ItemID = {itemID} AND Quantity = {quantity} AND Price = {price}";
            try
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = sqlConnection,
                    CommandText = cmdString
                };
                int rows = sqlCommand.ExecuteNonQuery();
                if (rows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }

        public bool DeleteSaleDetail(string saleID, string itemID, int quantity, string price)
        {
            bool success = false;
            string cmdString = $"DELETE FROM SaleItems WHERE SaleID = {saleID} AND ItemID = {itemID} AND Quantity = {quantity} AND Price = {price}";
            try
            {
                SqlCommand sqlCommand = new SqlCommand
                {
                    Connection = sqlConnection,
                    CommandText = cmdString
                };
                int rows = sqlCommand.ExecuteNonQuery();
                if (rows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }

        public bool AddSale(List<int> quantities, List<double> prices, List<int> itemIDs)
        {
            bool success = false;
            string values = $"({itemIDs[0]}, {prices[0]}, {quantities[0]})";
            for (int i = 1; i < quantities.Count; i++)
            {
                values += $", ({itemIDs[i]}, {prices[i]}, {quantities[i]})";
            }
            string cmdString = $"INSERT INTO SaleItems (ItemID, Price, Quantity) VALUES {values}";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            try
            {
                int itemRows = sqlCommand.ExecuteNonQuery();
                for (int i = 0; i < quantities.Count; i++)
                {
                    cmdString = "UPDATE Items SET Quantity = Quantity - @quant WHERE ItemID = @id";
                    sqlCommand.CommandText = cmdString;
                    sqlCommand.Parameters.AddWithValue("@id", itemIDs[i]);
                    sqlCommand.Parameters.AddWithValue("@quant", quantities[i]);
                    sqlCommand.ExecuteNonQuery();
                }

                cmdString = $"INSERT INTO Sales (TotalPrice, TimePlaced) VALUES ({prices.Sum()}, #{DateTime.Now}#)";
                sqlCommand.CommandText = cmdString;
                int saleRows = sqlCommand.ExecuteNonQuery();

                if (itemRows > 0 && saleRows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }

        // Removes the record of an item from the DB/////////////////////////////////////////////////////
        public bool RemoveSale(String id)
        {
            bool success = false;
            string cmdString = $"DELETE FROM SaleItems WHERE SaleID = {id}";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            try
            {
                int itemRows = sqlCommand.ExecuteNonQuery();
                cmdString = $"DELETE FROM Sales WHERE SaleID = {id}";
                sqlCommand.CommandText = cmdString;
                int saleRows = sqlCommand.ExecuteNonQuery();
                if (itemRows > 0 && saleRows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }

        //Show all sales///////////////////////////////////////////////////
        public DataTable ShowAllSales()
        {
            string cmdString = "SELECT SaleID, TotalPrice, TimePlaced FROM Sales";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);

            return ds.Tables[0];
        }

        public DataTable ShowSaleDetails(String id)
        {
            string cmdString = $"SELECT I.ItemName, S.Price, S.Quantity FROM SaleItems S INNER JOIN Items I ON I.ItemID = S.ItemID WHERE S.SaleID = {id}";
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);

            return ds.Tables[0];
        }

        public DataTable FilterSales(List<string> filters)
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

                

                if (splitFilters[1].Equals("=") && !splitFilters[0].Equals("TimePlaced"))
                    signs.Add("LIKE");
                else if (splitFilters[1].Equals("!=") && !splitFilters[0].Equals("TimePlaced"))
                    signs.Add("NOT LIKE");
                else
                    signs.Add(splitFilters[1]);

                if ((splitFilters[1].Equals("LIKE") || splitFilters[1].Equals("NOT LIKE")) && !splitFilters[1].Equals("="))
                    values.Add($"%{splitFilters[2]}%");
                else if (splitFilters[0].Equals("TimePlaced"))
                    values.Add($"'{splitFilters[2]}'");
                else
                    values.Add(splitFilters[2]);
            }
            string cmdString = $"SELECT * FROM Sales WHERE {fields[0]} {signs[0]} {values[0]}";
            if (fields.Count > 1)
            {
                for (int i = 1; i < signs.Count; i++)
                {
                    cmdString += $" AND {fields[i]} {signs[i]} {values[i]}";
                }
            }
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmdString, sqlConnection);

            DataSet ds = new DataSet();
            dataAdapter.Fill(ds);

            return ds.Tables[0];
        }
        public bool UpdateSaleInfo(string update)
        {
            //id#ItemName#Price#Category#Quantity
            // 0 = ID
            // 1 = TotalPrice
            // 2 = Time
            var splitUpdate = update.Split('#');

            string id = splitUpdate[0];
            string totalPrice = splitUpdate[1];
            string time = splitUpdate[2];

            bool success = false;
            string cmdString = $"UPDATE Sales SET TotalPrice = {totalPrice}, TimePlaced = {time} WHERE ItemID = @id";
            SqlCommand sqlCommand = new SqlCommand
            {
                Connection = sqlConnection,
                CommandText = cmdString
            };
            sqlCommand.Parameters.AddWithValue("@id", id);
            try
            {
                int rows = sqlCommand.ExecuteNonQuery();
                if (rows > 0)
                {
                    success = true;
                }
            }
            catch (SqlException e)
            {
                MessageBox.Show(e.Message);
            }
            return success;
        }
    }
}