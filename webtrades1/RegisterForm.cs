using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace webtrades1
{
    public partial class RegisterForm : Form
    {
        webcontext db;
        public RegisterForm()//Загрузка данных из бд
        {
            InitializeComponent();
            
            db = new webcontext();
            db.Roles.Load();//Загружаем данные по всем ролям, товарам, пользователям из бд
            db.Items.Load();
            db.People.Load();
            Check();//Проверка при загрузке формы
        }

        private void textBox1_TextChanged(object sender, EventArgs e)//При изменении поля логина делаем проверку
        {
            Check();
        }
        private void Check()//Проверка введенных данных
        {
            if (textBox1.Text.Length < 5 || textBox1.Text.Length > 20)//проверка логина
            {
                textBox3.Text = "Введите логин: от 5 до 20 символов";
                textBox3.Visible = true;
                button1.Enabled = false;
            }
            else
            {
                Person item = new Person();
                item = db.People.FirstOrDefault(u => u.Login == textBox1.Text);
                if (item != null)//Если был найден с таким логином
                {
                    textBox3.Text = "Пользователь с таким логином уже существует";
                    textBox3.Visible = true;
                    button1.Enabled = false;
                }
                else
                {
                    if (textBox2.Text.Length < 7 || textBox2.Text.Length > 20)//Проверка пароля
                    {
                        textBox3.Text = "Введите пароль: от 7 до 20 символов";
                        textBox3.Visible = true;
                        button1.Enabled = false;
                    }
                    else if (textBox4.Text != textBox2.Text || String.IsNullOrEmpty(textBox4.Text))//проверка введенного и подтверждающего пароля
                    {
                        textBox3.Text = "Пароли не совпадают";
                        textBox3.Visible = true;
                        button1.Enabled = false;

                    }
                    else
                    {
                        textBox3.Visible = false;
                        button1.Enabled = true;
                    }
                }
            }
        }

        private void textBox2_TextChanged(object sender, EventArgs e)//При изменении поля пароля делаем проверку
        {
            Check();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)//При изменении подтверждающего поля делаем проверку
        {
            Check();
        }

        private void RegisterForm_Load(object sender, EventArgs e)
        {

        }
    }
}
