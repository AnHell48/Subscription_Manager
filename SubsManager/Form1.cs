using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Data.SqlServerCe;
using System.Data.SqlClient; 

namespace SubsManager
{
    /*----------------------------
     * By: 
     *      Angel A. Robles
     *      26/may/2018
     *----------------------------*/
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
           // LoadFile();
            OpenDataBase();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //adjust column headers to fit it's content
            dataGridView1.AutoResizeColumns(DataGridViewAutoSizeColumnsMode.ColumnHeader);
            dataGridView1.ClearSelection();
            VerifyExpired();
        }

        private void OpenDataBase()
        {
            try
            {
                SqlCeConnection connect = new SqlCeConnection(@"Data Source=|DataDirectory|\SubsDB.sdf");
                connect.Open();
                FillDatagrid(connect);
                connect.Close();
            }
            catch (SqlCeException e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        private void FillDatagrid(SqlCeConnection connection)
        {
            SqlCeDataAdapter adapt = new SqlCeDataAdapter("SELECT UserName,SubscriptionTime,StartDate,EndDate  FROM SubscribersTBL ORDER BY EndDate ASC", connection);
           
            DataTable table = new DataTable();
            adapt.Fill(table);
            dataGridView1.DataSource = table; 
        }

        private void button1_Click(object sender, EventArgs e) // Addbtn
        {
            int months;
            string userName,tempUser;
            DateTime expireDate = new DateTime();
            DateTime todaysDate = DateTime.Today;
            bool exist = false;

            userName = textBox1.Text;
            months = (int)motnhsSelectUpDown.Value;
            expireDate = dateTimePicker1.Value.AddMonths(months);
            expireDate.ToShortDateString();

            // to validate it exist with the toLower()
            tempUser = userName;

            for (int x = 0; x != dataGridView1.RowCount-1;x++ )
            {
                if (tempUser.ToLower() == dataGridView1.Rows[x].Cells[0].Value.ToString().ToLower())
                {
                    exist = true;
                    break;
                }                               
            }

            if (!exist)
            {
                AddToDB(userName, months, dateTimePicker1.Value.Date, expireDate.Date);
            }
            else
            {
                // ask if want to update user subcription 
                DialogResult input = MessageBox.Show("User already exist.\nWould you like to update\"" + userName + "\"?", "Update user?", MessageBoxButtons.YesNo);
               
                // if yes then update the months
                if (input == DialogResult.Yes)
                    UpdateSubcription(userName,months);
            }
        }

        private void VerifyExpired()
        {
            DateTime  todaysDate = DateTime.Today.Date;

            for (int rows = 0; rows != dataGridView1.RowCount-1; rows++)
            {
                //get the row date
                DateTime date = (DateTime)dataGridView1.Rows[rows].Cells[3].Value;

                // verify by year first. If the year is less then the current then it expire already
                if(todaysDate.Year > date.Year)
                {
                    dataGridView1.Rows[rows].DefaultCellStyle.BackColor = Color.PaleVioletRed;
                }
                // verify by month, if expired month pass already then sub ended.
                else if (todaysDate.Month > date.Month)
                {
                    //if expired, paint the row of certain color. 
                    if (date.Date == date.Date )
                            dataGridView1.Rows[rows].DefaultCellStyle.BackColor = Color.PaleVioletRed;
                    
                }
                // if it's the same month we are verifying then check if the day passed
                else if (todaysDate.Month == date.Month)
                {
                    if (todaysDate.Day > date.Day)
                    {
                        if (date.Date == date.Date)
                              dataGridView1.Rows[rows].DefaultCellStyle.BackColor = Color.PaleVioletRed;                        
                    }
                }
            }
        }

        private void AddToDB(string username, int months, DateTime start, DateTime expire)
        {
            try
            {
                SqlCeConnection connect = new SqlCeConnection(@"Data Source=|DataDirectory|\SubsDB.sdf");
                connect.Open();
                SqlCeDataAdapter adapt = new SqlCeDataAdapter();
                
                SqlCeCommand query = new SqlCeCommand("INSERT INTO SubscribersTBL(UserName,SubscriptionTime,StartDate,EndDate) VALUES(@username,@months,@start,@expire)",connect);
                query.Parameters.AddWithValue("@username", username);
                query.Parameters.AddWithValue("@months", months);
                query.Parameters.AddWithValue("@start", start);
                query.Parameters.AddWithValue("@expire", expire);
                query.ExecuteNonQuery();

                FillDatagrid(connect);
                connect.Close();
            }
            catch (SqlCeException e)
            {
                MessageBox.Show(e.ToString());
            }
            // after connected to DB verify which subscription had expire.
            VerifyExpired();
        }


        private void UpdateSubcription(string username,int newsubtime)
        {
            int newtime = 0;
            DateTime newExpdt = new DateTime();
            try
            {
                SqlCeConnection connect = new SqlCeConnection(@"Data Source=|DataDirectory|\SubsDB.sdf");
                connect.Open();
                SqlCeDataAdapter adapt = new SqlCeDataAdapter();
                SqlCeDataReader sqlreader;
                 
                SqlCeCommand query = new SqlCeCommand("SELECT SubscriptionTime,EndDate FROM SubscribersTBL WHERE UserName = @username", connect);
                query.Parameters.AddWithValue("@username", username);
                query.ExecuteNonQuery();

                sqlreader = query.ExecuteReader();
                while (sqlreader.Read())
                {
                    newtime = (int)sqlreader[0] ;
                    newExpdt = (DateTime)sqlreader[1];
                }

                newExpdt = newExpdt.AddMonths(newsubtime);
                newtime = newtime + newsubtime;

                query = new SqlCeCommand("UPDATE SubscribersTBL SET SubscriptionTime = @newtime,EndDate = @newExpdt WHERE UserName = @username", connect);
                query.Parameters.AddWithValue("@username", username);
                query.Parameters.AddWithValue("@newtime", newtime);
                query.Parameters.AddWithValue("@newExpdt", newExpdt);

                query.ExecuteNonQuery();

                FillDatagrid(connect);
                connect.Close();
            }
            catch (SqlCeException ex)
            {
                MessageBox.Show(ex.ToString());
            }
            VerifyExpired();
        }

        private void deleteBTN_Click(object sender, EventArgs e)
        {
            // get username of row selected
            string username = dataGridView1.SelectedCells[0].Value.ToString(); 

            //ask the user to confirm deletion
            DialogResult userInput = MessageBox.Show("Are you sure you want to delete \"" + username + "\" ?","Delete", MessageBoxButtons.YesNo);
            if (userInput == DialogResult.Yes)
            {
                try
                {
                    SqlCeConnection connect = new SqlCeConnection(@"Data Source=|DataDirectory|\SubsDB.sdf");
                    connect.Open();
                    SqlCeDataAdapter adapt = new SqlCeDataAdapter();

                    SqlCeCommand query = new SqlCeCommand("DELETE FROM SubscribersTBL WHERE UserName = @username", connect);
                    query.Parameters.AddWithValue("@username", username);          
                    query.ExecuteNonQuery();

                    FillDatagrid(connect);
                    connect.Close();
                }
                catch (SqlCeException ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            VerifyExpired();
        }

        public void PopUpUpdateform(string user,int newmonths)
        {
            if(user == "" || user == null)
                user = dataGridView1.SelectedCells[0].Value.ToString();
            UpdateSubcription(user, newmonths);
        }

        private void updateBTN_Click(object sender, EventArgs e)
        {
            PopUpdateForm popF2 = new PopUpdateForm();
            // if the textbox is empty, verify if something is selected
            //if nothing is selectec then it will get the first user on the list. 
            if (textBox1.Text == "" || textBox1.Text == null)
            {
                if ( dataGridView1.SelectedCells.Count <= 0)
                    popF2.Getusername(dataGridView1.Rows[0].Cells[0].Value.ToString());
                else
                    popF2.Getusername(dataGridView1.SelectedCells[0].Value.ToString());

            }                
            else
            {
                popF2.Getusername(textBox1.Text);
            }
               
            popF2.Show();
        }

        private void button1_Click_1(object sender, EventArgs e) // refreshBTN
        {
            try
            {
                SqlCeConnection connect = new SqlCeConnection(@"Data Source=|DataDirectory|\SubsDB.sdf");
                connect.Open();
                FillDatagrid(connect);
                connect.Close();
            }
            catch (SqlCeException exc)
            {
                MessageBox.Show(exc.ToString());
            }
            VerifyExpired();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SearchUser();
            }
        }

        private void searchBTN_Click(object sender, EventArgs e)
        {
            SearchUser();
        }

        private void SearchUser()
        {
            dataGridView1.ClearSelection();

            for (int row = 0; row != dataGridView1.RowCount - 1; row++)
            {
                if (dataGridView1.Rows[row].Cells[0].Value.ToString().ToLower().Contains(textBox1.Text))
                {
                    dataGridView1.Rows[row].Selected = true;
                    dataGridView1.FirstDisplayedScrollingRowIndex = row;
                }
            }
        }

        private void label5_MouseClick(object sender, MouseEventArgs e)
        {
            MessageBox.Show("Subscription Manager:\n + Rows marked in red means they expire.\n + You can add,search,update and delete subscribers.\n+ When updating, you can select the person and click input button or input the name on the box and click input. \n+ Refresh button is to update the list in case a change don't appear. \n\n \t\t By: Angel A. Robles\n\t\t\t 2018");
        }

    }
}
