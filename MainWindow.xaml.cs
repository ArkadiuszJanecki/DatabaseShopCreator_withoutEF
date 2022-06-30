using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Data.SqlTypes;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Drawing;
using System.Diagnostics;

namespace Database_Shop_Creator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        public DataTable dataTable = new DataTable();
        private void Exit_App(object sender, RoutedEventArgs e)
        {
            this.Close();
        }//wylaczanie

        private void Import_Database(object sender, RoutedEventArgs e)
        {
            HardResetCategories();
            string connString = @"Server = localhost; Database = master; Trusted_Connection = True;";
            //autowczytywanie bazy autostart

            try
            {
                //sql connection object
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"SELECT Id_Category,Parent,Name FROM Category ORDER BY Parent";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dataTable);
                    dataTable.Columns.Add("Added", typeof(bool));
                    foreach (DataRow row in dataTable.Rows)//dodawanie przyciskow kategorii do stackpanelu
                    {
                        if (row["Parent"].ToString() == "")
                        {
                            string name = row["ID_Category"].ToString();
                            string header = row["Name"].ToString();
                            Button newButton = new Button();
                            newButton.Height = 30;
                            newButton.Content = header;
                            newButton.Name = name;
                            newButton.Click += new RoutedEventHandler(button_Click);
                            CategoryList.Children.Add(newButton);
                            row["Added"] = false;
                        }
                        else
                        {
                            row["Added"] = false;
                        }
                    }
                    dataGrid1.DataContext = dataTable.DefaultView;
                    conn.Close();
                    da.Dispose();
                }

            }

            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

        }//import bazy

        void button_Click(object sender, System.EventArgs e)
        {
            SoftResetCategories();
            Button button = sender as Button;
            string name = button.Name;
            StackPanel SubPanel = new StackPanel()
            {
                Name = "panel_" + name
            };
            Subcategories.Children.Add(SubPanel);
            //dodanie buttona pojedynczej podkategorii ktora bedzie miala podkategorie pod sobą

            for (int i = 0; i < 3; i++)
            {
                //dodanie jednej podkategorii

                foreach (DataRow ParentRow in dataTable.Rows)
                {
                    if ((button.Name == ParentRow["Parent"].ToString()) && ParentRow["Added"].Equals(false))
                    {
                        Button subCategory = new Button();
                        subCategory.Name = ParentRow["Id_Category"].ToString();
                        subCategory.Content = ParentRow["Name"].ToString();
                        subCategory.Height = 35;
                        subCategory.Click += new RoutedEventHandler(button_goToCategory);
                        SubPanel.Children.Add(subCategory);
                        ParentRow["Added"] = true;
                        break;
                    }
                }
                //dodanie wszystkich podpodkategorii pod button wyzej


                foreach (DataRow ParentRow in dataTable.Rows)
                {
                    foreach (Button btn in SubPanel.Children)
                    {
                        if ((btn.Name == ParentRow["Parent"].ToString()) && ParentRow["Added"].Equals(false))
                        {
                            Button specCategory = new Button();
                            specCategory.Name = ParentRow["Id_Category"].ToString();
                            specCategory.Content = ParentRow["Name"].ToString();
                            specCategory.Height = 20;
                            specCategory.Click += new RoutedEventHandler(button_goToCategory);
                            SubPanel.Children.Add(specCategory);
                            ParentRow["Added"] = true;
                            break;
                        }
                    }
                }
            }

        }//przyciski kategorii

        void button_goToCategory(object sender, System.EventArgs e)//konkretne wyszykiwanie produktow po kategorii
        {
            Button button = sender as Button;
            string name = button.Name;
            DataTable allProductTable = new DataTable();
            string connString = @"Server = localhost; Database = master; Trusted_Connection = True;";
            try
            {
                //sql connection object
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = "SELECT * FROM dbo.Product as p INNER JOIN dbo.ProductWithCategory" +
                        " as pwc ON p.ID_Product = pwc.ID_Product WHERE ID_Category = '" + name + "'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(allProductTable);
                    dataGrid2.DataContext = allProductTable.DefaultView;
                    conn.Close();
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

        void HardResetCategories()
        {
            dataTable.Reset();
            CategoryList.Children.Clear();
            Subcategories.Children.Clear();
        }//do resetowania 
        void SoftResetCategories()
        {
            Subcategories.Children.Clear();
            foreach (DataRow row in dataTable.Rows)
            {
                row["Added"] = false;
            }
        }//do resetowania
        void AddSampleProducts(string Name, string Desc, float Weight, float Height, float Width, float Depth, int ProdID, string ProdName, string Warranty)
        {
            //image
            string filePath = @"D:\ogor.jpg";
            FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            BinaryReader reader = new BinaryReader(stream);
            byte[] photo = reader.ReadBytes((int)stream.Length);
            reader.Close();
            stream.Close();
            //image
            string connString = @"Server = localhost; Database = makro; Trusted_Connection = True;";
            //sprawdzenie ostatniego ID by dynamicznie dodawac produkty
            Int32 count = 0;
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Product", conn);
                    count = (Int32)cmd.ExecuteScalar();
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }

            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    string query = @"SELECT COUNT(*) FROM Product";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT into makro.dbo.Product (ID_Product,Name,Description,Weight,Height,Width,Depth,Producent_ID,Producent_Name,Warranty,Image)" +
                        "VALUES(@ID_Product, @Name, @Description, @Weight, @Height, @Width, @Depth, @Producent_ID, @Producent_Name, @Warranty, @Image)";
                    cmd.Parameters.AddWithValue("@ID_Product", count);
                    cmd.Parameters.AddWithValue("@Name", Name);
                    cmd.Parameters.AddWithValue("@Description", Desc);
                    cmd.Parameters.AddWithValue("@Weight", Weight);
                    cmd.Parameters.AddWithValue("@Height", Height);
                    cmd.Parameters.AddWithValue("@Width", Width);
                    cmd.Parameters.AddWithValue("@Depth", Depth);
                    cmd.Parameters.AddWithValue("@Producent_ID", ProdID);
                    cmd.Parameters.AddWithValue("@Producent_Name", ProdName);
                    cmd.Parameters.AddWithValue("@Warranty", Warranty);
                    cmd.Parameters.AddWithValue("@Image", photo);
                    cmd.ExecuteNonQuery();
                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }//dodawanie produktow


        private void Window_Initialized(object sender, EventArgs e)
        {
            HardResetCategories();
            string connString = @"Server = localhost; Database = master; Trusted_Connection = True;";
            //autowczytywanie bazy autostart
            try
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"SELECT Id_Category,Parent,Name FROM Category ORDER BY Parent";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(dataTable);
                    dataTable.Columns.Add("Added", typeof(bool));
                    foreach (DataRow row in dataTable.Rows)
                    {
                        if (row["Parent"].ToString() == "")
                        {
                            string name = row["ID_Category"].ToString();
                            string header = row["Name"].ToString();
                            Button newButton = new Button();
                            newButton.Height = 30;
                            newButton.Content = header;
                            newButton.Name = name;
                            newButton.Click += new RoutedEventHandler(button_Click);
                            CategoryList.Children.Add(newButton);
                            row["Added"] = false;
                        }
                        else
                        {
                            row["Added"] = false;
                        }
                    }
                    dataGrid1.DataContext = dataTable.DefaultView;
                    conn.Close();
                    da.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
            //for (int i = 0; i < 2000; i++)
            //{
            //    AddSampleProducts("Ogorek zielony" + i, "Swiezy ", i, i, i, i, i, "Polands", "4 dni ");
            //}


        }//po wczytaniu okna

        private void ClickSearch(object sender, RoutedEventArgs e)
        {
            string searchProduct = SearchBar.Text;
            DataTable allProductTable = new DataTable();
            string connString = @"Server = localhost; Database = master; Trusted_Connection = True;";
            try
            {
                var watch = Stopwatch.StartNew();
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    string query = @"SELECT * FROM dbo.Product WHERE Name Like'%" + searchProduct + "%'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(allProductTable);
                    dataGrid2.DataContext = allProductTable.DefaultView;
                    conn.Close();
                }
                watch.Stop();
                var elapsedTime = watch.ElapsedMilliseconds;
                MessageBox.Show(elapsedTime.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }//wyszukiwanie

        private void Add_Category(object sender, RoutedEventArgs e)
        {

        }//to do

        private void Add_Product(object sender, RoutedEventArgs e)
        {

        }//to do

        private void ShowData(object sender, MouseButtonEventArgs e)//wyswietlanie danego przedmiotu
        {
            DataRowView row = dataGrid2.SelectedItem as DataRowView;
            LabelID.Content = row[0];
            LabelName.Content = row[1];
            LabelDesc.Content = row[2];
            LabelDims.Content = row[3] + "x" + row[4] + "x" + row[5] + "x" + row[6];
            LabelProdID.Content = row[7];
            LabelProdName.Content = row[8];
            LabelWarr.Content = row[9];
            try
            {
                byte[] photo = (byte[])row[10];
                System.Drawing.Image img = null;
                BitmapImage bi = new BitmapImage();
                using (MemoryStream ImageDataStream = new MemoryStream())
                {
                    ImageDataStream.Write(photo, 0, photo.Length);
                    ImageDataStream.Position = 0;
                    photo = System.Text.UnicodeEncoding.Convert(Encoding.Unicode, Encoding.Default, photo);
                    img = System.Drawing.Image.FromStream(ImageDataStream);

                    bi.BeginInit();
                    MemoryStream ms = new MemoryStream();
                    img.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                    ms.Seek(0, SeekOrigin.Begin);
                    bi.StreamSource = ms;
                    bi.EndInit();

                }
                ProductImage.Source = bi;
            }
            catch (Exception ex)
            {
                ProductImage.Source = null;
                Console.WriteLine(ex);
            }
        }


        private void AdvanceSearch(object sender, RoutedEventArgs e)
        {
            DataTable allProductTable = new DataTable();
            string connString = @"Server = localhost; Database = master; Trusted_Connection = True;";

           /* string query = "SELECT * FROM dbo.Product as p INNER JOIN dbo.ProductWithCategory" +" as pwc ON p.ID_Product = pwc.ID_Product WHERE ID_Category = '" + name + "'"; */
            StringBuilder sb = new StringBuilder("SELECT * FROM dbo.Product as p ");
            var watch = Stopwatch.StartNew();
            string query = 
                "SELECT * FROM dbo.Product as p " +
                "INNER JOIN dbo.Price AS pc ON p.ID_Product = pc.ID_Product " +
                "INNER JOIN dbo.StockStatus AS ss ON p.ID_Product = ss.ID_Product " +
                "INNER JOIN dbo.Promotion AS pro ON p.ID_Product = pro.ID_Product " +
                "INNER JOIN dbo.Opinion AS op ON p.ID_Product = op.ID_Product " +
                "WHERE pc.Price > '10' AND ss.Stock_Status >'1' " +
                "AND pro.Price_Percent >'10' AND p.Warranty IS NOT NULL AND op.Rating >'1'";


            //if (PriceCB.IsChecked == true)//cena
            //{
            //    sb.Append("INNER JOIN dbo.Price AS pc ON p.ID_Product = pc.ID_Product ");
            //}
            //if (OpinionCB.IsChecked == true)//rating
            //{
            //    sb.Append("INNER JOIN dbo.ProductWithOpinion AS pwo ON p.ID_Product = pwo.ID_Product"+" "+" \r\n INNER JOIN dbo.Opinion AS po ON pwo.ID_Opinion = po.ID_Opinion ");
            //}
            //if (CategoryCB.IsChecked == true)//kategoria
            //{
            //    sb.Append("INNER JOIN dbo.ProductWithCategory AS pwc ON p.ID_Product = pwc.ID_Product" + " " + " \r\n INNER JOIN dbo.Category AS pcat ON pwc.ID_Category = pcat.ID_Category");
            //}
            //if (sb.ToString().Contains("WHERE"))
            //{

            //}
            //else
            //{
            //    sb.Append(" WHERE ");
            //}
            //if (NameCB.IsChecked == true)//nazwa
            //{
              
            //}
            try
            {
                //sql connection object
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    SqlCommand cmd = new SqlCommand(query.ToString(), conn); //tu ewentualnie zmienic na sb.ToString()
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    da.Fill(allProductTable);
                    dataGrid2.DataContext = allProductTable.DefaultView;
                    conn.Close();
                }
                watch.Stop();
                var elapsedTime = watch.ElapsedMilliseconds;
                MessageBox.Show(elapsedTime.ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception: " + ex.Message);
            }
        }

    }
}
